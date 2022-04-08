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
using System.Globalization;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		

		public async Task<PerformARIMarkAssignmentAsWorkingOnSiteResponse> PerformARIMarkAssignmentAsWorkingOnSite(PerformARIMarkAssignmentAsWorkingOnSiteParams p)
		{
			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET))
				return new PerformARIMarkAssignmentAsWorkingOnSiteResponse(IsError: true, ErrorMessage: "ARI_AND_API_SHARED_SECRET_FILE not set!", Completed: false);
			if (p.SharedSecret != SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)
				return new PerformARIMarkAssignmentAsWorkingOnSiteResponse(IsError: true, ErrorMessage: "Shared Secret doesn't match.", Completed: false);
			if (null == p)
				throw new ArgumentNullException(nameof(p));
			if (string.IsNullOrWhiteSpace(p.CompanyPhoneId))
				return new PerformARIMarkAssignmentAsWorkingOnSiteResponse(IsError: true, ErrorMessage: "Didn't receive a company phone id.", Completed: false);
			if (string.IsNullOrWhiteSpace(p.AgentPhoneId))
				return new PerformARIMarkAssignmentAsWorkingOnSiteResponse(IsError: true, ErrorMessage: "Didn't receive a agent phone id.", Completed: false);
			if (string.IsNullOrWhiteSpace(p.EnteredPasscode))
				return new PerformARIMarkAssignmentAsWorkingOnSiteResponse(IsError: true, ErrorMessage: "Didn't receive a agent passcode.", Completed: false);
			if (null == p.AssignmentId)
				return new PerformARIMarkAssignmentAsWorkingOnSiteResponse(IsError: true, ErrorMessage: "Didn't receive a assignment id.", Completed: false);

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

				// Get the assignment. 
				var resAssignments = Assignments.ForId(dpDB, p.AssignmentId.Value);
				if (0 == resAssignments.Count)
				{
					isError = true;
					errorMessage = "Couldn't find the assignment.";
					break;
				}

				Assignments assignment = resAssignments.FirstOrDefault().Value;


				// Make sure that the agent is assigned to the assignment.
				HashSet<Guid> agentIds = assignment.AgentIds;

				if (!agentIds.Contains(agent.Id.Value))
				{
					isError = true;
					errorMessage = "Your agent isn't assigned to this assignment.";
					break;
				}

				// Get the project if applicable.
				Projects? project = null;

				do
				{
					if (null == assignment.ProjectId)
						break;

					var resProject = Projects.ForId(dpDB, assignment.ProjectId.Value);
					if (0 == resProject.Count)
						break;

					project = resProject.FirstOrDefault().Value;

				} while (false);




				// Find the default billable labour type.

				var resLabourTypes = LabourTypes.ForBillableAndDefault(dpDB);
				if (0 == resLabourTypes.Count)
				{
					isError = true;
					errorMessage = "Couldn't find the labour type.";
					break;
				}

				LabourTypes type = resLabourTypes.FirstOrDefault().Value;


				// Create a new labour entry.
				Guid labourId = Guid.NewGuid();
				Labour labour = new Labour(
					Id: labourId,
					Json: new JObject
					{
						[Labour.kJsonKeyProjectId] = assignment.ProjectId == null ? null : assignment.ProjectId.Value,
						[Labour.kJsonKeyAgentId] = agent.Id,
						[Labour.kJsonKeyAssignmentId] = assignment.Id,
						[Labour.kJsonKeyTypeId] = type.Id,
						[Labour.kJsonKeyTimeMode] = Labour.kJsonValueTimeModeStartStopTimestamp,
						[Labour.kJsonKeyHours] = null,
						[Labour.kJsonKeyStartISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
						[Labour.kJsonKeyEndISO8601] = null,
						[Labour.kJsonKeyIsActive] = true,
						[Labour.kJsonKeyLocationType] = Labour.kJsonValueLocationTypeOnSite,
						[Labour.kJsonKeyIsExtra] = true,
						[Labour.kJsonKeyIsBilled] = false,
						[Labour.kJsonKeyIsPaidOut] = false,
						[Labour.kJsonKeyIsEnteredThroughTelephoneCompanyAccess] = true,
						[Labour.kJsonKeyExceptionTypeId] = null,
						[Labour.kJsonKeyHolidayTypeId] = null,
						[Labour.kJsonKeyNotes] = "Entered through company access over the telephone.",
						[Labour.kJsonKeyBankedPayOutAmount] = null,

					}.ToString(),
					SearchString: Labour.GenerateSearchString(
						project?.AddressesSearchString,
						agent.Name,
						"Is Active",
						"Is Extra",
						"",
						"",
						"Entered through company access over the telephone."
						),
					LastModifiedIso8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture)
				);

				Dictionary<Guid, Labour> toSendToOthers;
				Labour.Upsert(
					dpDB,
					new Dictionary<Guid, Labour>() {
						{ labourId, labour }
					},
					out _,
					out toSendToOthers
				);

				RequestLabourResponse othersMsg = new RequestLabourResponse
				{
					IdempotencyToken = Guid.NewGuid().ToString(),
					Labour = toSendToOthers
				};

				await Clients.Group(BillingCompanies.GroupNameForCompanyId(company.Uuid.Value)).SendAsync("RequestLabourCB", othersMsg).ConfigureAwait(false);

				completed = true;
			}
			while (false);

			return new PerformARIMarkAssignmentAsWorkingOnSiteResponse(IsError: isError, ErrorMessage: errorMessage, Completed: completed);
		}
	}
}
