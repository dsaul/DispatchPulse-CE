using Databases.Records.JobRunner;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using Serilog;
using API.Hubs;
using System.Threading.Tasks;
using Databases.Records.CRM;
using Databases.Records.Billing;

namespace JobRunnerUpdateWebCalFiles
{
	class Program
	{
		static async Task Main()
		{
			Log.Logger = new LoggerConfiguration()
				.Enrich.WithMachineName()
				.Enrich.FromLogContext()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.WithMachineName()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.CreateLogger();

			Log.Information("Job Runner Update Web Cal Files by Dan Saul https://github.com/dsaul");

			string? NPGSQL_CONNECTION_STRING = Databases.Konstants.NPGSQL_CONNECTION_STRING;
			if (string.IsNullOrWhiteSpace(NPGSQL_CONNECTION_STRING)) {
				Log.Debug("NPGSQL_CONNECTION_STRING_FILE must be set");
				return;
			}
			string? pgpassfile = Environment.GetEnvironmentVariable("PGPASSFILE");
			if (string.IsNullOrWhiteSpace(pgpassfile)) {
				Log.Error("PGPASSFILE must be set");
				return;
			}

			while (true) {
				//Log.Debug($"Polling...");

				string connectionString = $"{Databases.Konstants.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName)}ApplicationName=JobRunnerUpdateWebCalFiles;";
				using NpgsqlConnection jobsDB = new NpgsqlConnection(connectionString);
				//Log.Debug("Postgres Connection String: {ConnectionString}", connectionString); 
				
				try {
					jobsDB.Open();
				}
				catch (Exception ex) {
					Log.Error(ex, "Unable to connect to Database!");
					break;
				}

				JobRunnerJob? job = JobRunnerJob.ClaimJobIfAvailable(jobsDB, JobRunnerJob.kJobTypeValueUpdateWebCalFiles);
				if (job != null) {
					await RunUpdateWebCalFiles(jobsDB, job);
				}


				jobsDB.Close();

				// Wait before trying agian.
				Thread.Sleep(1000);
			}












		}







		public static async Task RunUpdateWebCalFiles(
			NpgsqlConnection jobsDB,
			JobRunnerJob job
			) {

			Log.Information($"RunUpdateWebCalFiles(db, {job.Id})");



			bool isError = false;
			do {

				// Do Task

				// Connect to 
				using NpgsqlConnection? billingConnection = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
				billingConnection.Open();

				var resBC = BillingCompanies.AllForProvisionOnCallAutoAttendants(billingConnection, true, out Dictionary<Guid, string> databaseNames);
				if (resBC.Count == 0) {
					Log.Warning("Found no companies that provision on call auto attendants.");
					isError = true;
					break;
				}

				foreach (KeyValuePair<Guid, BillingCompanies> kvp in resBC) {

					string dbName = databaseNames[kvp.Key];
					if (string.IsNullOrWhiteSpace(dbName)) {
						Log.Warning("dbName is null or whitespace company id is {CompanyId}", kvp.Key);
						continue;
					}

					using NpgsqlConnection? dpDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(dbName));
					dpDB.Open();

					var resCalendars = Calendars.All(dpDB);
					foreach (KeyValuePair<Guid, Calendars> calKvp in resCalendars) {

						IdempotencyResponse response = new IdempotencyResponse();
						await Calendars.RetrieveCalendar(dpDB, response, calKvp.Key);

						if (response != null && response.IsError != null && response.IsError.Value) {
							Log.Error("Error in RetrieveCalendar for id {Id} error:{Error}", calKvp.Key, response.ErrorMessage);

							continue;
						}



					}

					dpDB.Close();

				}



				//isError = response.IsError;
				billingConnection.Close();


			} while (false);

			if (isError) {

				// Mark latex task as errored.
				Log.Error("Task errored.");

			} else {
				Log.Information("Task success.");
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






		}

















	}
}
