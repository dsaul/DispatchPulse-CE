using AsterNET.FastAGI;
using Newtonsoft.Json.Linq;
using SharedCode;
using System.Text.RegularExpressions;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using System.Collections.Generic;
using System;
using Renci.SshNet;
using Serilog;
using System.Threading.Tasks;

namespace ARI.IVR.OnCall
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected async Task OnCallAutoAttendantConfirmedLeavingMessage(AGIRequest request, AGIChannel channel, LeaveMessageRequestData requestData) {

			try {
				if (null == requestData.AutoAttendant)
					await ThrowError(request, "526a", "null == data.AutoAttendant");
				if (null == requestData.AutoAttendant.RecordingsAskForCallbackNumberType)
					await ThrowError(request, "3251", "null == data.AutoAttendant.RecordingsAskForCallbackNumberType");
				if (null == requestData.AutoAttendant.RecordingsAskForCallbackNumberText)
					await ThrowError(request, "3a54", "null == data.AutoAttendant.RecordingsAskForCallbackNumberText");
				if (null == requestData.AutoAttendant.RecordingsAskForMessageType)
					await ThrowError(request, "s522", "null == data.AutoAttendant.RecordingsAskForMessageType");
				if (null == requestData.AutoAttendant.RecordingsAskForMessageText)
					await ThrowError(request, "a9s8", "null == data.AutoAttendant.RecordingsAskForMessageText");
				if (null == requestData.AutoAttendant.RecordingsThankYouAfterType)
					await ThrowError(request, "s542", "null == data.AutoAttendant.RecordingsThankYouAfterType");
				if (null == requestData.AutoAttendant.RecordingsThankYouAfterText)
					await ThrowError(request, "5436", "null == data.AutoAttendant.RecordingsThankYouAfterText");

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
							await ThrowError(request, "2133", "null == recordingId");
						}

						askForCallbackNumberEvents.Add(new AudioPlaybackEvent {
							Type = AudioPlaybackEvent.AudioPlaybackEventType.Recording,
							DPDB = requestData.DPDB,
							RecordingId = recordingId,
						});
						break;
				}

				requestData.CallbackNumber = await PromptDigitsPoundTerminated(
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
						await PlayTTS(requestData.AutoAttendant.RecordingsAskForMessageText, kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
						break;
					case OnCallAutoAttendants.RecordingTypeE.Recording:
						if (null == requestData.DPDB) {
							await ThrowError(request, "21433", "null == requestData.DPDB");
						}
						Guid? recordingId = requestData.AutoAttendant.RecordingsAskForMessageRecordingId;
						if (null == recordingId) {
							await ThrowError(request, "214354", "null == recordingId");
						}
						await PlayRecording(requestData.DPDB, recordingId.Value, kEscapeAllKeys);
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




					await SaveAndHangup(request, channel, requestData);
					throw new PerformHangupException();
				}


				requestData.AddTimelineEntry(
					type: LeaveMessageRequestData.TimelineType.text,
					timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					description: $"Caller ended the recording by pressing a button.",
					colour: "#ccc");


				switch (requestData.AutoAttendant.RecordingsThankYouAfterType) {
					case OnCallAutoAttendants.RecordingTypeE.Polly:
						await PlayTTS(requestData.AutoAttendant.RecordingsThankYouAfterText, string.Empty, Engine.Neural, VoiceId.Brian);
						break;
					case OnCallAutoAttendants.RecordingTypeE.Recording:
						if (null == requestData.DPDB) {
							await ThrowError(request, "21433", "null == requestData.DPDB");
						}
						Guid? recordingId = requestData.AutoAttendant.RecordingsThankYouAfterKeyRecordingId;
						if (null == recordingId) {
							await ThrowError(request, "214354", "null == recordingId");
						}
						await PlayRecording(requestData.DPDB, recordingId.Value, kEscapeAllKeys);
						break;
				}

				await SaveAndHangup(request, channel, requestData);

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


		async Task SaveAndHangup(AGIRequest request, AGIChannel channel, LeaveMessageRequestData data) {

			Log.Information("[{AGIRequestUniqueId}] Save and hangup.", request.UniqueId);

			if (null == data.BillingCompany)
				await ThrowError(request, "621a", "null == data.BillingCompany");
			if (string.IsNullOrWhiteSpace(data.BillingCompany.S3BucketName))
				await ThrowError(request, "3525", "string.IsNullOrWhiteSpace(data.BillingCompany.S3BucketName)");
			if (null == data.AutoAttendant)
				await ThrowError(request, "329", "null == data.AutoAttendant");
			if (null == data.AutoAttendant.Id)
				await ThrowError(request, "222r", "null == data.AutoAttendant.Id");
			if (null == data.MessageLeftAtISO8601)
				await ThrowError(request, "aq62", "null == data.MessageLeftAt");
			if (null == data.CallerIdNonDigitsRemoved)
				await ThrowError(request, "aq62", "null == data.CallerIdNonDigitsRemoved");

			


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
					JObject attempt = new JObject {
						[Voicemails.kJsonKeyOnCallAttemptsProgress_KeyCalendarId] = calendarId.ToString(),
						[Voicemails.kJsonKeyOnCallAttemptsProgress_KeyCallAttempts] = 0,
						[Voicemails.kJsonKeyOnCallAttemptsProgress_KeyCallAttemptsMax] = data.AutoAttendant.CallAttemptsToEachCalendarBeforeGivingUp,
						[Voicemails.kJsonKeyOnCallAttemptsProgress_KeyGivenUp] = false
					};
					onCallAttemptsProgress.Add(attempt);
				}
			}
			data.Json[Voicemails.kJsonKeyOnCallAttemptsProgress] = onCallAttemptsProgress;



			await AsyncProcess.StartProcess(
					"/bin/bash",
					$" -c \"s3cmd put {data.OnCallMessageRecordingPathActual} {s3CmdURI}\"",
					null,
					1000 * 60 * 60, // 60 minutes
					Console.Out,
					Console.Out);


			Log.Information("[{AGIRequestUniqueId}] S3CMD Upload complete", request.UniqueId);



			// Saved to database by the handler for the hangup exception.
		}
	}
}
