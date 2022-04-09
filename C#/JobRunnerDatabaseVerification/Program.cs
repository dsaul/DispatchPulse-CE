using SharedCode.DatabaseSchemas;
using SharedCode;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobRunnerDatabaseVerification
{
	class Program
	{
		static void Main(string[] args) {
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

			Log.Debug("Database Verification by Dan Saul https://github.com/dsaul");

			string? dbConnectionPrefix = EnvDatabases.NPGSQL_CONNECTION_STRING;
			if (string.IsNullOrWhiteSpace(dbConnectionPrefix)) {
				Log.Debug("NPGSQL_CONNECTION_STRING_FILE must be set");
				return;
			}
			string? pgpassfile = Environment.GetEnvironmentVariable("PGPASSFILE");
			if (string.IsNullOrWhiteSpace(pgpassfile)) {
				Log.Debug("PGPASSFILE must be set");
				return;
			}


			while (true) {

				string connectionString = $"{EnvDatabases.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName)}ApplicationName=JobRunnerDatabaseVerification;";
				using NpgsqlConnection jobsDB = new NpgsqlConnection(connectionString);
				//Log.Debug("Postgres Connection String: {ConnectionString}", connectionString);

				try {
					jobsDB.Open();
				}
				catch (Exception ex) {
					Log.Error(ex, "Unable to connect to Database!");
					break;
				}

				JobRunnerJob? job = JobRunnerJob.ClaimJobIfAvailable(jobsDB, JobRunnerJob.kJobTypeValueDPDatabaseVerification);
				if (job != null) {
					RunDatabaseVerification(jobsDB, job);
				}


				jobsDB.Close();



				// Wait before trying agian.
				Thread.Sleep(1000);
			}


			
		}


		public static void RunDatabaseVerification(
				NpgsqlConnection jobsDB,
				JobRunnerJob job
			) {

			Dictionary<Guid,BillingCompanies>? allCompanies = null;

			// Fetch all companies.
			{
				using NpgsqlConnection billingDB = new NpgsqlConnection(EnvDatabases.KBillingDatabaseConnectionString);
				billingDB.Open();

				allCompanies = BillingCompanies.All(billingDB);
				if (allCompanies.Count == 0) {
					Log.Debug("--- Error: No companies recieved from database.");
					return;
				}

				billingDB.Close();
			}


			List<Task> tasks = new List<Task>();

			foreach (KeyValuePair<Guid, BillingCompanies> kvp in allCompanies) {
				tasks.Add(VerifyCompany(kvp.Value));
			}

			Task.WhenAll(tasks).Wait();

			//#error delete job when done

			using NpgsqlConnection jobsDB2 = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName));
			jobsDB2.Open();

			// Complete worker job.

			if (null != job.Id) {
				JObject? jObject = job.JsonObject;
				if (null != jObject) {
					jObject[JobRunnerJob.kJobsJsonKeyCompleted] = true;

					JobRunnerJob mod = job with
					{
						Json = jObject.ToString()
					};


					JobRunnerJob.Upsert(jobsDB2, new Dictionary<Guid, JobRunnerJob> {
						{
							mod.Id.Value, mod
						}
					}, out _, out _);
				}

			}

			jobsDB2.Close();

		}

		static async Task VerifyCompany(BillingCompanies company) {

			//if (company.FullName != "Dispatch Pulse") {
			//	Log.Debug($"--- {company.FullName} Skipped because is not Dispatch Pulse.");
			//	return;
			//}

			if (null == company.Uuid) {
				Log.Error("Company.Uuid is null, skipping.");
				return;
			}

			Log.Debug($"--- Verifying Company \"{company.FullName}\"");

			using NpgsqlConnection billingDB = new NpgsqlConnection(EnvDatabases.KBillingDatabaseConnectionString);
			billingDB.Open();

			HashSet<string> dpDatabases = new HashSet<string>();

			Dictionary<Guid, BillingSubscriptions> allSubs = BillingSubscriptions.ForCompanyId(billingDB, company.Uuid.Value);
			foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in allSubs) {

				if (null == kvp.Value)
					continue;
				if (string.IsNullOrWhiteSpace(kvp.Value.ProvisionedDatabaseName))
					continue;

				string databaseName = kvp.Value.ProvisionedDatabaseName;
				if (string.IsNullOrWhiteSpace(databaseName))
					continue;

				Guid? packageId = kvp.Value.PackageId;
				if (null == packageId)
					continue;

				BillingPackages pkg = BillingPackages.ForId(billingDB, packageId.Value).FirstOrDefault().Value;
				if (null == pkg)
					continue;

				if (pkg.ProvisionDispatchPulse == true) {
					dpDatabases.Add(databaseName);
				}

				// If it was another type add that here.
			}

			List<Task> tasksToAwait = new List<Task>();
			foreach (string dpDBName in dpDatabases) {
				tasksToAwait.Add(DPVerify.Verify(dpDBName));
			}

			await Task.WhenAll(tasksToAwait);

			billingDB.Close();
		}
	}
}
