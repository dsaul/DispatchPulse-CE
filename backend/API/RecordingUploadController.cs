using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Id3;
using API.Hubs;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Npgsql;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API
{
	[Route("api/[controller]")]
	[ApiController]
	public class RecordingUploadController : ControllerBase
	{
		public record ProcessedFile(string mp3Path, string wavPath, string pcmPath);

		[HttpPost]
		public async Task<IActionResult> Post([FromForm] Guid sessionId, List<IFormFile> recordingFiles)
		{
			
			bool isError = false;
			string errorMessage = "";
			List<ProcessedFile> processedFiles = new List<ProcessedFile>();

			do
			{

				// Verify session has permissions to upload.

				NpgsqlConnection? billingConnection = null;
				BillingContacts? billingContact = null;
				BillingSessions? session = null;
				PermissionsIdempotencyResponse response = new PermissionsIdempotencyResponse();
				SessionUtils.GetSessionInformation(
						null,
						response,
						sessionId,
						out _,
						out billingConnection,
						out session,
						out billingContact,
						out _,
						out _,
						out _,
						out _
						);

				if (null == billingConnection)
				{
					isError = true;
					errorMessage = "Can't connect to database.";
					break;
				}
				
				if (null == billingContact)
				{
					isError = true;
					errorMessage = "Can't find billing contact.";
					break;
				}


				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(EnvDatabases.kPermCRMPushRecordingsAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMPushRecordingsCompany)
					)
				{
					isError = true;
					errorMessage = "No permissions.";
					break;
				}



				// Do Upload


				long sizeBytes = recordingFiles.Sum(f => f.Length);
				float sizeMB = (sizeBytes / 1024f) / 1024f;

				if (sizeMB > 5)
				{
					isError = true;
					errorMessage = "File must be less then 5 MB in size.";
					break;
				}

				foreach (var formFile in recordingFiles)
				{
					if (formFile.Length > 0)
					{
						ProcessedFile? processedFile = await DoProcessFile(formFile);
						if (processedFile == null)
							continue;

						processedFiles.Add(processedFile);

					}
				}

				if (processedFiles.Count == 0)
				{
					isError = true;
					errorMessage = "No MP3s were uploaded.";
					break;
				}


			}
			while (false);

			

			

			// Process uploaded files
			// Don't rely on or trust the FileName property without validation.

			return Ok(new {
				IsError = isError,
				ErrorMessage = errorMessage,
				ProcessedFiles = processedFiles,
			});
		}



		private async Task<ProcessedFile?> DoProcessFile(IFormFile formFile)
		{
			if (string.IsNullOrWhiteSpace(EnvTTS.FFMPEG_PATH))
				throw new Exception("string.IsNullOrWhiteSpace(SharedCode.TTS.Konstants.FFMPEG_PATH)");

			var filenameBase = Path.GetTempFileName();
			var filenameOrig = System.IO.Path.ChangeExtension(filenameBase, "orig");
			var filenameMP3 = System.IO.Path.ChangeExtension(filenameBase, "mp3");
			var filenameWAV = System.IO.Path.ChangeExtension(filenameBase, "wav");
			var filenamePCM = System.IO.Path.ChangeExtension(filenameBase, "pcm");

			using var stream = System.IO.File.Create(filenameOrig);
			await formFile.CopyToAsync(stream);
			stream.Close();

			// Confirm we were uploaded an MP3 file.
			try
			{
				using var mp3 = new Id3.Mp3(filenameOrig);
				_ = mp3.Audio;

			} catch (Id3.Id3Exception)
			{
				return null;
			}

			System.IO.File.Copy(filenameOrig, filenameMP3);

			// ffmpeg
			// .\ffmpeg.exe -i cf4f2f2e-dde7-4883-8e56-81b29a780b07-this-is-a-test.mp3 -ar 8000 -ac 1 -ab 64 cf4f2f2e-dde7-4883-8e56-81b29a780b07-this-is-a-test.wav -ar 8000 -ac 1 -ab 64 -f mulaw cf4f2f2e-dde7-4883-8e56-81b29a780b07-this-is-a-test.pcm -map 0:0 -map 0:0

			string strCmdText = $"-loglevel panic -hide_banner -nostats -i {filenameMP3} -ar 8000 -ac 1 -ab 64k {filenameWAV} -ar 8000 -ac 1 -ab 64k -f mulaw {filenamePCM} -map 0:0 -map 0:0";

			Log.Debug("[EnsureDatabaseEntry()] FFMPEG PATH = {FFMpegPath}", EnvTTS.FFMPEG_PATH);
			Log.Debug("[EnsureDatabaseEntry()] FFMPEG ARGUMENTS = {FFMpegArgs}", strCmdText);

			var process = System.Diagnostics.Process.Start(EnvTTS.FFMPEG_PATH, strCmdText);
			if (false == process.WaitForExit(5000)) {
				return null;
			}





			return new ProcessedFile(
				mp3Path: filenameMP3,
				wavPath: filenameWAV,
				pcmPath:filenamePCM
				);

		}

















	}
}
