using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SharedCode.DatabaseSchemas;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public async Task<PerformCancelAndDeleteThisWorkTimerResponse> PerformCancelAndDeleteThisWorkTimer(PerformCancelAndDeleteThisWorkTimerParams p)
		{
			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET))
				return new PerformCancelAndDeleteThisWorkTimerResponse(IsError: true, ErrorMessage: "ARI_AND_API_SHARED_SECRET_FILE not set!", Completed: false);
			if (p.SharedSecret != SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)
				return new PerformCancelAndDeleteThisWorkTimerResponse(IsError: true, ErrorMessage: "Shared Secret doesn't match.", Completed: false);
			if (null == p)
				throw new ArgumentNullException(nameof(p));
			if (string.IsNullOrWhiteSpace(p.CompanyPhoneId))
				return new PerformCancelAndDeleteThisWorkTimerResponse(IsError: true, ErrorMessage: "Didn't receive a company phone id.", Completed: false);
			if (string.IsNullOrWhiteSpace(p.AgentPhoneId))
				return new PerformCancelAndDeleteThisWorkTimerResponse(IsError: true, ErrorMessage: "Didn't receive a agent phone id.", Completed: false);
			if (string.IsNullOrWhiteSpace(p.EnteredPasscode))
				return new PerformCancelAndDeleteThisWorkTimerResponse(IsError: true, ErrorMessage: "Didn't receive a agent passcode.", Completed: false);
			if (null == p.LabourId)
				return new PerformCancelAndDeleteThisWorkTimerResponse(IsError: true, ErrorMessage: "Didn't receive a labour id.", Completed: false);

			bool isError = false;
			string? errorMessage = null;
			bool completed = false;

			do
			{

				using NpgsqlConnection? billingConnection = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(EnvDatabases.BILLING_DATABASE_NAME));
				billingConnection.Open();



				var resCompany = BillingCompanies.ForPhoneId(billingConnection, p.CompanyPhoneId);
				if (0 == resCompany.Count)
				{
					isError = true;
					errorMessage = "We couldn't find your company in the database.";
					break;
				}

				// Get the company
				BillingCompanies company = resCompany.FirstOrDefault().Value;

				if (null == company.Uuid)
				{
					isError = true;
					errorMessage = "We couldn't find your company id in the database.";
					break;
				}

				// Get the DP database name.
				var resPackages = BillingPackages.ForProvisionDispatchPulse(billingConnection, true);

				var resSubs = BillingSubscriptions.ForCompanyIdPackageIdsAndHasDatabase(billingConnection, company.Uuid.Value, resPackages.Keys);
				if (0 == resSubs.Count)
				{
					isError = true;
					errorMessage = "Can't find subscriptions for your company.";
					break;
				}

				BillingSubscriptions sub = resSubs.FirstOrDefault().Value;

				string? dbName = sub.ProvisionedDatabaseName;
				if (string.IsNullOrWhiteSpace(dbName))
				{
					isError = true;
					errorMessage = "Couldn't find the database name for your company.";
					break;
				}

				using NpgsqlConnection? dpDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(dbName));
				dpDB.Open();

				// Get the agent.
				var resAgents = Agents.ForPhoneId(dpDB, p.AgentPhoneId);
				if (0 == resAgents.Count)
				{
					isError = true;
					errorMessage = "Can't find the agent for the id entered.";
					break;
				}

				Agents agent = resAgents.FirstOrDefault().Value;
				if (null == agent.Id)
				{
					isError = true;
					errorMessage = "The found agent has no id.";
					break;
				}
				if (string.IsNullOrWhiteSpace(agent.PhonePasscode))
				{
					isError = true;
					errorMessage = "The found agent has no phone id to verify.";
					break;
				}

				if (agent.PhonePasscode.Trim() != p.EnteredPasscode.Trim())
				{
					isError = true;
					errorMessage = "The passcode provided does not match.";
					break;
				}


				// Get the labour. 
				var resLabour = Labour.ForId(dpDB, p.LabourId.Value);
				if (0 == resLabour.Count)
				{
					isError = true;
					errorMessage = "Couldn't find the labour entry.";
					break;
				}

				Labour labour = resLabour.FirstOrDefault().Value;

				if (null == labour.Id)
				{
					isError = true;
					errorMessage = "Unable to get the labour id to delete.";
					break;
				}

				// Make sure that the agent is assigned to the labour entry.
				if (labour.AgentId != agent.Id.Value)
				{
					isError = true;
					errorMessage = "Your agent isn't assigned to this labour entry.";
					break;
				}


				List<Guid> affected = Labour.Delete(dpDB, new List<Guid> { labour.Id.Value });
				if (affected.Count == 0)
				{
					isError = true;
					errorMessage = "Unable to delete the labour entry.";
					break;
				}

				DeleteLabourResponse othersMsg = new DeleteLabourResponse
				{
					IdempotencyToken = Guid.NewGuid().ToString(),
					LabourDelete = affected
				};

				await Clients.Group(BillingCompanies.GroupNameForCompanyId(company.Uuid.Value)).SendAsync("DeleteLabourCB", othersMsg).ConfigureAwait(false);

				completed = true;
			}
			while (false);

			return new PerformCancelAndDeleteThisWorkTimerResponse(IsError: isError, ErrorMessage: errorMessage, Completed: completed);
		}
	}
}
