using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using API.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using SharedCode;
using Serilog;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void SaveThisWorkTimerAndCompleteTheAssignment(
			AGIRequest request,
			AGIChannel channel,
			RequestData data,
			Labour labour
			) {

			BillingCompanies? company = data.Company;
			if (null == company) {
				PlayTTS("System Error 620", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			Agents? agent = data.Agent;
			if (null == agent) {
				PlayTTS("System Error 621", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			if (null == Program.SignalRConnection) {
				PlayTTS("System Error 622", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)) {
				PlayTTS("System Error 623", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			var payload = new PerformSaveThisWorkTimerAndCompleteTheAssignmentParams(
				SharedSecret: SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET,
				CompanyId: company.Uuid,
				CompanyPhoneId: data.CompanyPhoneId,
				LabourId: labour.Id,
				AgentId: agent.Id,
				AgentPhoneId: data.AgentPhoneId,
				EnteredPasscode: data.EnteredPasscode
				);
			Log.Debug($"payload: {payload}");

			PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse response =
				Program.SignalRConnection.InvokeAsync<PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse>(
					"PerformSaveThisWorkTimerAndCompleteTheAssignment", payload).Result;

			Log.Debug($"payload: {response}");

			if (response.IsError) {
				PlayTTS($"There was an error, it was: {response.ErrorMessage}", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			if (!response.Completed) {
				PlayTTS($"There was no error, but we weren't told this worked, please double check.", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			PlayTTS("Your work timer has been completed, and the assignment has been put in billable review.", "", Engine.Neural, VoiceId.Brian);

			// Try to get active labour
			if (null != data.DPDB && null != data.Agent && null != data.Agent.Id) {
				data.AgentActiveLabour.Clear();
				var resLabour = Labour.ForAgentIDIsActive(data.DPDB, data.Agent.Id.Value, true);
				data.AgentActiveLabour.AddRange(resLabour);
			}


		}
	}
}
