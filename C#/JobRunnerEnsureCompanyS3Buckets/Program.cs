using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Databases.Records.Billing;
using Databases.Records.JobRunner;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;
using Serilog.Events;

namespace JobRunnerEnsureCompanyS3Buckets
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

			Log.Debug("Ensure Company S3 Buckets by Dan Saul https://github.com/dsaul");

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

			string? S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE = Environment.GetEnvironmentVariable("S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE)) {
				Log.Debug("S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE must be set");
				return;
			}
			if (string.IsNullOrWhiteSpace(File.ReadAllText(S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE))) {
				Log.Debug("S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE has no contents");
				return;
			}

			string? S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE = Environment.GetEnvironmentVariable("S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE)) {
				Log.Debug("S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE must be set");
				return;
			}
			if (string.IsNullOrWhiteSpace(File.ReadAllText(S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE))) {
				Log.Debug("S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE has no contents");
				return;
			}

			

			while (true) {
				//Log.Debug($"Polling...");

				string connectionString = $"{Databases.Konstants.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName)}ApplicationName=JobRunnerEnsureCompanyS3Buckets;";
				using NpgsqlConnection jobsDB = new NpgsqlConnection(connectionString);
				//Log.Debug("Postgres Connection String: {ConnectionString}", connectionString);

				try {
					jobsDB.Open();
				}
				catch (Exception ex) {
					Log.Error(ex, "Unable to connect to Database!");
					break;
				}

				JobRunnerJob? job = JobRunnerJob.ClaimJobIfAvailable(jobsDB, JobRunnerJob.kJobTypeValueEnsureCompanyS3Buckets);
				if (job != null) {
					EnsureCompanyS3Buckets(jobsDB, job);
				}


				jobsDB.Close();

				// Wait 2 seconds before trying agian.
				Thread.Sleep(1000);
			}


		}

		static void EnsureCompanyS3Buckets(
			NpgsqlConnection jobsDB,
			JobRunnerJob job
			) {

			Log.Debug($"EnsureCompanyS3Buckets({job.Id})");

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

			string? S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE = Environment.GetEnvironmentVariable("S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE)) {
				Log.Debug("S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE must be set");
				return;
			}

			string? S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY = File.ReadAllText(S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE);
			if (string.IsNullOrWhiteSpace(S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY)) {
				Log.Debug("S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY_FILE has no contents");
				return;
			}

			string? S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE = Environment.GetEnvironmentVariable("S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE)) {
				Log.Debug("S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE must be set");
				return;
			}

			string? S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY = File.ReadAllText(S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE);
			if (string.IsNullOrWhiteSpace(S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY)) {
				Log.Debug("S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY_FILE has no contents");
				return;
			}

			using NpgsqlConnection billingDB = new NpgsqlConnection(Databases.Konstants.KBillingDatabaseConnectionString);
			billingDB.Open();

			var config = new AmazonS3Config
			{
				RegionEndpoint = RegionEndpoint.USWest1,
				ServiceURL = SharedCode.S3.Konstants.S3_DISPATCH_PULSE_SERVICE_URI,
				ForcePathStyle = true
			};
			var s3Client = new AmazonS3Client(S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY, S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY, config);
			Log.Debug("S3 Client {ACCESS_KEY} {SECRET_KEY} {SERVICE_URI}",
				S3_MANAGE_CLIENT_BUCKETS_ACCESS_KEY, S3_MANAGE_CLIENT_BUCKETS_SECRET_KEY, SharedCode.S3.Konstants.S3_DISPATCH_PULSE_SERVICE_URI);




			Dictionary<Guid,BillingCompanies> resAllCompanies = BillingCompanies.All(billingDB);
			if (resAllCompanies.Count == 0) {
				Log.Debug("--- Error: No companies recieved from database.");
				return;
			}

			foreach(KeyValuePair<Guid, BillingCompanies> kvp in resAllCompanies) {
				CancellationToken token = new CancellationToken();
				CheckCompany(billingDB, kvp.Value, s3Client, token).Wait();
			}









			// Complete worker job.

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














			billingDB.Close();


		}

		static async Task CheckCompany(
			NpgsqlConnection billingDB, 
			BillingCompanies company, 
			AmazonS3Client s3Client,
			CancellationToken token
			) {

			if (company.Uuid == null) {
				Log.Debug($"company.Uuid == null (for {company.FullName}) returning");
				return;
			}

			if (company.JsonObject == null) {
				Log.Debug("Company json is null returning.");
				return;
			}

			var bucketname = string.IsNullOrWhiteSpace(company.S3BucketName) ? $"snp-client-{company.Uuid}" : company.S3BucketName;
			if (await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketname)) {
				Log.Debug($"Bucket exists for {company.FullName}, skipping");
				return;
			}

			await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketname, UseClientRegion = true }, token);

			JObject? json = company.JsonObject.DeepClone() as JObject;
			if (null == json) {
				Log.Debug("Unable to clone company json returning.");
				return;
			}
			json[BillingCompanies.kJsonKeyS3BucketName] = bucketname;

			company = company with
			{
				Json = json.ToString()
			};

			BillingCompanies.Upsert(billingDB, new Dictionary<Guid, BillingCompanies> {
				{ company.Uuid.Value, company }
			}, out _, out _);


		}



	}
}
