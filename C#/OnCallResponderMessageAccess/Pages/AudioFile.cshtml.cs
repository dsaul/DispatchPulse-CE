using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SharedCode.DatabaseSchemas;
using SharedCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace OnCallResponderMessageAccess.Pages
{
	public class AudioFileModel : PageModel
	{

		[BindProperty(SupportsGet = true)]
		public Guid? BillingCompanyId { get; set; }
		[BindProperty(SupportsGet = true)]
		public Guid? VoicemailId { get; set; }
		[BindProperty(SupportsGet = true)]
		public string? AdditionalDataBase64 { get; set; }
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public NpgsqlConnection? DPDB { get; set; } = null;
		public BillingCompanies? BillingCompany { get; set; } = null;
		public BillingSubscriptions? BillingSubscription { get; set; } = null;
		public List<Guid> DPPackageKeys { get; set; } = new List<Guid>();
		public string? DPDatabaseName { get; set; } = null;
		public Voicemails? Voicemail { get; set; } = null;
		public HttpMethod? Method { get; set; }
		public string? UserDescription { get; set; } = null;

		public IActionResult OnGet()
		{
			Method = HttpMethod.Get;

			if (!SharedSetup())
				return Page();
			if (null == Voicemail)
				return Page();
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_ACCESS_KEY))
				return Page();
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_SECRET_KEY))
				return Page();
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_SERVICE_URI))
				return Page();

			string? key = EnvAmazonS3.S3_PBX_ACCESS_KEY;
			string? secret = EnvAmazonS3.S3_PBX_SECRET_KEY;

			// Create S3 Client
			using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
				{
				RegionEndpoint = RegionEndpoint.USWest1,
				ServiceURL = EnvAmazonS3.S3_PBX_SERVICE_URI,
				ForcePathStyle = true
			});

			// Create a GetPreSignedUrlRequest request
			GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
				{
				BucketName = Voicemail.RecordingS3Bucket,
				Key = Voicemail.RecordingS3Key,
				Expires = DateTime.Now.AddMinutes(5),
				ResponseHeaderOverrides = new ResponseHeaderOverrides
					{
					ContentType = "audio/vnd.wav",
					ContentDisposition = $"attachment; filename={Voicemail.MessageLeftAtISO8601}-{Voicemail.CallerIdNumber}-{Voicemail.CallbackNumber}.wav",
					CacheControl = "No-cache",
					Expires = "Thu, 01 Dec 1994 16:00:00 GMT",
				}
			};

			string uri = s3Client.GetPreSignedURL(request);


			do {
				if (null == DPDB)
					break;
				if (null == Voicemail)
					break;
				if (null == Voicemail.JsonObject)
					break;
				if (null == VoicemailId)
					break;

				JObject? json = Voicemail.JsonObject.DeepClone() as JObject;
				if (null == json)
					break;

				// Add audit entry.
				JObject entry = new JObject {
					[Voicemails.kJsonKeyTimelineKeyType] = "text",
					[Voicemails.kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
					[Voicemails.kJsonKeyTimelineKeyDescription] = $"\"{UserDescription}\" accessed the recording through responder message access.",
					[Voicemails.kJsonKeyTimelineKeyColour] = "green",
				};

				JArray? timeline = json[Voicemails.kJsonKeyTimeline] as JArray;
				if (null != timeline) {
					timeline.Add(entry);
				}

				Voicemail = Voicemail with
				{
					Json = json.ToString(Formatting.Indented)
				};

				Voicemails.Upsert(DPDB, new Dictionary<Guid, Voicemails> {
								{ VoicemailId.Value, Voicemail }
							}, out _, out _);


			} while (false);


			return Redirect(uri);


		}

		private bool SharedSetup() {
			if (null == BillingCompanyId)
				return false;

			if (null == VoicemailId)
				return false;

			// Parse additional data.
			if (string.IsNullOrWhiteSpace(AdditionalDataBase64))
				return false;
			byte[] additionalDataBytes = Convert.FromBase64String(AdditionalDataBase64);
			string? additionalDataStr = Encoding.UTF8.GetString(additionalDataBytes);
			if (string.IsNullOrWhiteSpace(additionalDataStr))
				return false;
			JObject? additionalData = JObject.Parse(additionalDataStr);
			if (null == additionalData)
				return false;
			UserDescription = additionalData.Value<string?>("description");
			if (string.IsNullOrWhiteSpace(UserDescription))
				return false;





			BillingDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(EnvDatabases.BILLING_DATABASE_NAME));
			if (null == BillingDB)
				return false;
			BillingDB.Open();

			var resBC = BillingCompanies.ForIds(BillingDB, BillingCompanyId.Value);
			if (0 == resBC.Count)
				return false;

			BillingCompany = resBC.FirstOrDefault().Value;
			if (null == BillingCompany)
				return false;

			var resPackages = BillingPackages.ForProvisionOnCallAutoAttendants(BillingDB, true);
			if (0 == resPackages.Count)
				return false;

			DPPackageKeys.AddRange(resPackages.Keys);

			var resSubs = BillingSubscriptions.ForCompanyIdPackageIdsAndHasDatabase(BillingDB, BillingCompanyId.Value, DPPackageKeys);
			if (0 == resSubs.Count)
				return false;

			BillingSubscription = resSubs.FirstOrDefault().Value;
			if (null == BillingSubscription)
				return false;

			if (string.IsNullOrWhiteSpace(BillingSubscription.ProvisionedDatabaseName))
				return false;

			DPDatabaseName = BillingSubscription.ProvisionedDatabaseName;
			if (string.IsNullOrWhiteSpace(DPDatabaseName))
				return false;

			DPDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(DPDatabaseName));
			if (null == DPDB)
				return false;
			DPDB.Open();

			var resVM = Voicemails.ForId(DPDB, VoicemailId.Value);
			if (0 == resVM.Count)
				return false;

			Voicemail = resVM.FirstOrDefault().Value;
			if (null == Voicemail)
				return false;

			return true;
		}
	}
}
