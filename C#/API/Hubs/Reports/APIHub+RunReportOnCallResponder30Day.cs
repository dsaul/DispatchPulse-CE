using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Databases.Records.Billing;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using Databases.Records.PDFLaTeX;
using Databases.Records.JobRunner;
using System.Globalization;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class RunReportOnCallResponder30DayParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
		}

		public class RunReportOnCallResponder30DayResponse : IdempotencyResponse
		{
			public Guid? TaskId { get; set; }

		}

		public async Task RunReportOnCallResponder30Day(RunReportOnCallResponder30DayParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			RunReportOnCallResponder30DayResponse response = new RunReportOnCallResponder30DayResponse();


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

				if (!permissions.Contains(Databases.Konstants.kPermCRMReportOnCallResponder30DayPDF)
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
					[PDFLaTeXTask.kLaTeXJsonKeyReportType] = PDFLaTeXTask.kLaTeXJsonReportTypeValueOnCallResponder30Days,
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
					[JobRunnerJob.kJobsJsonKeyJobType] = JobRunnerJob.kJobTypeValueRunReportOnCallResponder30Days,
					[JobRunnerJob.kJobsJsonKeyRequestingBillingId] = billingContact.Uuid.ToString(),
					[JobRunnerJob.kJobsJsonKeyDPDatabase] = dpDBName,
					[JobRunnerJob.kJobsJsonKeyTaskId] = response.TaskId.Value,
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
				await Clients.Caller.SendAsync("RunReportOnCallResponder30DayCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RunReportOnCallResponder30DayCB", response).ConfigureAwait(false);
			}


		}
	}
}