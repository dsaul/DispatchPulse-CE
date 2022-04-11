using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using API.Hubs;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.IO;
using Serilog;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void CancelAndDeleteThisWorkTimer(
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
				PlayTTS("System Error w98", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			if (null == Program.SignalRConnection) {
				PlayTTS("System Error w97", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)) {
				PlayTTS("System Error w99", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			var payload = new PerformCancelAndDeleteThisWorkTimerParams(
				SharedSecret: SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET,
				CompanyId: company.Uuid,
				CompanyPhoneId: data.CompanyPhoneId,
				LabourId: labour.Id,
				AgentId: agent.Id,
				AgentPhoneId: data.AgentPhoneId,
				EnteredPasscode: data.EnteredPasscode
				);
			Log.Debug($"payload: {payload}");

			PerformCancelAndDeleteThisWorkTimerResponse response = Program.SignalRConnection.InvokeAsync<PerformCancelAndDeleteThisWorkTimerResponse>("PerformCancelAndDeleteThisWorkTimer", payload).Result;

			Log.Debug($"response: {response}");

			PlayTTS("We have cancelled and deleted this active work timer.", "", Engine.Neural, VoiceId.Brian);
		}
	}
}
