using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using SharedCode.DatabaseSchemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SharedCode;

namespace JobRunnerPDFLaTeX
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

			Log.Debug($"Job Runner PdfLaTeX by Dan Saul https://github.com/dsaul");

			string? NPGSQL_CONNECTION_STRING = EnvDatabases.NPGSQL_CONNECTION_STRING;
			if (string.IsNullOrWhiteSpace(NPGSQL_CONNECTION_STRING)) {
				Log.Debug("NPGSQL_CONNECTION_STRING_FILE must be set");
				return;
			}
			string? PGPASSFILE = System.Environment.GetEnvironmentVariable("PGPASSFILE");
			if (string.IsNullOrWhiteSpace(PGPASSFILE)) {
				Log.Debug("PGPASSFILE must be set");
				return;
			}

			string? S3_PDFLATEX_ACCESS_KEY_FILE = System.Environment.GetEnvironmentVariable("S3_PDFLATEX_ACCESS_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_PDFLATEX_ACCESS_KEY_FILE)) {
				Log.Debug("S3_PDFLATEX_ACCESS_KEY_FILE must be set");
				return;
			}
			if (string.IsNullOrWhiteSpace(File.ReadAllText(S3_PDFLATEX_ACCESS_KEY_FILE))) {
				Log.Debug("S3_PDFLATEX_ACCESS_KEY_FILE has no contents");
				return;
			}

			string? S3_PDFLATEX_SECRET_KEY_FILE = System.Environment.GetEnvironmentVariable("S3_PDFLATEX_SECRET_KEY_FILE");
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

				string connectionString = $"{EnvDatabases.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName)}ApplicationName=JobRunnerPDFLaTeX;";
				using NpgsqlConnection jobsDB = new NpgsqlConnection(connectionString);
				//Log.Debug("Postgres Connection String: {ConnectionString}", connectionString);

				try {
					jobsDB.Open();
				}
				catch (Exception ex) {
					Log.Error(ex, "Unable to connect to Database!");
					break;
				}

				JobRunnerJob? job = JobRunnerJob.ClaimJobIfAvailable(jobsDB, JobRunnerJob.kJobTypeValuePDFLaTeX);
				if (job != null) {
					PDFLaTeXRunTask(jobsDB, job);
				}


				jobsDB.Close();

				// Wait 2 seconds before trying agian.
				Thread.Sleep(1000);
			}
		}

		static void PDFLaTeXRunTask(
			NpgsqlConnection jobsDB, 
			JobRunnerJob job
			) {

			Log.Debug($"LaTeXTask.PDFLaTeXRunTask({job.TaskId})");

			string? S3_PDFLATEX_ACCESS_KEY_FILE = System.Environment.GetEnvironmentVariable("S3_PDFLATEX_ACCESS_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_PDFLATEX_ACCESS_KEY_FILE)) {
				Log.Debug("S3_PDFLATEX_ACCESS_KEY_FILE must be set");
				return;
			}
			string? S3_PDFLATEX_ACCESS_KEY = File.ReadAllText(S3_PDFLATEX_ACCESS_KEY_FILE);
			if (string.IsNullOrWhiteSpace(S3_PDFLATEX_ACCESS_KEY)) {
				Log.Debug("S3_PDFLATEX_ACCESS_KEY_FILE has no contents");
				return;
			}

			string? S3_PDFLATEX_SECRET_KEY_FILE = System.Environment.GetEnvironmentVariable("S3_PDFLATEX_SECRET_KEY_FILE");
			if (string.IsNullOrWhiteSpace(S3_PDFLATEX_SECRET_KEY_FILE)) {
				Log.Debug("S3_PDFLATEX_SECRET_KEY_FILE must be set");
				return;
			}
			string? S3_PDFLATEX_SECRET_KEY = File.ReadAllText(S3_PDFLATEX_SECRET_KEY_FILE);
			if (string.IsNullOrWhiteSpace(File.ReadAllText(S3_PDFLATEX_SECRET_KEY_FILE))) {
				Log.Debug("S3_PDFLATEX_SECRET_KEY_FILE has no contents");
				return;
			}

			using NpgsqlConnection pdfLatexDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(PDFLaTeXTask.kPDFLaTeXDBName));
			pdfLatexDB.Open();

			if (null == job.TaskId) {
				Log.Debug("There is no task ID for this job.");
				return;
			}

			var resTask = PDFLaTeXTask.ForId(pdfLatexDB, job.TaskId.Value);
			if (resTask.Count == 0) {
				Log.Debug($"A task for ID {job.TaskId} doesn't exist, skipping.");
				return;
			}

			PDFLaTeXTask task = resTask.FirstOrDefault().Value;
			if (task.Status != PDFLaTeXTask.kLaTeXJsonStatusValueLatexGenerated) {
				Log.Debug("PDFLaTeXRunTask called on id that isn't status == latex");
				return;
			}


			JObject? json = task.JsonObject;
			if (null == json) {
				Log.Debug("No JSON Object");
				return;
			}

			string? latex = task.LaTeXSource;
			if (null == latex) {
				Log.Debug("No latex.");
				return;
			}

			string tmpDirPath = Path.GetTempPath();
			string jobDir = Path.Join(tmpDirPath, job.TaskId.ToString());

			if (!Directory.Exists(jobDir)) {
				Directory.CreateDirectory(jobDir);
			}

			string filenameTEX = Path.Join(jobDir, $"{job.TaskId}.tex");
			string filenamePDF = Path.Join(jobDir, $"{job.TaskId}.pdf");
			string filenameAUX = Path.Join(jobDir, $"{job.TaskId}.aux");
			string filenameLOG = Path.Join(jobDir, $"{job.TaskId}.log");

			File.WriteAllText(filenameTEX, latex);
			json.Remove(PDFLaTeXTask.kLaTeXJsonKeyLatexSource);

			// We need to run pdflatex twice...

			StringBuilder stdOutBuff = new StringBuilder();
			StringBuilder stdErrBuff = new StringBuilder();

			for (int i = 0; i < 2; i++) {
				Process proc = new Process {
					StartInfo = new ProcessStartInfo {
						FileName = "pdflatex",
						Arguments = $"-interaction nonstopmode -halt-on-error -file-line-error -output-directory \"{jobDir}\" \"{filenameTEX}\"",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						CreateNoWindow = true
					}
				};
				proc.Start();

				while (!proc.StandardOutput.EndOfStream) {
					string? line = proc.StandardOutput.ReadLine();
					stdOutBuff.Append(line);
					stdOutBuff.Append('\n');
				}
				while (!proc.StandardError.EndOfStream) {
					string? line = proc.StandardError.ReadLine();
					stdErrBuff.Append(line);
					stdErrBuff.Append('\n');
				}
			}

			var config = new AmazonS3Config
			{
				RegionEndpoint = RegionEndpoint.USWest1,
				ServiceURL = EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI,
				ForcePathStyle = true
			};
			var s3Client = new AmazonS3Client(S3_PDFLATEX_ACCESS_KEY, S3_PDFLATEX_SECRET_KEY, config);
			var fileTransferUtility = new TransferUtility(s3Client);


			try {
				using MemoryStream stream = new MemoryStream();
				var writer = new StreamWriter(stream);
				writer.Write(stdOutBuff.ToString());
				writer.Flush();
				stream.Position = 0;

				fileTransferUtility.Upload(stream, PDFLaTeXTask.kLaTeXBucketName, $"{job.TaskId}/{job.TaskId}.stdout.txt");
			}
			catch (Exception e) {
				Log.Debug($"Error encountered on server. Message:'{e.Message}' when writing an object");
			}

			try {
				using MemoryStream stream = new MemoryStream();
				var writer = new StreamWriter(stream);
				writer.Write(stdErrBuff.ToString());
				writer.Flush();
				stream.Position = 0;

				fileTransferUtility.Upload(stream, PDFLaTeXTask.kLaTeXBucketName, $"{job.TaskId}/{job.TaskId}.stderr.txt");
			}
			catch (Exception e) {
				Log.Debug($"Error encountered on server. Message:'{e.Message}' when writing an object");
			}

			json[PDFLaTeXTask.kLaTeXS3URITex] = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{PDFLaTeXTask.kLaTeXBucketName}/{job.TaskId}/{job.TaskId}.tex";
			json[PDFLaTeXTask.kLaTeXS3URIPdf] = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{PDFLaTeXTask.kLaTeXBucketName}/{job.TaskId}/{job.TaskId}.pdf";
			json[PDFLaTeXTask.kLaTeXS3URIAux] = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{PDFLaTeXTask.kLaTeXBucketName}/{job.TaskId}/{job.TaskId}.aux";
			json[PDFLaTeXTask.kLaTeXS3URILog] = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{PDFLaTeXTask.kLaTeXBucketName}/{job.TaskId}/{job.TaskId}.log";
			json[PDFLaTeXTask.kLaTeXS3URIStdout] = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{PDFLaTeXTask.kLaTeXBucketName}/{job.TaskId}/{job.TaskId}.stdout.txt";
			json[PDFLaTeXTask.kLaTeXS3URIStderr] = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{PDFLaTeXTask.kLaTeXBucketName}/{job.TaskId}/{job.TaskId}.stderr.txt";



			if (File.Exists(filenameTEX)) {
				try {
					fileTransferUtility.Upload(filenameTEX, PDFLaTeXTask.kLaTeXBucketName, $"{job.TaskId}/{job.TaskId}.tex");
				}
				catch (Exception e) {
					Log.Debug($"Error encountered on server. Message:'{e.Message}' when writing an object");
				}
			}



			if (File.Exists(filenamePDF)) {
				try {
					fileTransferUtility.Upload(filenamePDF, PDFLaTeXTask.kLaTeXBucketName, $"{job.TaskId}/{job.TaskId}.pdf");

					json[PDFLaTeXTask.kLaTeXJsonKeyStatus] = PDFLaTeXTask.kLaTeXJsonStatusValueCompleted;
				}
				catch (AmazonS3Exception e) {
					json[PDFLaTeXTask.kLaTeXJsonKeyStatus] = PDFLaTeXTask.kLaTeXJsonStatusValueError;
					json[PDFLaTeXTask.kLaTeXJsonKeyErrorMessage] = $"Error encountered on server. Message:'{e.Message}' when writing an object";
				}
				catch (Exception e) {
					json[PDFLaTeXTask.kLaTeXJsonKeyStatus] = PDFLaTeXTask.kLaTeXJsonStatusValueError;
					json[PDFLaTeXTask.kLaTeXJsonKeyErrorMessage] = $"Unknown encountered on server. Message:'{e.Message}' when writing an object";
				}


			} else {

				json[PDFLaTeXTask.kLaTeXJsonKeyStatus] = PDFLaTeXTask.kLaTeXJsonStatusValueError;
				json[PDFLaTeXTask.kLaTeXJsonKeyErrorMessage] = "Unable to generate PDF.";
			}

			if (File.Exists(filenameAUX)) {
				try {
					fileTransferUtility.Upload(filenameAUX, PDFLaTeXTask.kLaTeXBucketName, $"{job.TaskId}/{job.TaskId}.aux");
				}
				catch (Exception e) {
					Log.Debug($"Error encountered on server. Message:'{e.Message}' when writing an object");
				}
			}

			if (File.Exists(filenameLOG)) {
				try {
					fileTransferUtility.Upload(filenameLOG, PDFLaTeXTask.kLaTeXBucketName, $"{job.TaskId}/{job.TaskId}.log");
				}
				catch (Exception e) {
					Log.Debug($"Error encountered on server. Message:'{e.Message}' when writing an object");
				}
			}

			PDFLaTeXTask.Upsert(pdfLatexDB, new Dictionary<Guid, PDFLaTeXTask> {
				{
					job.TaskId.Value, new PDFLaTeXTask(job.TaskId.Value, json.ToString(Formatting.Indented))
				}
			}, out _, out _);

			Directory.Delete(jobDir, true);




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



			pdfLatexDB.Close();

		}
	}
}
