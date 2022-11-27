using System;
using AsterNET.FastAGI;
using Serilog;
using SharedCode;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		private const int maxRetryAttempts = 3;

		private const string escapeAllKeys = "0123456789*#";

		public override void Service(AGIRequest request, AGIChannel channel) {
			using RequestData data = new RequestData();

			Answer();
			SetAutoHangup(60 * 20); // These calls shouldn't take more than 20 minutes. Set auto hang up not waste too much money on dead channels.

			try {
				WelcomeToCompanyAccess(request, channel, data).Wait();
			} catch (PerformHangupException) {
				Hangup();
				return;
			} catch (Exception e) {
				Log.Debug($"Exception: {e.Message}");
			}
		}
		
	}
}
