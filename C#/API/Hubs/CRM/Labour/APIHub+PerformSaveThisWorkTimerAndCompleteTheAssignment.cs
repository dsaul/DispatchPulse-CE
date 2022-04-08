using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Databases.Records.Billing;
using System.IO;
using Databases.Records.CRM;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public async Task<PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse> PerformSaveThisWorkTimerAndCompleteTheAssignment(PerformSaveThisWorkTimerAndCompleteTheAssignmentParams p)
		{
			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET))
				return new PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse(IsError: true, ErrorMessage: "ARI_AND_API_SHARED_SECRET_FILE not set!", Completed: false);
			if (p.SharedSecret != SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)
				return new PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse(IsError: true, ErrorMessage: "Shared Secret doesn't match.", Completed: false);
			if (null == p)
				throw new ArgumentNullException(nameof(p));
			if (string.IsNullOrWhiteSpace(p.CompanyPhoneId))
				return new PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse(IsError: true, ErrorMessage: "Didn't receive a company phone id.", Completed: false);
			if (string.IsNullOrWhiteSpace(p.AgentPhoneId))
				return new PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse(IsError: true, ErrorMessage: "Didn't receive a agent phone id.", Completed: false);
			if (string.IsNullOrWhiteSpace(p.EnteredPasscode))
				return new PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse(IsError: true, ErrorMessage: "Didn't receive a agent passcode.", Completed: false);
			if (null == p.LabourId)
				return new PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse(IsError: true, ErrorMessage: "Didn't receive a labour id.", Completed: false);

			bool isError = false;
			string? errorMessage = null;
			bool completed = false;

			do
			{

				using NpgsqlConnection? billingConnection = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
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

				using NpgsqlConnection? dpDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(dbName));
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

				Labour travelLabour = resLabour.FirstOrDefault().Value;

				// Make sure that the agent is assigned to the labour entry.
				if (travelLabour.AgentId != agent.Id.Value)
				{
					isError = true;
					errorMessage = "Your agent isn't assigned to this labour entry.";
					break;
				}


				// Close labour entry.
				var labourUpdatePayload = new Dictionary<Guid, Labour>();

				do
				{
					if (null == travelLabour.JsonObject)
						break;

					JObject travelJSON = travelLabour.JsonObject;
					travelJSON[Labour.kJsonKeyIsActive] = false;
					travelJSON[Labour.kJsonKeyEndISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);

					travelLabour = travelLabour with
					{
						Json = travelJSON.ToString(Formatting.Indented)
					};

					if (null == travelLabour.Id)
						break;

					labourUpdatePayload.Add(travelLabour.Id.Value, travelLabour);

				} while (false);

				Dictionary<Guid, Labour> labourSendToOthers;
				Labour.Upsert(
					dpDB,
					labourUpdatePayload,
					out _,
					out labourSendToOthers
				);

				RequestLabourResponse labourOthersMsg = new RequestLabourResponse
				{
					IdempotencyToken = Guid.NewGuid().ToString(),
					Labour = labourSendToOthers
				};

				await Clients.Group(BillingCompanies.GroupNameForCompanyId(company.Uuid.Value)).SendAsync("RequestLabourCB", labourOthersMsg).ConfigureAwait(false);


				// Complete assignment.

				// Get the assignment.
				if (null == travelLabour || null == travelLabour.AssignmentId)
                {
					completed = true;
					break;
				}

				var resAssignment = Assignments.ForId(dpDB, travelLabour.AssignmentId.Value);
				if (0 == resAssignment.Count)
				{
					completed = true;
					break;
				}

				Assignments assignment = resAssignment.FirstOrDefault().Value;

				// Get the assignments status

				var resAssignmentStatus = AssignmentStatus.ForIsBillableReview(dpDB, true);
				if (0 == resLabour.Count)
				{
					isError = true;
					errorMessage = "Couldn't find a suitable assignment status.";
					break;
				}

				AssignmentStatus status = resAssignmentStatus.FirstOrDefault().Value;
				if (null == status.Id)
				{
					isError = true;
					errorMessage = "Status has no id.";
					break;
				}

				var assignmentUpdatePayload = new Dictionary<Guid, Assignments>();

				do
				{
					if (null == assignment.JsonObject)
						break;

					JObject assignmentJSON = assignment.JsonObject;
					assignmentJSON["statusId"] = status.Id.Value;

					assignment = assignment with
					{
						Json = assignmentJSON.ToString(Formatting.Indented)
					};

					if (null == assignment.Id)
						break;

					assignmentUpdatePayload.Add(assignment.Id.Value, assignment);

				} while (false);

				Dictionary<Guid, Assignments> assignmentsSendToOthers;
				Assignments.Upsert(
					dpDB,
					assignmentUpdatePayload,
					out _,
					out assignmentsSendToOthers
				);

				RequestAssignmentsResponse assignmentsOthersMsg = new RequestAssignmentsResponse
				{
					IdempotencyToken = Guid.NewGuid().ToString(),
					Assignments = assignmentsSendToOthers
				};

				await Clients.Group(BillingCompanies.GroupNameForCompanyId(company.Uuid.Value)).SendAsync("RequestAssignmentsCB", assignmentsOthersMsg).ConfigureAwait(false);



				completed = true;
			}
			while (false);

			return new PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse(IsError: isError, ErrorMessage: errorMessage, Completed: completed);
		}
	}
}
