using System;
using ARI;
using AsterNET.FastAGI;
using Databases.Records.CRM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedCode;
using System.Linq;
using Amazon.Polly;
using Databases.Records.Billing;
using System.Collections.Generic;
using Renci.SshNet;
using SharedCode.Extensions;
using System.Text;
using Serilog;
using ARI_OnCall.Properties;
using NodaTime;
using NodaTime.Text;
using System.Globalization;

namespace ARI.IVR.OnCallRespondee
{
	public partial class EntryPoint : AGIScriptPlus
	{
		public override void Service(AGIRequest request, AGIChannel channel) {

			Log.Information("[{AGIRequestUniqueId}] OnCallRespondeeMenu New Call {CallerIdNumber} {CallerIdName}", request.UniqueId, request.CallerId, request.CallerIdName);


			using RespondeeCallRequestData respondeeRequestData = new RespondeeCallRequestData();

			Answer();
			SetAutoHangup(60 * 20); // These calls shouldn't take more than 20 minutes. Set auto hang up not waste too much money on dead channels.


			if (Guid.TryParse(GetFullVariable("${billingCompanyId}"), out Guid companyId))
				respondeeRequestData.BillingCompanyId = companyId;
			if (null == respondeeRequestData.BillingCompanyId)
				ThrowError(request, "4532", "null == data.BillingCompanyId");


			if (Guid.TryParse(GetFullVariable("${voicemailId}"), out Guid voicemailId))
				respondeeRequestData.VoicemailId = voicemailId;
			if (null == respondeeRequestData.VoicemailId)
				ThrowError(request, "123d", "null == data.VoicemailId");

			respondeeRequestData.DatabaseName = GetFullVariable("${databaseName}");
			if (string.IsNullOrWhiteSpace(respondeeRequestData.DatabaseName))
				ThrowError(request, "52ss", "null == data.DatabaseName");

			respondeeRequestData.CallWasTo = GetFullVariable("${callWasTo}");
			if (string.IsNullOrWhiteSpace(respondeeRequestData.CallWasTo))
				ThrowError(request, "2543", "null == data.CallWasTo");


			respondeeRequestData.ConnectToBillingDB();
			if (null == respondeeRequestData.BillingDB)
				ThrowError(request, "a532", "null == data.BillingDB");

			respondeeRequestData.ConnectToDPDBName(respondeeRequestData.DatabaseName);
			if (null == respondeeRequestData.DPDB)
				ThrowError(request, "222a", "null == data.DPDB");

			var resBC = BillingCompanies.ForIds(respondeeRequestData.BillingDB, respondeeRequestData.BillingCompanyId.Value);
			if (0 == resBC.Count) {
				ThrowError(request, "292a", "0 == resBC.Count");
			}

			respondeeRequestData.BillingCompany = resBC.FirstOrDefault().Value;

			var resVM = Voicemails.ForId(respondeeRequestData.DPDB, respondeeRequestData.VoicemailId.Value);
			if (0 == resVM.Count) {
				ThrowError(request, "221a", "0 == resVM.Count");
			}

			respondeeRequestData.Message = resVM.FirstOrDefault().Value;

			if (null == respondeeRequestData.Message.JsonObject)
				ThrowError(request, "311s", "null == data.Message.JsonObject");


			respondeeRequestData.AddTimelineEntry(
				type: OnCall.LeaveMessageRequestData.TimelineType.text,
				timestampISO8601: DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture),
				description: $"Respondee call to \"{request.CallerId}\" answered.",
				colour: "green");
			

			// Start main menu.
			char key = '\0';
			key = PlayTTS($"Hello, this is the on-call responder service for {respondeeRequestData.BillingCompany.FullName}. ", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);

			int i = 3;

			while ((--i) >= 0) {
				if (key == '\0') {
					key = PlayTTS($"A new on-call message has been left, press one " +
						$"to accept this message and mark this message as handled.", 
						kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
				}

				if (key == '\0') {
					key = WaitForDigit(5000);
				}

				switch (key) {
					case '1':
						key = '\0';

						respondeeRequestData.Message = respondeeRequestData.Message.MarkHandled(respondeeRequestData.DPDB, request.CallerId, Resources.MarkedHandledNotificationEmailTemplate, respondeeRequestData.BillingCompanyId.Value, respondeeRequestData.BillingCompany) ?? respondeeRequestData.Message;

						MessageMenu(request, channel, respondeeRequestData);
						throw new PerformHangupException();
					default:
						key = '\0';
						if (i != 0) {
							key = PlayTTS("That isn't a valid option, please try again.", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
						} else {
							key = PlayTTS("That isn't a valid option.", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
						}

						continue;
				}
			}

			PlayTTS("Unfortunately we weren't able to get a response, please try again later.", string.Empty, Engine.Neural, VoiceId.Brian);
			throw new PerformHangupException();
		}


		protected void MessageMenu(AGIRequest request, AGIChannel channel, RespondeeCallRequestData respondeeRequestData) {

			char key = '\0';

			if (key == '\0') {
				key = PlayTTS("Message Menu", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
			}

			int i = 3;

			while ((--i) >= 0) {
				if (key == '\0') {
					key = PlayTTS(
						$"Press 1 to play the date and time the message was received. " +
						$"Press 2 to play the caller id information. " +
						$"Press 3 to play the callback number. " +
						$"Press 4 to play the caller's recording. " +
						$"If you need to access this information later, please see the " +
						$"link that was texted to this same phone number. " +
						$"When done, hang up this call.",
						kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
				}

				if (key == '\0') {
					key = WaitForDigit(5000);
				}

				switch (key) {
					case '1':
						key = '\0';
						i = 3;
						PlayDateAndTimeMessageWasRecievedFallthrough(request, channel, respondeeRequestData);
						break;
					case '2':
						key = '\0';
						i = 3;
						PlayCallerIdInformationFallthrough(request, channel, respondeeRequestData);
						break;
					case '3':
						key = '\0';
						i = 3;
						PlayCallbackNumberFallthrough(request, channel, respondeeRequestData);
						break;
					case '4':
						key = '\0';
						i = 3;
						PlayCallersRecordingFallthrough(request, channel, respondeeRequestData);
						break;
					default:
						key = '\0';
						if (i != 0) {
							if (key == '\0') {
								key = PlayTTS("That isn't a valid option, please try again.", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
							}
						} else {
							if (key == '\0') {
								key = PlayTTS("That isn't a valid option.", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
							}
						}

						continue;
				}
			}

			PlayTTS("Unfortunately we weren't able to get a response, please try again later.", string.Empty, Engine.Neural, VoiceId.Brian);
			throw new PerformHangupException();
		}


		protected void PlayDateAndTimeMessageWasRecievedFallthrough(AGIRequest request, AGIChannel channel, RespondeeCallRequestData respondeeRequestData) {

			if (null == respondeeRequestData.Message)
				ThrowError(request, "54q2", "null == data.Message");

			if (null == respondeeRequestData.BillingCompany) {
				ThrowError(request, "3547", "null == respondeeRequestData.BillingCompany");
			}
			if (string.IsNullOrWhiteSpace(respondeeRequestData.BillingCompany.IANATimezone)) {
				ThrowError(request, "2n33", "string.IsNullOrWhiteSpace(respondeeRequestData.BillingCompany.IANATimezone)");
			}

			string? messageLeftAtISO8601 = respondeeRequestData.Message.MessageLeftAtISO8601;
			if (null == messageLeftAtISO8601) {
				PlayTTS($"The time the message was left is not stored in the database.",
						kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			ParseResult<Instant> result = InstantPattern.ExtendedIso.Parse(messageLeftAtISO8601);
			if (result.Success) {
				Instant instant = result.Value;
				var timeZone = DateTimeZoneProviders.Tzdb[respondeeRequestData.BillingCompany.IANATimezone];
				if (null == timeZone) {
					timeZone = DateTimeZoneProviders.Tzdb[BillingCompanies.kJsonValueIANATimezoneDefault];
				}
				ZonedDateTime dtWpg = instant.InZone(timeZone);
				string dateString = dtWpg.ToString(@"h m tt \o\n dddd MMMM d yyyy", new CultureInfo("en-CA"));

				PlayTTS($"The message was received at {dateString}.",
						kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
			} else {
				PlayTTS($"I wasn't able to record the time the message was recieved.",
						kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
			}

			

			

		}

		protected void PlayCallerIdInformationFallthrough(AGIRequest request, AGIChannel channel, RespondeeCallRequestData respondeeRequestData) {

			if (null == respondeeRequestData.Message)
				ThrowError(request, "a525", "null == data.Message");

			StringBuilder sb = new StringBuilder();

			if (string.IsNullOrWhiteSpace(respondeeRequestData.Message.CallerIdName) && string.IsNullOrWhiteSpace(respondeeRequestData.Message.CallerIdNumber)) {
				sb.Append("There is no caller id information provided with this call.");
			} else {
				sb.Append("The caller id information is as follows; \n\n");
				if (!string.IsNullOrWhiteSpace(respondeeRequestData.Message.CallerIdName)) {
					sb.Append($"The name on the call is: {respondeeRequestData.Message.CallerIdName}\n\n");
				}
				if (!string.IsNullOrWhiteSpace(respondeeRequestData.Message.CallerIdNumber)) {
					sb.Append($"The number on the call is: {respondeeRequestData.Message.CallerIdNumber.WithSpacesBetweenLetters()}");
				}
			}



			PlayTTS(sb.ToString(), kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
		}

		protected void PlayCallbackNumberFallthrough(AGIRequest request, AGIChannel channel, RespondeeCallRequestData respondeeRequestData) {
			if (null == respondeeRequestData.Message)
				ThrowError(request, "a5s5", "null == data.Message");

			StringBuilder sb = new StringBuilder();
			if (string.IsNullOrWhiteSpace(respondeeRequestData.Message.CallbackNumber)) {
				sb.Append("There is no callback number information provided with this call.");
			} else {
				sb.Append($"The callback number is {respondeeRequestData.Message.CallbackNumber.WithSpacesBetweenLetters()}.");
			}

			PlayTTS(sb.ToString(), kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
		}

		protected void PlayCallersRecordingFallthrough(AGIRequest request, AGIChannel channel, RespondeeCallRequestData respondeeRequestData) {
			if (null == respondeeRequestData.Message)
				ThrowError(request, "a535", "null == data.Message");
			if (null == respondeeRequestData.Message.JsonObject)
				ThrowError(request, "a623", "null == data.Message.JsonObject");
			if (null == respondeeRequestData.DPDB)
				ThrowError(request, "a222", "null == data.DPDB");
			if (null == respondeeRequestData.VoicemailId)
				ThrowError(request, "a233", "data.VoicemailId");

			// Instruct the PBX to download the voicemail recording.

			//string? bucket = data.Message.RecordingS3Bucket;
			//string? key = data.Message.RecordingS3Key;
			//string? host = data.Message.RecordingS3Host;
			string? s3CmdUri = respondeeRequestData.Message.RecordingS3CmdURI;
			if (string.IsNullOrWhiteSpace(s3CmdUri)) {
				ThrowError(request, "5824", "string.IsNullOrWhiteSpace(s3CmdUri)");
			}

			PlayS3File(s3CmdUri, kEscapeAllKeys);

			respondeeRequestData.AddTimelineEntry(
				type: OnCall.LeaveMessageRequestData.TimelineType.text,
				timestampISO8601: DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture),
				description: $"\"{request.CallerId}\" Listened to the caller's recording.",
				colour: "green");

		}
	}
}
