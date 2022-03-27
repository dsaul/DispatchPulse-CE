using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Databases.Records.CRM;
using Databases.Records.Billing;
using SharedCode.Databases.Records.CRM;
using Amazon.S3;
using SharedCode.S3;
using Amazon;
using Amazon.S3.Transfer;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class DeleteRecordingsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> RecordingsDelete { get; set; } = new List<Guid>();
		}
		public class DeleteRecordingsResponse : IdempotencyResponse
		{
			public List<Guid> RecordingsDelete { get; set; } = new List<Guid>();
		}
		public async Task DeleteRecordings(DeleteRecordingsParams p)
		{
			DeleteRecordingsResponse response = new DeleteRecordingsResponse()
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
				if (p.RecordingsDelete == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.RecordingsDelete == null";
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

				if (p.RecordingsDelete.Count == 0)
					break;

				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to billing database.";
					break;
				}

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}


				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(Databases.Konstants.kPermCRMDeleteRecordingsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMDeleteRecordingsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// Delete the files on S3 first.

				var resRec = Recordings.ForIds(dpDBConnection, p.RecordingsDelete);

				string? key = SharedCode.S3.Konstants.S3_PBX_ACCESS_KEY;
				string? secret = SharedCode.S3.Konstants.S3_PBX_SECRET_KEY;

				foreach (KeyValuePair<Guid, Recordings> kvp in resRec)
				{
					string? s3Host = kvp.Value.S3Host;
					string? bucket = kvp.Value.S3Bucket;
					string? s3MP3Key = kvp.Value.S3MP3Key;
					string? s3WAVKey = kvp.Value.S3WAVKey;
					string? s3PCMKey = kvp.Value.S3PCMKey;



					using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
					{
						RegionEndpoint = RegionEndpoint.USWest1,
						ServiceURL = s3Host,
						ForcePathStyle = true
					});

					

					await s3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest() { 
						BucketName = bucket, 
						Key = s3MP3Key
					});
					await s3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest()
					{
						BucketName = bucket,
						Key = s3WAVKey
					});
					await s3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest()
					{
						BucketName = bucket,
						Key = s3PCMKey
					});
				}












				List<Guid> affected = Recordings.Delete(dpDBConnection, p.RecordingsDelete);
				if (affected.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "No rows deleted?";
					break;
				}

				response.RecordingsDelete = affected;
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

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("DeleteRecordingsCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("DeleteRecordingsCB", response).ConfigureAwait(false);
			}




		}
	}
}