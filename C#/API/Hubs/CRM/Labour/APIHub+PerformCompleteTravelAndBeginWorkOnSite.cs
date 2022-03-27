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

		public async Task<PerformCompleteTravelAndBeginWorkOnSiteResponse> PerformCompleteTravelAndBeginWorkOnSite(PerformCompleteTravelAndBeginWorkOnSiteParams p)
		{
			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET))
				return new PerformCompleteTravelAndBeginWorkOnSiteResponse(IsError: true, ErrorMessage: "ARI_AND_API_SHARED_SECRET_FILE not set!", Completed: false);
			if (p.SharedSecret != SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)
				return new PerformCompleteTravelAndBeginWorkOnSiteResponse(IsError: true, ErrorMessage: "Shared Secret doesn't match.", Completed: false);
			if (null == p)
				throw new ArgumentNullException(nameof(p));
			if (string.IsNullOrWhiteSpace(p.CompanyPhoneId))
				return new PerformCompleteTravelAndBeginWorkOnSiteResponse(IsError: true, ErrorMessage: "Didn't receive a company phone id.", Completed: false);
			if (string.IsNullOrWhiteSpace(p.AgentPhoneId))
				return new PerformCompleteTravelAndBeginWorkOnSiteResponse(IsError: true, ErrorMessage: "Didn't receive a agent phone id.", Completed: false);
			if (string.IsNullOrWhiteSpace(p.EnteredPasscode))
				return new PerformCompleteTravelAndBeginWorkOnSiteResponse(IsError: true, ErrorMessage: "Didn't receive a agent passcode.", Completed: false);
			if (null == p.LabourId)
				return new PerformCompleteTravelAndBeginWorkOnSiteResponse(IsError: true, ErrorMessage: "Didn't receive a labour id.", Completed: false);

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


				// Get the project if applicable.
				Projects? project = null;

				do
				{
					if (null == travelLabour.ProjectId)
						break;

					var resProject = Projects.ForId(dpDB, travelLabour.ProjectId.Value);
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


				var updateObjects = new Dictionary<Guid, Labour>();



				do
				{
					if (null == travelLabour.JsonObject)
						break;

					JObject travelJSON = travelLabour.JsonObject;
					travelJSON[Labour.kJsonKeyIsActive] = false;
					travelJSON[Labour.kJsonKeyEndISO8601] = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);

					travelLabour = travelLabour with
					{
						Json = travelJSON.ToString(Formatting.Indented)
					};

					if (null == travelLabour.Id)
						break;

					updateObjects.Add(travelLabour.Id.Value, travelLabour);

				} while (false);



				// Create a new labour entry.
				Guid labourId = Guid.NewGuid();
				Labour labour = new Labour(
					Id: labourId,
					Json: new JObject
					{
						[Labour.kJsonKeyProjectId] = travelLabour.ProjectId == null ? null : travelLabour.ProjectId.Value,
						[Labour.kJsonKeyAgentId] = agent.Id,
						[Labour.kJsonKeyAssignmentId] = travelLabour.AssignmentId == null ? null : travelLabour.AssignmentId.Value,
						[Labour.kJsonKeyTypeId] = type.Id,
						[Labour.kJsonKeyTimeMode] = Labour.kJsonValueTimeModeStartStopTimestamp,
						[Labour.kJsonKeyHours] = null,
						[Labour.kJsonKeyStartISO8601] = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture),
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
					LastModifiedIso8601: DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture)
				);
				updateObjects.Add(labourId, labour);
				
				Dictionary<Guid, Labour> toSendToOthers;
				Labour.Upsert(
					dpDB,
					updateObjects,
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

			return new PerformCompleteTravelAndBeginWorkOnSiteResponse(IsError: isError, ErrorMessage: errorMessage, Completed: completed);
		}
	}
}
