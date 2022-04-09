using SharedCode.DatabaseSchemas;
using SharedCode;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading;

namespace JobRunnerRemoveExpiredJobs
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


			Log.Debug("JobRunnerRemoveExpiredJobs by Dan Saul https://github.com/dsaul");

			string? NPGSQL_CONNECTION_STRING = EnvDatabases.NPGSQL_CONNECTION_STRING;
			if (string.IsNullOrWhiteSpace(NPGSQL_CONNECTION_STRING)) {
				Log.Debug("NPGSQL_CONNECTION_STRING_FILE must be set");
				return;
			}
			string? pgpassfile = Environment.GetEnvironmentVariable("PGPASSFILE");
			if (string.IsNullOrWhiteSpace(pgpassfile)) {
				Log.Debug("PGPASSFILE must be set");
				return;
			}


			while (true) {

				string connectionString = $"{EnvDatabases.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName)}ApplicationName=JobRunnerRemoveExpiredJobs;";
				using NpgsqlConnection jobsDB = new NpgsqlConnection(connectionString);
				//Log.Debug("Postgres Connection String: {ConnectionString}", connectionString); 
				
				try {
					jobsDB.Open();
				}
				catch (Exception ex) {
					Log.Error(ex, "Unable to connect to Database!");
					break;
				}

				JobRunnerJob? job = JobRunnerJob.ClaimJobIfAvailable(jobsDB, JobRunnerJob.kJobTypeValueJobRunnerRemoveExpiredJobs);
				if (job != null) {
					RemoveExpiredJobs(jobsDB, job);
				}


				jobsDB.Close();



				// Wait before trying agian.
				Thread.Sleep(1000);
			}



		}


		public static void RemoveExpiredJobs(
				NpgsqlConnection jobsDB,
				JobRunnerJob job
			) {


			int rowsAffected = JobRunnerJob.DeleteExpiredJobs(jobsDB);
			Log.Debug($"Removed {rowsAffected} expired jobs.");

			// Complete worker job.

			try {
				if (null != job.Id) {
					JObject? jObject = job.JsonObject;
					if (null != jObject) {
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
			catch {
				Log.Debug("Exception while completing task to remove expired jobs, job was probably expired as well.");
			}



		}





	}
}
