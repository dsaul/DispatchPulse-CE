using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SharedCode.DatabaseSchemas;
using Npgsql;
using SharedCode.ARI;
using Twilio.Rest.Api.V2010.Account;
using Serilog;
using FluentEmail.Core.Models;
using FluentEmail.Core;
using System.IO;
using ARI_OnCall.Properties;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Text;
using System.Globalization;
using SharedCode;

namespace ARI.IVR.OnCall
{
	public static partial class OnCallPostMessageHandler
	{
		

		static void RunMessage(NpgsqlConnection billingDB, NpgsqlConnection dpDB, Voicemails message, Guid billingCompanyId, string databaseName) {

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)) {
				
				Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} ARI_TO_PBX_SSH_IDRSA_FILE not set", message.Id, billingCompanyId, databaseName);
				return;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_FQDN)) {
				Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} PBX_FQDN not set", message.Id, billingCompanyId, databaseName);
				return;
			}
			if (null == SharedCode.ARI.Konstants.PBX_SSH_PORT) {
				Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} PBX_SSH_PORT not set", message.Id, billingCompanyId, databaseName);
				return;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_SSH_USER)) {
				Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} PBX_SSH_USER not set", message.Id, billingCompanyId, databaseName);
				return;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY)) {
				Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY not set", message.Id, billingCompanyId, databaseName);
				return;
			}


			if (string.IsNullOrWhiteSpace(EnvTwilio.TWILIO_AUTH_TOKEN)) {
				Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} TWILIO_AUTH_TOKEN not set!", message.Id, billingCompanyId, databaseName);
				return;
			}

			if (string.IsNullOrWhiteSpace(EnvTwilio.TWILIO_ACCOUNT_SID)) {
				Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} TWILIO_ACCOUNT_SID not set!", message.Id, billingCompanyId, databaseName);
				return;
			}

			if (null != message.OnCallAttemptsFinished && message.OnCallAttemptsFinished.Value) {
				return;
			}

			if (null != message.IsMarkedHandled && message.IsMarkedHandled.Value) {
				return;
			}
			


			var resBC = BillingCompanies.ForIds(billingDB, billingCompanyId);
			if (0 == resBC.Count) {
				Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} 0 == resBC.Count", message.Id, billingCompanyId, databaseName);
				return;
			}

			BillingCompanies billingCompany = resBC.FirstOrDefault().Value;


			// Get a link to the recording for Twilio
			string messageRecordingUri = S3Utils.GetPreSignedUrl(
				EnvAmazonS3.S3_PBX_ACCESS_KEY,
				EnvAmazonS3.S3_PBX_SECRET_KEY,
				EnvAmazonS3.S3_PBX_SERVICE_URI,
				message.RecordingS3Bucket,
				message.RecordingS3Key,
				"audio/vnd.wave",
				$"{message.MessageLeftAtISO8601}-{message.CallerIdNumber}-{message.CallbackNumber}.wav");


			// Go through the attempts
			bool madeAttemptToContact = false;

			for (int i=0; i < message.OnCallAttemptsProgress.Count; i++) {
				Voicemails.CallAttemptsProgressEntry entry = message.OnCallAttemptsProgress[i];
				if (null != entry.GivenUp && entry.GivenUp.Value)
					continue;

				// Make sure we have a calendar id
				if (null == entry.CalendarId) {
					Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} null == entry.CalendarId", message.Id, billingCompanyId, databaseName);
					message = message.GiveUpOnAttemptAtIndex(dpDB, i, "We couldn't find the calendar (Error #1).") ?? message;
					continue;
				}

				// If we've used up all the attempts, stop trying.
				if (entry.CallAttempts >= entry.CallAttemptsMax) {
					Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} entry.CallAttempts >= entry.CallAttemptsMax", message.Id, billingCompanyId, databaseName);
					message = message.GiveUpOnAttemptAtIndex(dpDB, i, "Reached max attempts for the calendar.") ?? message;
					continue;
				}

				// If it isn't time to try next, then don't.
				DateTime? now = DateTime.Now;
				DateTime? nextAttemptAfterDt;
				if (string.IsNullOrWhiteSpace(message.NextAttemptAfterISO8601)) {
					nextAttemptAfterDt = DateTime.UtcNow.AddMinutes(-1);
				} else {
					nextAttemptAfterDt = DateTime.Parse(message.NextAttemptAfterISO8601);
				}
				if (now < nextAttemptAfterDt) {
					Log.Information("VoicemailId:{VoicemailId}, BillingCompanyId:{BillingCompanyId}, Database:{Database} Not time yet.", message.Id, billingCompanyId, databaseName);
					return; // we don't want to trigger the final stuff by breaking
				}

				// Get the calendar info.
				Guid calendarId = entry.CalendarId.Value;
				
				var resCal = Calendars.ForId(dpDB, calendarId);
				if (0 == resCal.Count) {
					Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} 0 == resCal.Count", message.Id, billingCompanyId, databaseName);
					message = message.GiveUpOnAttemptAtIndex(dpDB, i, "We couldn't find the calendar (Error #2).") ?? message;
					continue;
				}

				Calendars cal = resCal.FirstOrDefault().Value;
				
				// Extract the numbers from the calendars.
				HashSet<CalendarOnCallPhoneNumber> pns = cal.OnCallPhoneNumbersRightNow;
				if (0 == pns.Count) {
					Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} 0 == pns.Count", message.Id, billingCompanyId, databaseName);
					message = message.GiveUpOnAttemptAtIndex(dpDB, i, "We weren't able to get any phone numbers out of the on call calendar!") ?? message;
					continue;
				}

				// Extract the emails from the calendars.
				if (null == entry.SentEMail || (null != entry.SentEMail && false == entry.SentEMail.Value)) {

					HashSet<string> emails = cal.OnCallEMailsRightNow;
					if (0 == emails.Count) {
						Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} 0 == emails.Count", message.Id, billingCompanyId, databaseName);

						// Sent MMS to description.
						message = message.MarkSentEMailAtIndex(dpDB, i, "No e-mail addresses in description to send to.", true) ?? message;
					} else {

						// Get S3 File for attachment.

						string? key = EnvAmazonS3.S3_PBX_ACCESS_KEY;
						string? secret = EnvAmazonS3.S3_PBX_SECRET_KEY;

						using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
						{
							RegionEndpoint = RegionEndpoint.USWest1,
							ServiceURL = EnvAmazonS3.S3_PBX_SERVICE_URI,
							ForcePathStyle = true
						});

						GetObjectRequest request = new GetObjectRequest
						{
							BucketName = message.RecordingS3Bucket,
							Key = message.RecordingS3Key,
						};

						using GetObjectResponse s3Response = s3Client.GetObjectAsync(request).Result;

						// Send a notification email to the above emails.

						foreach (string email in emails) {

							JObject additionalData = new JObject();
							additionalData["description"] = email;
							string additionalDataStr = additionalData.ToString();
							byte[] additionalDataBytes = Encoding.UTF8.GetBytes(additionalDataStr);
							string additionalDataBase64 = Convert.ToBase64String(additionalDataBytes);

							using Stream s3ResponseStream = s3Response.ResponseStream;

							string template = Resources.NewOnCallVoicemailNotificationEmailTemplate;

							string? dateString = null;
							
							if (null != message.MessageLeftAtISO8601) {
								ParseResult<Instant> result = InstantPattern.ExtendedIso.Parse(message.MessageLeftAtISO8601);
								if (result.Success) {
									Instant instant = result.Value;
									DateTimeZone? timeZone = null;
									if (!string.IsNullOrWhiteSpace(billingCompany.IANATimezone)) {
										timeZone = DateTimeZoneProviders.Tzdb[billingCompany.IANATimezone];
									}
									if (null == timeZone) {
										timeZone = DateTimeZoneProviders.Tzdb[BillingCompanies.kJsonValueIANATimezoneDefault];
									}
									ZonedDateTime dtWpg = instant.InZone(timeZone);
									dateString = dtWpg.ToString(@"h:mm tt \o\n dddd MMMM d yyyy", new CultureInfo("en-CA"));
								}
							}
							

							SendResponse? response = Email
								.From(EnvOnCallResponder.ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS, "On Call Responder")
								.To(email)
								.Subject("On Call Message")
								.UsingTemplate(template, new {
									MessageId = message.Id,
									message.CallerIdName,
									message.CallerIdNumber,
									message.CallbackNumber,
									BillingCompanyId = billingCompanyId,
									BillingCompanyName = billingCompany.FullName,
									DateString = dateString,
									AdditionalDataBase64 = additionalDataBase64,
									ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI = EnvOnCallResponder.ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI
								})
								.Attach(new Attachment{
									Data = s3ResponseStream,
									Filename = "Recording.wav",
									ContentType = "audio/vnd.wav"
								})
								.Send();

							Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} Sent email to {EMailAddress}", message.Id, billingCompanyId, databaseName, email);
						}

						message = message.MarkSentEMailAtIndex(dpDB, i, $"Sent e-mail to {string.Join(", ", emails)}", false) ?? message;


					}
				}
				


				// Create the channel information for ast
				List<string> astChannels = new List<string>();
				List<string> justNumbersCalled = new List<string>();
				foreach (CalendarOnCallPhoneNumber pn in pns) {
					astChannels.Add($"Local/{pn.Number}@{kAstOutboundCallContextPrimary}");
					justNumbersCalled.Add(pn.Number);
				}


				// Send a text message containing the voicemail information.
				if (null == entry.SentMMS || (null != entry.SentMMS && false == entry.SentMMS.Value)) {
					var mediaUrl = new [] {
						new Uri(messageRecordingUri)
					}.ToList();

					foreach (string pn in justNumbersCalled) {

						string smsPn = pn;

						if (smsPn.Length == 10) {
							smsPn = $"1{smsPn}";
						}
						if (smsPn.Length == 11) {
							smsPn = $"+{smsPn}";
						}

						JObject additionalData = new JObject();
						additionalData["description"] = $"MMS:{smsPn}";
						string additionalDataStr = additionalData.ToString();
						byte[] additionalDataBytes = Encoding.UTF8.GetBytes(additionalDataStr);
						string additionalDataBase64 = Convert.ToBase64String(additionalDataBytes);

						StringBuilder sb = new StringBuilder();
						sb.Append("Hello, there is a new on call message. It is attached to this message.\n\n");

						string? messageLeftAtISO8601 = message.MessageLeftAtISO8601;
						if (string.IsNullOrWhiteSpace(messageLeftAtISO8601)) {
							sb.Append("We don't have a time in the database this message was left.\n\n");
						} else {
							string? dateString = null;

							ParseResult<Instant> result = InstantPattern.ExtendedIso.Parse(messageLeftAtISO8601);
							if (result.Success) {
								Instant instant = result.Value;
								DateTimeZone? timeZone = null;
								if (!string.IsNullOrWhiteSpace(billingCompany.IANATimezone)) {
									timeZone = DateTimeZoneProviders.Tzdb[billingCompany.IANATimezone];
								}
								if (null == timeZone) {
									timeZone = DateTimeZoneProviders.Tzdb[BillingCompanies.kJsonValueIANATimezoneDefault];
								}
								ZonedDateTime dtWpg = instant.InZone(timeZone);
								dateString = dtWpg.ToString(@"h:mm tt \o\n dddd MMMM d yyyy", new CultureInfo("en-CA"));
							}

							sb.Append($"The message was received at {dateString}.\n\n");
						}

						if (string.IsNullOrWhiteSpace(message.CallerIdName) && string.IsNullOrWhiteSpace(message.CallerIdNumber)) {
							sb.Append("There is no caller id information provided with this call.\n\n");
						} else {
							sb.Append("Caller Id Information: \n");
							if (!string.IsNullOrWhiteSpace(message.CallerIdName)) {
								sb.Append($"Name: {message.CallerIdName}\n");
							}
							if (!string.IsNullOrWhiteSpace(message.CallerIdNumber)) {
								sb.Append($"Number: {message.CallerIdNumber}");
							}
							sb.Append("\n\n");
						}

						if (string.IsNullOrWhiteSpace(message.CallbackNumber)) {
							sb.Append("There is no callback number information provided with this call.\n\n");
						} else {
							sb.Append($"The callback number is {message.CallbackNumber}.\n\n");
						}

						sb.Append($"Please go to the following link to mark this message as handled {EnvOnCallResponder.ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI}{billingCompanyId}/{message.Id}/{additionalDataBase64} .\n\n");

						sb.Append($"If you are not a part of this company and you wish to stop recieving these messages, " +
							$"please reply with STOP and you will not recieve any further messages. If you do this in error " +
							$"you can reply with START to begin recieving these messages again. \n\nIf this doesn't work, please " +
							$"contact support at {EnvOnCallResponder.ON_CALL_RESPONDER_SUPPORT_CONTACT_LOCATION} and we will manually remove your number.");

						MessageResource.Create(
							body: sb.ToString(),
							from: new Twilio.Types.PhoneNumber(EnvOnCallResponder.ON_CALL_RESPONDER_SMS_FROM_E164),
							to: new Twilio.Types.PhoneNumber(smsPn),
							mediaUrl: mediaUrl
						);

						Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} Sent MMS to {SMSPhoneNumber}", message.Id, billingCompanyId, databaseName, smsPn);

					}

					// Sent MMS to description.
					message = message.MarkSentMMSAtIndex(dpDB, i, string.Join(", ", justNumbersCalled)) ?? message;
				}


				if (null == message.Id) {
					Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} null == message.Id", message.Id, billingCompanyId, databaseName);
					return;
				}

				Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} Calling agent.", message.Id, billingCompanyId, databaseName);

				bool successfullySentAnyToPBX = false;

				List<Voicemails.CallFile> callFiles = new List<Voicemails.CallFile>();

				for (int j=0; j<astChannels.Count; j++) {

					string channel = astChannels[j];
					string displayNumber = justNumbersCalled[j];

					bool sent = SpooledCall.Call(
						astChannel: channel,
						context: kAstOnCallRepContext,
						extension: kAstOnCallRepContextExtension,
						callCategory: message.Id.Value.ToString(),
						callFileName: out string? callFileName,
						callFileContents: out string? callFileContents,
						callFilePrefix: kOnCallCallFilePrefix,
						variables: new Dictionary<string, string> {
							{ "billingCompanyId", billingCompanyId.ToString() },
							{ "voicemailId", message.Id.Value.ToString() },
							{ "databaseName", databaseName },
							{ "callWasTo", displayNumber },
							{ "isBackupTrunk", "false" }
						}
					);

					if (sent == true) {
						successfullySentAnyToPBX = true;

						Voicemails.CallFile callFile = new Voicemails.CallFile(
							FileName: callFileName,
							IsPBXDone: false,
							IsPBXError: false,
							HasSubmittedRetry: false,
							OriginalCallFile: callFileContents,
							ArchivedCallFile: null
							);
						callFiles.Add(callFile);
					}
				}
				
				if (!successfullySentAnyToPBX) {
					return;
				}


				message = message.MarkANewAttemptAtIndex(dpDB, i, string.Join(", ", justNumbersCalled), callFiles) ?? message;
				message = message.MarkNextAttemptTime(dpDB) ?? message;

				madeAttemptToContact = true;
				break;
			}

			// We weren't able to do any attempts. Mark as attempts finished.
			if (false == madeAttemptToContact && null != message.OnCallAttemptsFinished && false == message.OnCallAttemptsFinished.Value) {

				// Call noAgentResponseNotificationNumber and tell them that no agent was able to answer this vm

				// If it isn't time to try next, then don't.
				DateTime? now = DateTime.Now;
				DateTime? nextAttemptAfterDt;
				if (string.IsNullOrWhiteSpace(message.NextAttemptAfterISO8601)) {
					nextAttemptAfterDt = DateTime.UtcNow.AddMinutes(-1);
				} else {
					nextAttemptAfterDt = DateTime.Parse(message.NextAttemptAfterISO8601);
				}
				if (now < nextAttemptAfterDt) {
					Log.Information("VoicemailId:{VoicemailId}, BillingCompanyId:{BillingCompanyId}, Database:{Database} Not time yet.", 
						message.Id, billingCompanyId, databaseName);
					return; // we don't want to trigger the final stuff below
				}

				if (null == message.Id) {
					Log.Error("{VoicemailId}, {BillingCompanyId}, {Database} null == message.Id", message.Id, billingCompanyId, databaseName);
					return;
				}

				bool successfullySentToPBX = SpooledCall.Call(
					astChannel: $"Local/{message.NoAgentResponseNotificationNumber}@{kAstOutboundCallContextPrimary}",
					context: kAstOnCallRepContext,
					extension: kAstOnCallRepContextExtension,
					callCategory: message.Id.Value.ToString(),
					callFileName: out _,
					callFileContents: out _,
					callFilePrefix: kOnCallCallFilePrefix,
					variables: new Dictionary<string, string> {
						{ "billingCompanyId", billingCompanyId.ToString() },
						{ "voicemailId", message.Id.Value.ToString() },
						{ "databaseName", databaseName },
						{ "callWasTo", message.NoAgentResponseNotificationNumber ?? "" },
						{ "isBackupTrunk", "false" }
					}
				);

				if (!successfullySentToPBX) {
					return;
				}

				message = message.MarkFailed(dpDB) ?? message;

				if (!string.IsNullOrWhiteSpace(message.NoAgentResponseNotificationEMail)) {
					// Get S3 File for attachment.

					string? key = EnvAmazonS3.S3_PBX_ACCESS_KEY;
					string? secret = EnvAmazonS3.S3_PBX_SECRET_KEY;

					using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
						{
						RegionEndpoint = RegionEndpoint.USWest1,
						ServiceURL = EnvAmazonS3.S3_PBX_SERVICE_URI,
						ForcePathStyle = true
					});

					GetObjectRequest request = new GetObjectRequest
						{
						BucketName = message.RecordingS3Bucket,
						Key = message.RecordingS3Key,
					};

					using GetObjectResponse s3Response = s3Client.GetObjectAsync(request).Result;

					// Send notification email.

					JObject additionalData = new JObject();
					additionalData["description"] = message.NoAgentResponseNotificationEMail;
					string additionalDataStr = additionalData.ToString();
					byte[] additionalDataBytes = Encoding.UTF8.GetBytes(additionalDataStr);
					string additionalDataBase64 = Convert.ToBase64String(additionalDataBytes);

					using Stream s3ResponseStream = s3Response.ResponseStream;

					string template = Resources.NoAgentResponseNotificationEmailTemplate;

					string? dateString = null;

					if (null != message.MessageLeftAtISO8601) {
						ParseResult<Instant> result = InstantPattern.ExtendedIso.Parse(message.MessageLeftAtISO8601);
						if (result.Success) {
							Instant instant = result.Value;
							DateTimeZone? timeZone = null;
							if (!string.IsNullOrWhiteSpace(billingCompany.IANATimezone)) {
								timeZone = DateTimeZoneProviders.Tzdb[billingCompany.IANATimezone];
							}
							if (null == timeZone) {
								timeZone = DateTimeZoneProviders.Tzdb[BillingCompanies.kJsonValueIANATimezoneDefault];
							}
							ZonedDateTime dtWpg = instant.InZone(timeZone);
							dateString = dtWpg.ToString(@"h:mm tt \o\n dddd MMMM d yyyy", new CultureInfo("en-CA"));
						}
					}

					SendResponse? response = Email
					.From(EnvOnCallResponder.ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS, "On Call Responder")
					.To(message.NoAgentResponseNotificationEMail)
					.Subject("On Call Message (No Agent Response)")
					.UsingTemplate(template, new {
						MessageId = message.Id,
						message.CallerIdName,
						message.CallerIdNumber,
						message.CallbackNumber,
						BillingCompanyId = billingCompanyId,
						BillingCompanyName = billingCompany.FullName,
						DateString = dateString,
						AdditionalDataBase64 = additionalDataBase64,
						ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI = EnvOnCallResponder.ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI
					})
					.Attach(new Attachment{
						Data = s3ResponseStream,
						Filename = "Recording.wav",
						ContentType = "audio/vnd.wav"
					})
					.Send();

					Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} Sent email to {EMailAddress}", message.Id, billingCompanyId, databaseName, message.NoAgentResponseNotificationEMail);
				}
				























			}






		}

		


		


		

	}
}
