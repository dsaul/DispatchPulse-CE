using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Threading;
using SharedCode.DatabaseSchemas;
using LaTeXGenerators;
using SharedCode;
using Serilog;
using Serilog.Events;

namespace JobRunnerLabourReport
{
	class Program
	{
		static void Main() {

			Log.Logger = new LoggerConfiguration()
				.Enrich.WithMachineName()
				.Enrich.FromLogContext()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.WithMachineName()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
				.WriteTo.Console()
				//.WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), SERILOG_LOG_FILE)
				.CreateLogger();

			Log.Debug("Job Runner Labour by Dan Saul https://github.com/dsaul");

			string? NPGSQL_CONNECTION_STRING = EnvDatabases.NPGSQL_CONNECTION_STRING;
			if (string.IsNullOrWhiteSpace(NPGSQL_CONNECTION_STRING)) {
				Log.Debug("NPGSQL_CONNECTION_STRING_FILE must be set");
				return;
			}
			string? PGPASSFILE = Environment.GetEnvironmentVariable("PGPASSFILE");
			if (string.IsNullOrWhiteSpace(PGPASSFILE)) {
				Log.Debug("PGPASSFILE must be set");
				return;
			}

			string? S3_PDFLATEX_ACCESS_KEY_FILE = Environment.GetEnvironmentVariable("S3_PDFLATEX_ACCESS_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_PDFLATEX_ACCESS_KEY_FILE)) {
				Log.Debug("S3_PDFLATEX_ACCESS_KEY_FILE must be set");
				return;
			}
			if (string.IsNullOrWhiteSpace(File.ReadAllText(S3_PDFLATEX_ACCESS_KEY_FILE))) {
				Log.Debug("S3_PDFLATEX_ACCESS_KEY_FILE has no contents");
				return;
			}

			string? S3_PDFLATEX_SECRET_KEY_FILE = Environment.GetEnvironmentVariable("S3_PDFLATEX_SECRET_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_PDFLATEX_SECRET_KEY_FILE)) {
				Log.Debug("S3_PDFLATEX_SECRET_KEY_FILE must be set");
				return;
			}
			if (string.IsNullOrWhiteSpace(File.ReadAllText(S3_PDFLATEX_SECRET_KEY_FILE))) {
				Log.Debug("S3_PDFLATEX_SECRET_KEY_FILE has no contents");
				return;
			}

			while (true) {
				//Log.Debug($"Polling...");

				string connectionString = $"{EnvDatabases.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName)}ApplicationName=JobRunnerLabourReport;";
				using NpgsqlConnection jobsDB = new NpgsqlConnection(connectionString);
				//Log.Debug("Postgres Connection String: {ConnectionString}", connectionString);

				try {
					jobsDB.Open();
				}
				catch (Exception ex) {
					Log.Error(ex, "Unable to connect to Database!");
					break;
				}

				JobRunnerJob? job = JobRunnerJob.ClaimJobIfAvailable(jobsDB, JobRunnerJob.kJobTypeValueRunReportLabour);
				if (job != null) {
					RunReportLabour(jobsDB, job);
				}


				jobsDB.Close();

				// Wait before trying agian.
				Thread.Sleep(1000);
			}



		}



		public static void RunReportLabour(
			NpgsqlConnection jobsDB,
			JobRunnerJob job
			) {

			Log.Debug($"RunReportLabour(db, {job.Id})");


			if (null == job.DPDatabase) {
				Log.Debug("job.DPDatabase is null, returning");
				return;
			}


			using NpgsqlConnection pdfLatexDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(PDFLaTeXTask.kPDFLaTeXDBName));
			pdfLatexDB.Open();


			string? error = null;
			string? latex = null;

			do {

				using NpgsqlConnection dpDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(job.DPDatabase));
				dpDB.Open();

				List<Labour> list = new List<Labour>();
				if (null != job.RunOnAllLabour && job.RunOnAllLabour.Value) {
					var res = Labour.All(dpDB);
					list.AddRange(res.Values);
				}

				if (list.Count == 0) {
					error = "No assignments to generate a report on.";
					break;
				}

				IEnumerable<Labour> filtered = list;

				// IF there is an agent id on the job, filter by that.
				if (null != job.AgentId) {
					filtered = filtered.Where((entry) => {
						return job.AgentId == entry.AgentId;
					});
				}


				// If there is an project id on the job, filter by that.
				do {
					if (null == job.ProjectId)
						break;

					HashSet<Guid> projectIds = new HashSet<Guid> {
						job.ProjectId.Value
					};


					if (job.IncludeLabourForOtherProjectsWithMatchingAddresses != null && job.IncludeLabourForOtherProjectsWithMatchingAddresses.Value) {

						// Get specified project.
						var firstProjRes = Projects.ForId(dpDB, job.ProjectId.Value);
						if (firstProjRes.Count == 0)
							break;

						Projects project = firstProjRes.FirstOrDefault().Value;
						if (null == project)
							break;

						List<Address> addresses = project.Addresses;
						foreach (Address addr in addresses) {
							var res = Projects.ForAddressPart(dpDB, addr.Value);
							projectIds.UnionWith(res.Keys);
						}


					}

					filtered = filtered.Where((entry) => {
						if (null == entry.ProjectId)
							return false;

						return projectIds.Contains(entry.ProjectId.Value);
					});


				} while (false);







				
				

				// Start time if present.
				if (!string.IsNullOrWhiteSpace(job.StartISO8601) &&
					!string.IsNullOrWhiteSpace(job.EndISO8601)) {
					filtered = filtered.Where((entry) => {

						if (string.IsNullOrWhiteSpace(entry.StartISO8601)) {
							return false;
						}

						int startComp = ISO8601Compare.Compare(entry.StartISO8601, job.StartISO8601);
						int endComp = ISO8601Compare.Compare(entry.StartISO8601, job.EndISO8601);

						return startComp > 0 && endComp < 0;
					});
				}







				using NpgsqlConnection billingDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(EnvDatabases.BILLING_DATABASE_NAME));
				billingDB.Open();


				latex = LaTeXLabour.Generate(billingDB, dpDB, true, true, filtered).Result;
				if (string.IsNullOrWhiteSpace(latex)) {
					error = "Unable to generate report.";
					break;
				}


				billingDB.Close();
				dpDB.Close();

				//Log.Debug(latex);

			} while (false);

			if (string.IsNullOrWhiteSpace(error)) {

				// Update LaTeX Task with source.
				if (null != job.TaskId) {
					JObject root = new JObject {
						[PDFLaTeXTask.kLaTeXJsonKeyRequestingBillingId] = job.RequestingBillingId.ToString(),
						[PDFLaTeXTask.kLaTeXJsonKeyReportType] = PDFLaTeXTask.kLaTeXJsonReportTypeValueLabour,
						[PDFLaTeXTask.kLaTeXJsonKeyStatus] = PDFLaTeXTask.kLaTeXJsonStatusValueLatexGenerated,
						[PDFLaTeXTask.kLaTeXJsonKeyLatexSource] = latex,
						[PDFLaTeXTask.kLaTeXJsonKeyErrorMessage] = null,
					};

					PDFLaTeXTask.Upsert(pdfLatexDB, new Dictionary<Guid, PDFLaTeXTask> {
						{
							job.TaskId.Value, new PDFLaTeXTask(job.TaskId.Value, root.ToString())
						}
					}, out _, out _);
				}




				// Complete worker job.
				if (null != job.Id && null != job.JsonObject) {
					JObject? jObject = job.JsonObject;
					jObject[JobRunnerJob.kJobsJsonKeyCompleted] = true;

					JobRunnerJob mod = job with
					{
						Json = jObject.ToString()
					};


					JobRunnerJob.Upsert(jobsDB, new Dictionary<Guid, JobRunnerJob> {
						{
							mod.Id.Value, mod
						}
					}, out _, out _);
				}





				// Add compile job.
				if (null != job.TaskId) {
					Guid compileJobId = Guid.NewGuid();
					JobRunnerJob compileJob = new JobRunnerJob(compileJobId, new JObject
					{
						[JobRunnerJob.kJobsJsonKeyJobCreatedISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
						[JobRunnerJob.kJobsJsonKeyExpiresAtISO8601] = DateTime.UtcNow.AddDays(1).ToString("o", Culture.DevelopmentCulture),
						[JobRunnerJob.kJobsJsonKeyJobType] = JobRunnerJob.kJobTypeValuePDFLaTeX,
						[JobRunnerJob.kJobsJsonKeyRequestingBillingId] = job.RequestingBillingId,
						[JobRunnerJob.kJobsJsonKeyTaskId] = job.TaskId.Value,
						[JobRunnerJob.kJobsJsonKeyTaskRunnerClaimedISO8601] = null,
						[JobRunnerJob.kJobsJsonKeyCompleted] = false,
					}.ToString());

					JobRunnerJob.Upsert(jobsDB, new Dictionary<Guid, JobRunnerJob> {
						{
							compileJobId, compileJob
						}
					}, out _, out _);
				}




			} else {

				// Mark latex task as errored.
				if (null != job.TaskId) {
					JObject root = new JObject {
						[PDFLaTeXTask.kLaTeXJsonKeyReportType] = PDFLaTeXTask.kLaTeXJsonReportTypeValueAssignments,
						[PDFLaTeXTask.kLaTeXJsonKeyStatus] = PDFLaTeXTask.kLaTeXJsonStatusValueError,
						[PDFLaTeXTask.kLaTeXJsonKeyLatexSource] = latex,
						[PDFLaTeXTask.kLaTeXJsonKeyErrorMessage] = error,
					};

					PDFLaTeXTask.Upsert(pdfLatexDB, new Dictionary<Guid, PDFLaTeXTask> {
					{
							job.TaskId.Value, new PDFLaTeXTask(job.TaskId.Value, root.ToString())
						}
					}, out _, out _);
				}

			}



			pdfLatexDB.Close();

		}








	}
}
