using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using API.Utility;
using Databases.Records.Billing;
using Databases.Records.JobRunner;
using Databases.Records.PDFLaTeX;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class RunReportProjectsParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
			public List<Guid> ProjectIds { get; set; } = new List<Guid>();
			public bool IncludeCompanies { get; set; } = true;
			public bool IncludeContacts { get; set; } = true;
			public bool IncludeSchedule { get; set; } = true;
			public bool IncludeNotes { get; set; } = true;
			public bool IncludeLabour { get; set; } = true;
			public bool IncludeMaterials { get; set; } = true;
		}

		public class RunReportProjectsResponse : IdempotencyResponse
		{
			public Guid? TaskId { get; set; } = null;

		}

		public async Task RunReportProjects(RunReportProjectsParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			RunReportProjectsResponse response = new RunReportProjectsResponse();


			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			BillingContacts? billingContact = null;
			string? dpDBName = null;

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

				if (!permissions.Contains(Databases.Konstants.kPermCRMReportProjectsPDF)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// Create Task

				using NpgsqlConnection pdfLatexDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(PDFLaTeXTask.kPDFLaTeXDBName));
				pdfLatexDB.Open();

				response.TaskId = Guid.NewGuid();

				JObject root = new JObject
				{
					[PDFLaTeXTask.kLaTeXJsonKeyTaskCreated] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					[PDFLaTeXTask.kLaTeXJsonKeyRequestingBillingId] = billingContact.Uuid.ToString(),
					[PDFLaTeXTask.kLaTeXJsonKeyReportType] = PDFLaTeXTask.kLaTeXJsonReportTypeValueProjects,
					[PDFLaTeXTask.kLaTeXJsonKeyStatus] = PDFLaTeXTask.kLaTeXJsonStatusValueQueued,
				};

				PDFLaTeXTask.Upsert(pdfLatexDB, new Dictionary<Guid, PDFLaTeXTask> {
					{
						response.TaskId.Value, new PDFLaTeXTask(response.TaskId, root.ToString())
					}
				}, out _, out _);


				// Create job.

				using NpgsqlConnection jobsDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName));
				jobsDB.Open();

				Guid jobId = Guid.NewGuid();
				JobRunnerJob job = new JobRunnerJob(jobId, new JObject
				{
					[JobRunnerJob.kJobsJsonKeyJobCreatedISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					[JobRunnerJob.kJobsJsonKeyExpiresAtISO8601] = DateTime.UtcNow.AddDays(1).ToString("o", Culture.DevelopmentCulture),
					[JobRunnerJob.kJobsJsonKeyJobType] = JobRunnerJob.kJobTypeValueRunReportProjects,
					[JobRunnerJob.kJobsJsonKeyRequestingBillingId] = billingContact.Uuid.ToString(),
					[JobRunnerJob.kJobsJsonKeyDPDatabase] = dpDBName,
					[JobRunnerJob.kJobsJsonKeyTaskId] = response.TaskId.Value,
					[JobRunnerJob.kJobsJsonKeyProjectIds] = JArray.FromObject(p.ProjectIds),
					[JobRunnerJob.kJobsJsonKeyIncludeCompanies] = p.IncludeCompanies,
					[JobRunnerJob.kJobsJsonKeyIncludeContacts] = p.IncludeContacts,
					[JobRunnerJob.kJobsJsonKeyIncludeSchedule] = p.IncludeSchedule,
					[JobRunnerJob.kJobsJsonKeyIncludeNotes] = p.IncludeNotes,
					[JobRunnerJob.kJobsJsonKeyIncludeLabour] = p.IncludeLabour,
					[JobRunnerJob.kJobsJsonKeyIncludeMaterials] = p.IncludeMaterials,
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
				await Clients.Caller.SendAsync("RunReportProjectsCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RunReportProjectsCB", response).ConfigureAwait(false);
			}

			

		}
	}
}