using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void AssignmentsList(
			AGIRequest request, 
			AGIChannel channel, 
			RequestData data
			) {

			PlayTTS("At anytime, press a button on your phone to jump past what I'm talking about.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

			if (data.ScheduledAssignments.Count > 0) {

				PlayTTS("Here are your scheduled assignments.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

				for (int i = 0; i < data.ScheduledAssignments.Count; i++) {
					Assignments assignment = data.ScheduledAssignments[i];

					PlayTTS($"Assignment #{i + 1}", escapeAllKeys, Engine.Neural, VoiceId.Brian);

					AssignmentDetail(request, channel, data, assignment);
				}

				PlayTTS("That was the last scheduled assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			}

			if (data.UnscheduledAssignments.Count > 0) {

				PlayTTS("Here are your unscheduled assignments.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

				for (int i = 0; i < data.UnscheduledAssignments.Count; i++) {
					Assignments assignment = data.UnscheduledAssignments[i];

					PlayTTS($"Assignment #{i + 1}", escapeAllKeys, Engine.Neural, VoiceId.Brian);

					AssignmentDetail(request, channel, data, assignment);
				}

				PlayTTS("That was the last unscheduled assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

			}


			AgentMenu(request, channel, data);
		}
	}
}
