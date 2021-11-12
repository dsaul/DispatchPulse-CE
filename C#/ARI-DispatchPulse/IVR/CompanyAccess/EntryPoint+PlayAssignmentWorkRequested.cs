using AsterNET.FastAGI;
using Databases.Records.CRM;
using Amazon.Polly;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void PlayAssignmentWorkRequested(AGIRequest request, AGIChannel channel,
			RequestData data, Assignments assignment) {

			string? workRequested = assignment.WorkRequested;

			if (string.IsNullOrWhiteSpace(workRequested)) {
				PlayTTS("There is no work requested on this assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			} else {
				PlayTTS(workRequested, escapeAllKeys, Engine.Neural, VoiceId.Brian);
			}
		}
	}
}
