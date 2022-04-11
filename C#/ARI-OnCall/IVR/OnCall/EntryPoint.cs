using System;
using ARI;
using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Newtonsoft.Json.Linq;
using Serilog;
using SharedCode;

namespace ARI.IVR.OnCall
{
	public partial class EntryPoint : AGIScriptPlus
	{
		public override void Service(AGIRequest request, AGIChannel channel) {

			Log.Information($"[{request.UniqueId}] OnCall New Call {request.CallerId} {request.CallerIdName}");


			using LeaveMessageRequestData requestData = new LeaveMessageRequestData();

			Answer();
			SetAutoHangup(60 * 20); // These calls shouldn't take more than 20 minutes. Set auto hang up not waste too much money on dead channels.

			requestData.CallerIdNumber = request.CallerId;
			requestData.CallerIdName = request.CallerIdName;

			
			requestData.AddTimelineEntry(
				type: LeaveMessageRequestData.TimelineType.text,
				timestampISO8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				description: "Call answered.",
				colour: "#ccc");




			try {
				IdentifyCompany(request, channel, requestData).Wait();
			}
			catch (PerformHangupException) {
				Hangup();
				return;
			}
			catch (Exception e) {
				Log.Fatal(e, $"{e.Message}");
				throw;
			}
		}

	}
}
