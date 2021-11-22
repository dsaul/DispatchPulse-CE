using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Databases.Records.JobRunner;
using Amazon.S3;
using Amazon;
using Amazon.S3.Transfer;
using Databases.Records.CRM;
using LaTeXGenerators;
using Databases.Records.PDFLaTeX;
using System.Globalization;
using Serilog;
using Serilog.Events;

namespace JobRunnerAssignmentsReport
{
	class Program
	{
		static void Main()
		{
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


			Log.Debug("Job Runner Assignments by Dan Saul https://github.com/dsaul");

			string? dbConnectionPrefix = Databases.Konstants.NPGSQL_CONNECTION_STRING;
			if (string.IsNullOrWhiteSpace(dbConnectionPrefix)) {
				Log.Debug("NPGSQL_CONNECTION_STRING_FILE must be set");
				return;
			}
			string? pgpassfile = Environment.GetEnvironmentVariable("PGPASSFILE");
			if (string.IsNullOrWhiteSpace(pgpassfile)) {
				Log.Debug("PGPASSFILE must be set");
				return;
			}

			string? s3PDFLaTeXAccessKeyFile = Environment.GetEnvironmentVariable("S3_PDFLATEX_ACCESS_KEY_FILE");
			if (string.IsNullOrWhiteSpace(s3PDFLaTeXAccessKeyFile)) {
				Log.Debug("S3_PDFLATEX_ACCESS_KEY_FILE must be set");
				return;
			}
			if (string.IsNullOrWhiteSpace(File.ReadAllText(s3PDFLaTeXAccessKeyFile))) {
				Log.Debug("S3_PDFLATEX_ACCESS_KEY_FILE has no contents");
				return;
			}

			string? s3PDFLaTeXSecretKeyFile = Environment.GetEnvironmentVariable("S3_PDFLATEX_SECRET_KEY_FILE");
			if (string.IsNullOrWhiteSpace(s3PDFLaTeXSecretKeyFile)) {
				Log.Debug("S3_PDFLATEX_SECRET_KEY_FILE must be set");
				return;
			}
			if (string.IsNullOrWhiteSpace(File.ReadAllText(s3PDFLaTeXSecretKeyFile))) {
				Log.Debug("S3_PDFLATEX_SECRET_KEY_FILE has no contents");
				return;
			}

			while (true) {
				//Log.Debug($"Polling...");

				string connectionString = $"{Databases.Konstants.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName)}ApplicationName=JobRunnerAssignmentsReport;";
				using NpgsqlConnection jobsDB = new NpgsqlConnection(connectionString);
				//Log.Debug("Postgres Connection String: {ConnectionString}", connectionString);

				try {
					jobsDB.Open();
				}
				catch (Exception ex) {
					Log.Error(ex, "Unable to connect to Database!");
					break;
				}


				JobRunnerJob? job = JobRunnerJob.ClaimJobIfAvailable(jobsDB, JobRunnerJob.kJobTypeValueRunReportAssignment);
				if (job != null) {
					RunReportAssignments(jobsDB, job);
				}

				jobsDB.Close();

				// Wait before trying agian.
				Thread.Sleep(1000);
			}



		}



		public static void RunReportAssignments(
			NpgsqlConnection jobsDB,
			JobRunnerJob job
			) {

			Log.Debug($"RunReportAssignments(db, {job.Id})");


			if (null == job.DPDatabase) {
				Log.Debug("job.DPDatabase is null, returning");
				return;
			}


			using NpgsqlConnection pdfLatexDB = new NpgsqlConnection($"{Databases.Konstants.DatabaseConnectionStringForDB(PDFLaTeXTask.kPDFLaTeXDBName)}ApplicationName=AssignmentsReport;");
			pdfLatexDB.Open();


			string? error = null;
			string? latex = null;

			do {

				using NpgsqlConnection dpDB = new NpgsqlConnection($"{Databases.Konstants.DatabaseConnectionStringForDB(job.DPDatabase)}ApplicationName=AssignmentsReport;");
				dpDB.Open();

				List<Assignments> list = new List<Assignments>();
				if (null != job.RunOnAllAssignments && job.RunOnAllAssignments.Value) {
					var res = Assignments.All(dpDB);
					list.AddRange(res.Values);
				} else {
					var res = Assignments.ForIds(dpDB, job.AssignmentIds);
					list.AddRange(res.Values);
				}

				if (list.Count == 0) {
					error = "No assignments to generate a report on.";
					break;
				}

				using NpgsqlConnection billingDB = new NpgsqlConnection($"{Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.KBillingDatabaseName)}ApplicationName=AssignmentsReport;");
				billingDB.Open();


				latex = LaTeXAssignments.Generate(billingDB, dpDB, true, true, list);
				if (string.IsNullOrWhiteSpace(latex)) {
					error = "Unable to generate report.";
					break;
				}


				dpDB.Close();
				billingDB.Close();


				//Log.Debug(latex);

			} while (false);

			if (string.IsNullOrWhiteSpace(error)) {

				// Update LaTeX Task with source.
				if (null != job.TaskId) {
					JObject root = new JObject {
						[PDFLaTeXTask.kLaTeXJsonKeyRequestingBillingId] = job.RequestingBillingId.ToString(),
						[PDFLaTeXTask.kLaTeXJsonKeyReportType] = PDFLaTeXTask.kLaTeXJsonReportTypeValueAssignments,
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
						[JobRunnerJob.kJobsJsonKeyJobCreatedISO8601] = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture),
						[JobRunnerJob.kJobsJsonKeyExpiresAtISO8601] = DateTime.UtcNow.AddDays(1).ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture),
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
