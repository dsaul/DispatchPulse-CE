using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using System.Threading.Tasks;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected async Task AssignmentsList(
			AGIRequest request, 
			AGIChannel channel, 
			RequestData data
			) {

			await PlayTTS("At anytime, press a button on your phone to jump past what I'm talking about.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

			if (data.ScheduledAssignments.Count > 0) {

				await PlayTTS("Here are your scheduled assignments.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

				for (int i = 0; i < data.ScheduledAssignments.Count; i++) {
					Assignments assignment = data.ScheduledAssignments[i];

					await PlayTTS($"Assignment #{i + 1}", escapeAllKeys, Engine.Neural, VoiceId.Brian);

					await AssignmentDetail(request, channel, data, assignment);
				}

				await PlayTTS("That was the last scheduled assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			}

			if (data.UnscheduledAssignments.Count > 0) {

				await PlayTTS("Here are your unscheduled assignments.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

				for (int i = 0; i < data.UnscheduledAssignments.Count; i++) {
					Assignments assignment = data.UnscheduledAssignments[i];

					await PlayTTS($"Assignment #{i + 1}", escapeAllKeys, Engine.Neural, VoiceId.Brian);

					await AssignmentDetail(request, channel, data, assignment);
				}

				await PlayTTS("That was the last unscheduled assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

			}


			await AgentMenu(request, channel, data);
		}
	}
}
