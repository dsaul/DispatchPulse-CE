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
using Databases.Records.CRM;
using Databases.Records.PDFLaTeX;
using NCrontab;
using System.Globalization;
using Serilog;
using Serilog.Events;

namespace RecurringTaskScheduler
{
	class Program
	{
		static void Main(string[] args)
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

			Log.Debug("Recurring Task Scheduler by Dan Saul https://github.com/dsaul");

			string? NPGSQL_CONNECTION_STRING = Databases.Konstants.NPGSQL_CONNECTION_STRING;
			if (string.IsNullOrWhiteSpace(NPGSQL_CONNECTION_STRING)) {
				Log.Debug("NPGSQL_CONNECTION_STRING_FILE must be set");
				return;
			}
			string? PGPASSFILE = Environment.GetEnvironmentVariable("PGPASSFILE");
			if (string.IsNullOrWhiteSpace(PGPASSFILE)) {
				Log.Debug("PGPASSFILE must be set");
				return;
			}


			while (true) {
				//Log.Debug($"Polling...");

				string connectionString = $"{Databases.Konstants.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName)}ApplicationName=JobRunnerRecurringTaskScheduler;";
				using NpgsqlConnection jobsDB = new NpgsqlConnection(connectionString);
				//Log.Debug("Postgres Connection String: {ConnectionString}", connectionString);

				try {
					jobsDB.Open();
				}
				catch (Exception ex) {
					Log.Error(ex, "Unable to connect to Database!");
					break;
				}

				var resAllScheduledTasks = ScheduledTasks.All(jobsDB);
				if (resAllScheduledTasks.Count == 0) {
					jobsDB.Close();
					Thread.Sleep(1000 * 60);
					continue;
				}

				foreach (KeyValuePair<Guid, ScheduledTasks> kvp in resAllScheduledTasks) {

					ProcessScheduledTask(jobsDB, kvp.Value);


				}



				jobsDB.Close();

				// Wait before trying agian.
				Thread.Sleep(1000*60);
			}

		}


		static void ProcessScheduledTask(NpgsqlConnection db, ScheduledTasks task) {

			Log.Debug($"Scheduled Task: '{task.Description}' '{task.CrontabExpression}' '{task.LastJobDispatchedTimestampISO8601}'");

			if (null == task.JsonObject) {
				Log.Debug("This task has no json.");
				return;
			}

			if (null == task.NewTaskJsonObject) {
				Log.Debug("This task has no new task json.");
				return;
			}

			if (null == task.Id) {
				Log.Debug("Task has no id.");
				return;
			}

			if (false == task.AllowMoreThanOneActive) {

				string? jobType = task.NewTaskJsonObject.Value<string?>(JobRunnerJob.kJobsJsonKeyJobType);
				if (null == jobType) {
					Log.Debug("null == jobType");
					return;
				}

				var res = JobRunnerJob.FreeJobsForJobType(db, jobType);
				if (res.Count != 0) {
					Log.Debug($"Task already exists, skipping.");
					return;
				}
			}





			CrontabSchedule? schedule;
			try {
				schedule = CrontabSchedule.Parse(task.CrontabExpression);
			}
			catch {
				Log.Debug("Error Parsing Crontab");
				return;
			}

			DateTime? lastOccurance = string.IsNullOrWhiteSpace(task.LastJobDispatchedTimestampISO8601) ? null : DateTime.Parse(task.LastJobDispatchedTimestampISO8601);
			DateTime? nextOccurance = schedule.GetNextOccurrence(DateTime.Now);

			if (
				(lastOccurance == null && nextOccurance < DateTime.Now.AddMinutes(1)) ||
				(lastOccurance != null && nextOccurance < DateTime.Now.AddMinutes(1) && DateTime.Now > lastOccurance.Value.AddMinutes(1))
				) {

				// Create job.
				JObject? newTaskJson = task.NewTaskJsonObject.DeepClone() as JObject;
				if (null == newTaskJson) {
					Log.Debug("null == newTaskJson");
					return;
				}

				newTaskJson[JobRunnerJob.kJobsJsonKeyJobCreatedISO8601] = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
				newTaskJson[JobRunnerJob.kJobsJsonKeyExpiresAtISO8601] = DateTime.UtcNow.AddDays(1).ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
				newTaskJson[JobRunnerJob.kJobsJsonKeyTaskRunnerClaimedISO8601] = null;
				newTaskJson[JobRunnerJob.kJobsJsonKeyCompleted] = false;

				Guid jobId = Guid.NewGuid();
				JobRunnerJob job = new JobRunnerJob(jobId, newTaskJson.ToString());

				JobRunnerJob.Upsert(db, new Dictionary<Guid, JobRunnerJob> {
					{
						jobId, job
					}
				}, out _, out _);

				Log.Debug($"Added Job {jobId}");


				JObject? jsonMod = task.JsonObject.DeepClone() as JObject;
				if (null == jsonMod) {
					Log.Debug("null == jsonMod");
					return;
				}
				jsonMod[ScheduledTasks.kJobsJsonKeyLastJobDispatchedTimestampISO8601] = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);

				ScheduledTasks mod = task with
				{
					Json = jsonMod.ToString()
				};

				ScheduledTasks.Upsert(db, new Dictionary<Guid, ScheduledTasks> {
					{
						task.Id.Value, mod
					}
				}, out _, out _);
			}

		}



	}
}
