using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Databases.Records.Billing;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Databases.Records.CRM;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;
using SharedCode.S3;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformGetVoicemailRecordingLinkParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Guid? VoicemailId { get; set; }
			public Guid? BillingCompanyId { get; set; }
		}

		public class PerformGetVoicemailRecordingLinkResponse : IdempotencyResponse
		{
			[JsonProperty(PropertyName = "voicemailURI")]
			[DataMember(Name = "voicemailURI")]
			public string? VoicemailURI { get; set; } = null;
		}

		public async Task PerformGetVoicemailRecordingLink(PerformGetVoicemailRecordingLinkParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformGetVoicemailRecordingLinkResponse response = new PerformGetVoicemailRecordingLinkResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				if (p.VoicemailId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No voicemail id provided.";
					break;
				}

				if (p.BillingCompanyId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No billing company id provided.";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;

				BillingSessions? session = null;
				BillingCompanies? billingCompany = null;

				SessionUtils.GetSessionInformation(
					this,
					response,
					p.SessionId,
					out _,
					out billingConnection,
					out session,
					out billingContact,
					out billingCompany,
					out _,
					out _,
					out dpDBConnection
					);

				if (null != response.IsError && response.IsError.Value)
					break;


				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to billing database.";
					break;
				}

				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dp database.";
					break;
				}

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				bool permAny = permissions.Contains(Databases.Konstants.kPermCRMRequestVoicemailsAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermCRMRequestVoicemailsCompany);

				if (permAny)
				{

				}
				else if (permCompany)
				{
					if (p.BillingCompanyId.Value != billingContact.CompanyId)
					{
						response.IsError = true;
						response.ErrorMessage = "No permissions. #2";
						response.IsPermissionsError = true;
						break;
					}
				}
				else
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}




				// Do action.

				var resVM = Voicemails.ForId(dpDBConnection, p.VoicemailId.Value);
				if (0 == resVM.Count)
					break;

				Voicemails vm = resVM.FirstOrDefault().Value;

				string? key = SharedCode.S3.Konstants.S3_PBX_ACCESS_KEY;
				string? secret = SharedCode.S3.Konstants.S3_PBX_SECRET_KEY;

				// Create S3 Client
				using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
				{
					RegionEndpoint = RegionEndpoint.USWest1,
					ServiceURL = SharedCode.S3.Konstants.S3_PBX_SERVICE_URI,
				});

				// Create a GetPreSignedUrlRequest request
				GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
				{
					BucketName = vm.RecordingS3Bucket,
					Key = vm.RecordingS3Key,
					Expires = DateTime.Now.AddMinutes(5),
					ResponseHeaderOverrides = new ResponseHeaderOverrides
					{
						ContentType = "audio/vnd.wav",
						ContentDisposition = $"attachment; filename={vm.MessageLeftAtISO8601}-{vm.CallerIdNumber}-{vm.CallbackNumber}.wav",
						CacheControl = "No-cache",
						Expires = "Thu, 01 Dec 1994 16:00:00 GMT",
					}
				};

				string uri = s3Client.GetPreSignedURL(request);

				// Get path for request
				response.VoicemailURI = uri;

				// We log this request to the audit timeline.
				if (null == vm.Id)
					break;
				if (null == vm.JsonObject)
					break;

				JObject? jsonMod = vm.JsonObject.DeepClone() as JObject;
				if (null == jsonMod)
					break;

				// Add Timeline Entry
				{
					JObject entry = new JObject
					{
						[Voicemails.kJsonKeyTimelineKeyType] = "text",
						[Voicemails.kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture),
						[Voicemails.kJsonKeyTimelineKeyDescription] = $"Recording accessed by {billingContact.FullName} through {(string.IsNullOrWhiteSpace(SharedCode.Konstants.APP_BASE_URI) ? Konstants.kAppBaseURINotSetErrorMessage : SharedCode.Konstants.APP_BASE_URI)} .",
						[Voicemails.kJsonKeyTimelineKeyColour] = "purple",
					};

					JArray? timeline = jsonMod[Voicemails.kJsonKeyTimeline] as JArray;
					if (null != timeline)
					{
						timeline.Add(entry);
					}

					Voicemails.Upsert(dpDBConnection, new Dictionary<Guid, Voicemails>
					{
						{ vm.Id.Value, vm with { Json = jsonMod.ToString() } },
					}, out _, out _);
				}


			}
			while (false);


			if (billingConnection != null)
			{
				billingConnection.Dispose();
				billingConnection = null;
			}

			if (dpDBConnection != null)
			{
				dpDBConnection.Dispose();
				dpDBConnection = null;
			}

			if (null == billingContact)
			{
				await Clients.Caller.SendAsync("PerformGetVoicemailRecordingLinkCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformGetVoicemailRecordingLinkCB", response).ConfigureAwait(false);
			}

			if (null != p && null != p.VoicemailId)
			{
				_ = RequestVoicemails(new RequestVoicemailsParams
				{
					SessionId = p.SessionId,
					LimitToIds = new List<Guid> { p.VoicemailId.Value },
					IdempotencyToken = Guid.NewGuid().ToString(),
					RoundTripRequestId = Guid.NewGuid().ToString(),
				});
			}
			
			









			




			
		}
	}
}
