using AsterNET.FastAGI;
using Amazon.Polly;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void SubMenuMaxRetryAttempts(AGIRequest request, AGIChannel channel, RequestData data) {
			PlayTTS("I haven't been getting a response, please call back later.", "", Engine.Neural, VoiceId.Brian);

			throw new PerformHangupException();
		}
	}
}
