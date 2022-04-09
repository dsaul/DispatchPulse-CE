using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class RunReportLabourParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public bool? RunOnAllLabour { get; set; } = true;
			public Guid? AgentId { get; set; }
			public Guid? ProjectId { get; set; }
			public string? StartISO8601 { get; set; }
			public string? EndISO8601 { get; set; }
			public bool? IncludeLabourForOtherProjectsWithMatchingAddresses { get; set; } = false;
		}

		public class RunReportLabourResponse : PermissionsIdempotencyResponse
		{
			public Guid? TaskId { get; set; }

		}

		public async Task RunReportLabour(RunReportLabourParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			RunReportLabourResponse response = new RunReportLabourResponse();


			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			BillingContacts? billingContact = null;


			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;

				BillingSessions? session = null;
				BillingCompanies? billingCompany = null;
				string? dpDBName = null;

				SessionUtils.GetSessionInformation(
					this,
					response,
					p.SessionId,
					out _,
					out billingConnection,
					out session,
					out billingContact,
					out billingCompany,
					out dpDBName,
					out _,
					out dpDBConnection
					);

				if (null != response.IsError && response.IsError.Value)
					break;

				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to billing database.";
					break;
				}

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(EnvDatabases.kPermCRMReportLabourPDF)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				// Create Task

				using NpgsqlConnection pdfLatexDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(PDFLaTeXTask.kPDFLaTeXDBName));
				pdfLatexDB.Open();

				response.TaskId = Guid.NewGuid();

				JObject root = new JObject
				{
					[PDFLaTeXTask.kLaTeXJsonKeyTaskCreated] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					[PDFLaTeXTask.kLaTeXJsonKeyRequestingBillingId] = billingContact.Uuid.ToString(),
					[PDFLaTeXTask.kLaTeXJsonKeyReportType] = PDFLaTeXTask.kLaTeXJsonReportTypeValueLabour,
					[PDFLaTeXTask.kLaTeXJsonKeyStatus] = PDFLaTeXTask.kLaTeXJsonStatusValueQueued,
				};

				PDFLaTeXTask.Upsert(pdfLatexDB, new Dictionary<Guid, PDFLaTeXTask> {
					{
						response.TaskId.Value, new PDFLaTeXTask(response.TaskId, root.ToString())
					}
				}, out _, out _);


				// Create job.

				using NpgsqlConnection jobsDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName));
				jobsDB.Open();

				Guid jobId = Guid.NewGuid();
				JobRunnerJob job = new JobRunnerJob(jobId, new JObject
				{
					[JobRunnerJob.kJobsJsonKeyJobCreatedISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					[JobRunnerJob.kJobsJsonKeyExpiresAtISO8601] = DateTime.UtcNow.AddDays(1).ToString("o", Culture.DevelopmentCulture),
					[JobRunnerJob.kJobsJsonKeyJobType] = JobRunnerJob.kJobTypeValueRunReportLabour,
					[JobRunnerJob.kJobsJsonKeyRequestingBillingId] = billingContact.Uuid.ToString(),
					[JobRunnerJob.kJobsJsonKeyDPDatabase] = dpDBName,
					[JobRunnerJob.kJobsJsonKeyTaskId] = response.TaskId.Value,
					[JobRunnerJob.kJobsJsonKeyAgentId] = p.AgentId,
					[JobRunnerJob.kJobsJsonKeyProjectId] = p.ProjectId,
					[JobRunnerJob.kJobsJsonKeyRunOnAllLabour] = p.RunOnAllLabour,
					[JobRunnerJob.kJobsJsonKeyStartISO8601] = p.StartISO8601,
					[JobRunnerJob.kJobsJsonKeyEndISO8601] = p.EndISO8601,
					[JobRunnerJob.kJobsJsonKeyIncludeLabourForOtherProjectsWithMatchingAddresses] = p.IncludeLabourForOtherProjectsWithMatchingAddresses,
					[JobRunnerJob.kJobsJsonKeyTaskRunnerClaimedISO8601] = null,
					[JobRunnerJob.kJobsJsonKeyCompleted] = false,
				}.ToString());

				JobRunnerJob.Upsert(jobsDB, new Dictionary<Guid, JobRunnerJob> {
					{
						jobId, job
					}
				}, out _, out _);



			} while (false);

			if (billingConnection != null)
			{
				billingConnection.Dispose();
				billingConnection = null;
			}

			if (dpDBConnection != null)
			{
				dpDBConnection.Dispose();
				dpDBConnection = null;
			}

			if (null == billingContact)
			{
				await Clients.Caller.SendAsync("RunReportLabourCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RunReportLabourCB", response).ConfigureAwait(false);
			}
			

		}
	}
}