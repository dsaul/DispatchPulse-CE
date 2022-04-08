using System.Text;
using AsterNET.FastAGI;
using Npgsql;
using System.Linq;
using Newtonsoft.Json.Linq;
using SharedCode;
using System.IO;
using Afk.ZoneInfo;
using System.Text.RegularExpressions;
using Databases.Records.CRM;
using Databases.Records.Billing;
using Databases.Records;
using Amazon.Polly;
using System.Collections.Generic;
using System;
using Renci.SshNet;
using Serilog;

namespace ARI.IVR.OnCall
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void OnCallAutoAttendantConfirmedLeavingMessage(AGIRequest request, AGIChannel channel, LeaveMessageRequestData requestData) {

			try {
				if (null == requestData.AutoAttendant)
					ThrowError(request, "526a", "null == data.AutoAttendant");
				if (null == requestData.AutoAttendant.RecordingsAskForCallbackNumberType)
					ThrowError(request, "3251", "null == data.AutoAttendant.RecordingsAskForCallbackNumberType");
				if (null == requestData.AutoAttendant.RecordingsAskForCallbackNumberText)
					ThrowError(request, "3a54", "null == data.AutoAttendant.RecordingsAskForCallbackNumberText");
				if (null == requestData.AutoAttendant.RecordingsAskForMessageType)
					ThrowError(request, "s522", "null == data.AutoAttendant.RecordingsAskForMessageType");
				if (null == requestData.AutoAttendant.RecordingsAskForMessageText)
					ThrowError(request, "a9s8", "null == data.AutoAttendant.RecordingsAskForMessageText");
				if (null == requestData.AutoAttendant.RecordingsThankYouAfterType)
					ThrowError(request, "s542", "null == data.AutoAttendant.RecordingsThankYouAfterType");
				if (null == requestData.AutoAttendant.RecordingsThankYouAfterText)
					ThrowError(request, "5436", "null == data.AutoAttendant.RecordingsThankYouAfterText");

				List<AudioPlaybackEvent> askForCallbackNumberEvents = new List<AudioPlaybackEvent>();

				switch (requestData.AutoAttendant.RecordingsAskForCallbackNumberType) {
					case OnCallAutoAttendants.RecordingTypeE.Polly:
						askForCallbackNumberEvents.Add(new AudioPlaybackEvent {
							Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
							Text = requestData.AutoAttendant.RecordingsAskForCallbackNumberText,
						});
						break;
					case OnCallAutoAttendants.RecordingTypeE.Recording:

						Guid? recordingId = requestData.AutoAttendant.RecordingsAskForCallbackNumberRecordingId;
						if (null == recordingId) {
							ThrowError(request, "2133", "null == recordingId");
						}

						askForCallbackNumberEvents.Add(new AudioPlaybackEvent {
							Type = AudioPlaybackEvent.AudioPlaybackEventType.Recording,
							DPDB = requestData.DPDB,
							RecordingId = recordingId,
						});
						break;
				}

				requestData.CallbackNumber = PromptDigitsPoundTerminated(
					askForCallbackNumberEvents,
					kEscapeAllKeys
				);

				Log.Information("[{AGIRequestUniqueId}] Callback # is '{CallbackNumber}'", request.UniqueId, requestData.CallbackNumber);

				requestData.AddTimelineEntry(
					type: LeaveMessageRequestData.TimelineType.text,
					timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					description: $"Caller entered call back number \"{requestData.CallbackNumber}\".",
					colour: "#ccc");





				switch (requestData.AutoAttendant.RecordingsAskForMessageType) {
					case OnCallAutoAttendants.RecordingTypeE.Polly:
						PlayTTS(requestData.AutoAttendant.RecordingsAskForMessageText, kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
						break;
					case OnCallAutoAttendants.RecordingTypeE.Recording:
						if (null == requestData.DPDB) {
							ThrowError(request, "21433", "null == requestData.DPDB");
						}
						Guid? recordingId = requestData.AutoAttendant.RecordingsAskForMessageRecordingId;
						if (null == recordingId) {
							ThrowError(request, "214354", "null == recordingId");
						}
						PlayRecording(requestData.DPDB, recordingId.Value, kEscapeAllKeys);
						break;
				}





				requestData.OnCallMessageRecordingId = Guid.NewGuid();
				requestData.OnCallMessageRecordingPathAsterisk = $"{Program.PBX_LOCAL_RECORD_FILE_DIRECTORY}/on-call-message-{requestData.OnCallMessageRecordingId}";
				requestData.OnCallMessageRecordingPathActual = $"{requestData.OnCallMessageRecordingPathAsterisk}.wav";

				int recordRes = RecordFile(requestData.OnCallMessageRecordingPathAsterisk,"wav", kEscapeAllKeys, 1000 * 60 * 2, 0, true, 10);
				if (-1 == recordRes) {
					Log.Information("[{AGIRequestUniqueId}] Caller hungup while recording message.", request.UniqueId);

					// Add Timeline Entry
					requestData.AddTimelineEntry(
						type: LeaveMessageRequestData.TimelineType.text,
						timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
						description: $"Caller hungup while recording message.",
						colour: "#ccc");




					SaveAndHangup(request, channel, requestData);
					throw new PerformHangupException();
				}


				requestData.AddTimelineEntry(
					type: LeaveMessageRequestData.TimelineType.text,
					timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					description: $"Caller ended the recording by pressing a button.",
					colour: "#ccc");


				switch (requestData.AutoAttendant.RecordingsThankYouAfterType) {
					case OnCallAutoAttendants.RecordingTypeE.Polly:
						PlayTTS(requestData.AutoAttendant.RecordingsThankYouAfterText, string.Empty, Engine.Neural, VoiceId.Brian);
						break;
					case OnCallAutoAttendants.RecordingTypeE.Recording:
						if (null == requestData.DPDB) {
							ThrowError(request, "21433", "null == requestData.DPDB");
						}
						Guid? recordingId = requestData.AutoAttendant.RecordingsThankYouAfterKeyRecordingId;
						if (null == recordingId) {
							ThrowError(request, "214354", "null == recordingId");
						}
						PlayRecording(requestData.DPDB, recordingId.Value, kEscapeAllKeys);
						break;
				}

				SaveAndHangup(request, channel, requestData);

				throw new PerformHangupException();
			} catch {

				requestData.AddTimelineEntry(
					type: LeaveMessageRequestData.TimelineType.text,
					timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					description: $"Call ended with original caller.",
					colour: "#ccc");


				// Save voicemail to database.
				if (null != requestData.OnCallMessageRecordingId && null != requestData.DPDB) {
					Voicemails vm = new Voicemails(
						requestData.OnCallMessageRecordingId.Value,
						requestData.Json.ToString(),
						"",
						DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture)
					);

					Voicemails.Upsert(requestData.DPDB, new Dictionary<Guid, Voicemails> {
						{ requestData.OnCallMessageRecordingId.Value, vm }
					}, out _, out _);
				}
				

				throw;
			}
			
		}


		void SaveAndHangup(AGIRequest request, AGIChannel channel, LeaveMessageRequestData data) {

			Log.Information("[{AGIRequestUniqueId}] Save and hangup.", request.UniqueId);

			if (null == data.BillingCompany)
				ThrowError(request, "621a", "null == data.BillingCompany");
			if (string.IsNullOrWhiteSpace(data.BillingCompany.S3BucketName))
				ThrowError(request, "3525", "string.IsNullOrWhiteSpace(data.BillingCompany.S3BucketName)");
			if (null == data.AutoAttendant)
				ThrowError(request, "329", "null == data.AutoAttendant");
			if (null == SharedCode.ARI.Konstants.PBX_SSH_PORT)
				ThrowError(request, "as35", "null == PBX_SSH_PORT");
			if (null == SharedCode.ARI.Konstants.PBX_SSH_USER)
				ThrowError(request, "129s", "null == PBX_SSH_USER");
			if (null == data.AutoAttendant.Id)
				ThrowError(request, "222r", "null == data.AutoAttendant.Id");
			if (null == data.MessageLeftAtISO8601)
				ThrowError(request, "aq62", "null == data.MessageLeftAt");
			if (null == data.CallerIdNonDigitsRemoved)
				ThrowError(request, "aq62", "null == data.CallerIdNonDigitsRemoved");

			


			string bucket = data.BillingCompany.S3BucketName;
			data.Json[Voicemails.kJsonKeyRecordingS3Bucket] = bucket;

			Regex rgx = new Regex("[^a-zA-Z0-9]");
			string autoAttendantNameFiltered = rgx.Replace(string.IsNullOrWhiteSpace(data.AutoAttendant.Name) ? data.AutoAttendant.Id.Value.ToString() : data.AutoAttendant.Name, "-");
			autoAttendantNameFiltered = autoAttendantNameFiltered.Trim();
			autoAttendantNameFiltered = autoAttendantNameFiltered.ToLower();
			string mailboxPathComponent = $"on-call-responder-{autoAttendantNameFiltered}";

			string fileNameFiltered = rgx.Replace($"{data.MessageLeftAtISO8601}-{data.CallerIdNonDigitsRemoved}-{data.OnCallMessageRecordingId}", "-");
			fileNameFiltered = fileNameFiltered.Trim();
			fileNameFiltered = fileNameFiltered.ToLower();
			string filenamePathComponent = $"{fileNameFiltered}.wav";

			string s3Host = "us-east-1.linodeobjects.com";
			data.Json[Voicemails.kJsonKeyRecordingS3Host] = s3Host;

			string s3Key = $"Voicemail/{mailboxPathComponent}/{filenamePathComponent}";
			data.Json[Voicemails.kJsonKeyRecordingS3Key] = s3Key;

			string recordingS3HttpsURI = $"https://{s3Host}/{bucket}/{s3Key}";
			data.Json[Voicemails.kJsonKeyRecordingS3HttpsURI] = recordingS3HttpsURI;

			string s3CmdURI = $"s3://{bucket}/{s3Key}";
			data.Json[Voicemails.kJsonKeyRecordingS3CmdURI] = s3CmdURI;

			data.Json[Voicemails.kJsonKeyNextAttemptAfterISO8601] = DateTime.UtcNow.AddDays(-1).ToString("o", Culture.DevelopmentCulture);
			data.Json[Voicemails.kJsonKeyMinutesBetweenCallAttempts] = data.AutoAttendant.MinutesBetweenCallAttempts;

			JArray onCallAttemptsProgress = new JArray();
			{
				List<Guid> calendarIds = data.AutoAttendant.AgentOnCallPriorityCalendars;
				foreach (Guid calendarId in calendarIds) {
					JObject attempt = new JObject();
					attempt[Voicemails.kJsonKeyOnCallAttemptsProgress_KeyCalendarId] = calendarId.ToString();
					attempt[Voicemails.kJsonKeyOnCallAttemptsProgress_KeyCallAttempts] = 0;
					attempt[Voicemails.kJsonKeyOnCallAttemptsProgress_KeyCallAttemptsMax] = data.AutoAttendant.CallAttemptsToEachCalendarBeforeGivingUp;
					attempt[Voicemails.kJsonKeyOnCallAttemptsProgress_KeyGivenUp] = false;
					onCallAttemptsProgress.Add(attempt);
				}
			}
			data.Json[Voicemails.kJsonKeyOnCallAttemptsProgress] = onCallAttemptsProgress;







			string cmd = $"bash -c \"s3cmd put {data.OnCallMessageRecordingPathActual} {s3CmdURI}\"";

			// Tell the PBX To download the file.

			var pk = new PrivateKeyFile(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE);
			var keyFiles = new[] { pk };

			var methods = new List<AuthenticationMethod>();
			methods.Add(new PrivateKeyAuthenticationMethod(SharedCode.ARI.Konstants.PBX_SSH_USER, keyFiles));

			var conn = new ConnectionInfo(SharedCode.ARI.Konstants.PBX_FQDN, SharedCode.ARI.Konstants.PBX_SSH_PORT.Value, SharedCode.ARI.Konstants.PBX_SSH_USER, methods.ToArray());

			var sshClient = new SshClient(conn);
			sshClient.Connect();
			SshCommand sc= sshClient.CreateCommand(cmd);
			sc.Execute();

			string answer = sc.Result;
			Log.Information("[{AGIRequestUniqueId}] S3CMD Upload output: {UploadOutput}", request.UniqueId, answer);



			// Saved to database by the handler for the hangup exception.
		}
	}
}
