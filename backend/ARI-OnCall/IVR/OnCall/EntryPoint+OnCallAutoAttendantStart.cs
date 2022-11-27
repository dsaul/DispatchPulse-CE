using System;
using AsterNET.FastAGI;
using System.Linq;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using Serilog;
using System.Threading.Tasks;

namespace ARI.IVR.OnCall
{
	public partial class EntryPoint : AGIScriptPlus
	{



		protected void OnCallAutoAttendantStart(AGIRequest request, AGIChannel channel, LeaveMessageRequestData requestData) {

			requestData.MessageLeftAtISO8601 = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);

			if (null == requestData.DPDID)
				ThrowError(request, "ds42", "null == data.DPDID");
			if (null == requestData.DPDB)
				ThrowError(request, "4244", "null == data.DPDB");
			if (null == requestData.DPDID.AssignToID) {
				PlayTTS("This phone number doesn't have a on call responder attendant assigned to it. Please check the phone number page, or contact support.", "", Engine.Neural, VoiceId.Brian);
				throw new PerformHangupException();
			}

			var resAA = OnCallAutoAttendants.ForId(requestData.DPDB, requestData.DPDID.AssignToID.Value);
			if (0 == resAA.Count)
				ThrowError(request, "3255", "0 == resAA.Count");

			requestData.AutoAttendant = resAA.FirstOrDefault().Value;
			requestData.OnCallAutoAttendantId = requestData.AutoAttendant.Id;
			requestData.NoAgentResponseNotificationNumber = requestData.AutoAttendant.NoAgentResponseNotificationNumber;
			requestData.NoAgentResponseNotificationEMail = requestData.AutoAttendant.NoAgentResponseNotificationEMail;
			requestData.MarkedHandledNotificationEMail = requestData.AutoAttendant.MarkedHandledNotificationEMail;

			requestData.AddTimelineEntry(
				type: LeaveMessageRequestData.TimelineType.text,
				timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				description: $"Found on-call responder \"{requestData.AutoAttendant.Name}\".",
				colour: "#ccc");



			if (null == requestData.AutoAttendant.RecordingsIntroType)
				ThrowError(request, "1566", "null == data.AutoAttendant.RecordingsIntroType");
			if (null == requestData.AutoAttendant.RecordingsIntroText)
				ThrowError(request, "1567", "null == data.AutoAttendant.RecordingsIntroText");

			Log.Information("[{AGIRequestUniqueId}] Started On Call Auto Attendant {AutoAttendantName}", request.UniqueId, requestData.AutoAttendant.Name);

			int i = 3;

			while ((--i) >= 0) {

				char key = '\0';

				// Play intro.
				switch (requestData.AutoAttendant.RecordingsIntroType) {
					case OnCallAutoAttendants.RecordingTypeE.Polly:
						if (key == '\0') {
							key = PlayTTS(requestData.AutoAttendant.RecordingsIntroText, kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
						}
						break;
					case OnCallAutoAttendants.RecordingTypeE.Recording:
						if (key == '\0') {
							Guid? recordingId = requestData.AutoAttendant.RecordingsIntroRecordingId;
							if (null == recordingId) {
								ThrowError(request, "2133", "null == recordingId");
							}
							key = PlayRecording(requestData.DPDB, recordingId.Value, kEscapeAllKeys);

						}
						break;
				}

				if (key == '\0') {
					key = WaitForDigit(5000);
				}


				switch (key) {
					case '1':

						requestData.AddTimelineEntry(
							type: LeaveMessageRequestData.TimelineType.text,
							timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
							description: "Caller confirmed leaving a message.",
							colour: "#ccc");



						OnCallAutoAttendantConfirmedLeavingMessage(request, channel, requestData);
						throw new PerformHangupException();
					default:
						if (i != 0) {
							PlayTTS("That isn't a valid option, please try again.", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
						} else {
							PlayTTS("That isn't a valid option.", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
						}
						
						continue;
				}
			}

			PlayTTS("Unfortunately we weren't able to get a response, please try again later.", kEscapeAllKeys, Engine.Neural, VoiceId.Brian);
			throw new PerformHangupException();
		}





	}
}
