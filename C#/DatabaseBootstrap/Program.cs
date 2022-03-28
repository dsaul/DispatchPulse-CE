using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using SharedCode.Extensions;
using Serilog.Events;
using SharedCode.Databases.Records;

namespace DatabaseBootstrap
{
	public class Program
	{
		public static void Main(string[] args) {

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

			Log.Information("Dispatch Pulse Database Bootstrap");


			if (File.Exists("/data/completed") {
				Log.Debug("Idling");
				while (true) {
					Thread.Sleep(1000);
				}
			}



			using NpgsqlConnection? noDatabaseConnection = new NpgsqlConnection(Databases.Konstants.NPGSQL_CONNECTION_STRING);

			try {
				noDatabaseConnection.Open();
			}
			catch (Exception ex) {
				Log.Error(ex, "Unable to connect to database.");
				return;
			}

			// Make sure that the job_runner database exists.
			string? jobRunnerDatabaseName = "job_runner";
			if (false == noDatabaseConnection.DatabaseExists(jobRunnerDatabaseName)) {
				Log.Warning("{DatabaseName} database doesn't exist, creating.", jobRunnerDatabaseName);
				jobRunnerDatabaseName = noDatabaseConnection.CreateDatabase(
					dbName: jobRunnerDatabaseName,
					prefix: "",
					suffixBeforeNumber:"", 
					noNumberIteration: true
				);
			}

			// Make sure that the pdflatex database exists.
			string? pdflatexDatabaseName = "pdflatex";
			if (false == noDatabaseConnection.DatabaseExists(pdflatexDatabaseName)) {
				Log.Warning("{DatabaseName} database doesn't exist, creating.", pdflatexDatabaseName);
				pdflatexDatabaseName = noDatabaseConnection.CreateDatabase(
					dbName: pdflatexDatabaseName,
					prefix: "",
					suffixBeforeNumber: "",
					noNumberIteration: true
				);
			}


			// Make sure that the billing database exists.
			string? billingDatabaseName = "billing";
			if (false == noDatabaseConnection.DatabaseExists(billingDatabaseName)) {
				Log.Warning("{DatabaseName} database doesn't exist, creating.", billingDatabaseName);
				billingDatabaseName = noDatabaseConnection.CreateDatabase(
					dbName: billingDatabaseName,
					prefix: "",
					suffixBeforeNumber: "",
					noNumberIteration: true
				);
			}


			// Make sure that the tts database exists.
			string? ttsDatabaseName = "tts";
			if (false == noDatabaseConnection.DatabaseExists(ttsDatabaseName)) {
				Log.Warning("{DatabaseName} database doesn't exist, creating.", ttsDatabaseName);
				ttsDatabaseName = noDatabaseConnection.CreateDatabase(
					dbName: ttsDatabaseName,
					prefix: "",
					suffixBeforeNumber: "",
					noNumberIteration: true
				);
			}


			noDatabaseConnection.Close();

			// Run verification on job_runner
			if (!string.IsNullOrWhiteSpace(jobRunnerDatabaseName)) {
				using NpgsqlConnection db = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(jobRunnerDatabaseName));

				db.Open();

				db.EnsureUUIDExtension();
				db.EnsureTimestampISO8601();

				Verification.VerifyJobRunnerDatabase(db, true);

				db.Close();
			}

			// Run verification on pdflatex
			if (!string.IsNullOrWhiteSpace(pdflatexDatabaseName)) {
				using NpgsqlConnection db = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(pdflatexDatabaseName));

				db.Open();

				db.EnsureUUIDExtension();
				db.EnsureTimestampISO8601();

				Verification.VerifyPDFLaTeXDatabase(db, true);

				db.Close();
			}

			// Run verification on billing
			if (!string.IsNullOrWhiteSpace(billingDatabaseName)) {
				using NpgsqlConnection db = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(billingDatabaseName));

				db.Open();

				db.EnsureUUIDExtension();
				db.EnsureTimestampISO8601();

				Verification.VerifyBillingDatabase(db, true);

				db.Close();
			}

			// Run verification on tts
			if (!string.IsNullOrWhiteSpace(ttsDatabaseName)) {
				using NpgsqlConnection db = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(ttsDatabaseName));

				db.Open();

				db.EnsureUUIDExtension();
				db.EnsureTimestampISO8601();

				Verification.VerifyJobRunnerDatabase(db, true);

				db.Close();
			}


			noDatabaseConnection.Close();

			File.WriteAllText("/data/completed", DateTime.UtcNow.ToString("o"));

			// We don't want docker to keep relaunching this program,
			// so if it got to this point, sleep repeatedly forever.
			Log.Debug("Idling");
			while (true) {
				Thread.Sleep(1000);
			}


		}
	}
}