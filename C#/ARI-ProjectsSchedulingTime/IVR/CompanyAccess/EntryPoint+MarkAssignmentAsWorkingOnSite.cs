using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using API.Hubs;
using System.IO;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using SharedCode;
using Serilog;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void MarkAssignmentAsWorkingOnSite(
			AGIRequest request, 
			AGIChannel channel,
			RequestData data, 
			Assignments assignment
			) {
			BillingCompanies? company = data.Company;
			if (null == company) {
				PlayTTS("System Error 620", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			Agents? agent = data.Agent;
			if (null == agent) {
				PlayTTS("System Error a88", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			if (null == Program.SignalRConnection) {
				PlayTTS("System Error a89", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)) {
				PlayTTS("System Error a90", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			var payload = new PerformARIMarkAssignmentAsWorkingOnSiteParams(
				SharedSecret: SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET,
				CompanyId: company.Uuid,
				CompanyPhoneId: data.CompanyPhoneId,
				AssignmentId: assignment.Id,
				AgentId: agent.Id,
				AgentPhoneId: data.AgentPhoneId,
				EnteredPasscode: data.EnteredPasscode
				);
			Log.Debug($"payload: {payload}");

			PerformARIMarkAssignmentAsWorkingOnSiteResponse response = 
				Program.SignalRConnection.InvokeAsync<PerformARIMarkAssignmentAsWorkingOnSiteResponse>(
					"PerformARIMarkAssignmentAsWorkingOnSite", payload).Result;

			if (response.IsError) {
				PlayTTS($"There was an error, it was: {response.ErrorMessage}", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			if (!response.Completed) {
				PlayTTS($"There was no error, but we weren't told this worked, please double check.", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			PlayTTS("You have been marked as working on site.", "", Engine.Neural, VoiceId.Brian);

			// Try to get active labour
			if (null != data.DPDB && null != data.Agent && null != data.Agent.Id) {
				data.AgentActiveLabour.Clear();
				var resLabour = Labour.ForAgentIDIsActive(data.DPDB, data.Agent.Id.Value, true);
				data.AgentActiveLabour.AddRange(resLabour);
			}
		}
	}
}
