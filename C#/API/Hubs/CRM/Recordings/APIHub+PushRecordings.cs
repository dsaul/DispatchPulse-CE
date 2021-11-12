using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Databases.Records.CRM;
using Databases.Records.Billing;
using API.Utility;
using SharedCode.Extensions;
using SharedCode.Databases.Records.CRM;
using Newtonsoft.Json.Linq;
using Amazon.Runtime;
using Amazon.S3;
using Amazon;
using SharedCode.S3;
using Amazon.S3.Transfer;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushRecordingsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, Recordings> Recordings { get; set; } = new Dictionary<Guid, Recordings>();
		}
		public class PushRecordingsResponse : IdempotencyResponse
		{
			public List<Guid> Recordings { get; } = new List<Guid>();
		}

		public async Task PushRecordings(PushRecordingsParams p)
		{
			PushRecordingsResponse response = new PushRecordingsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestRecordingsResponse othersMsg = new RequestRecordingsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Recordings> toSendToOthers = new Dictionary<Guid, Recordings>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Recordings == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Recordings == null";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;
				othersMsg.RoundTripRequestId = p.RoundTripRequestId;

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

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}

				if (null == billingCompany)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing company.";
					break;
				}


				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

				if (string.IsNullOrWhiteSpace(billingCompany.S3BucketName))
				{
					response.IsError = true;
					response.ErrorMessage = "No S3 bucket created for this company.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushRecordingsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushRecordingsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// Go through all of these recordings we're uploading, send the sound files to S3, then delete the temporary files.
				string? s3Host = SharedCode.S3.Konstants.S3_PBX_SERVICE_URI;
				if (string.IsNullOrWhiteSpace(s3Host))
                {
					response.IsError = true;
					response.ErrorMessage = "S3 Configuration Error";
					response.IsPermissionsError = true;
					break;
				}

				string bucket = billingCompany.S3BucketName;

				foreach (KeyValuePair<Guid, Recordings> kvp in p.Recordings)
				{
					JObject json = null == kvp.Value.JsonObject ? new JObject() : kvp.Value.JsonObject;

					

					// if there is a new recording info...
					do
					{
						// Validate paths.
						if (string.IsNullOrWhiteSpace(kvp.Value.TmpMP3Path) ||
							string.IsNullOrWhiteSpace(kvp.Value.TmpWAVPath) ||
							string.IsNullOrWhiteSpace(kvp.Value.TmpPCMPath)
							)
						{
							break;
						}

						// Make sure that the path provided by the user has the same parent directory as we'd generate.
						string unusedTmpFilePath = System.IO.Path.GetTempFileName();
						string? unusedTmpFileParent = System.IO.Path.GetDirectoryName(unusedTmpFilePath);
						System.IO.File.Delete(unusedTmpFilePath);


						string? mp3Parent = System.IO.Path.GetDirectoryName(kvp.Value.TmpMP3Path);
						string? wavParent = System.IO.Path.GetDirectoryName(kvp.Value.TmpWAVPath);
						string? pcmParent = System.IO.Path.GetDirectoryName(kvp.Value.TmpPCMPath);

						if (
							unusedTmpFileParent != mp3Parent ||
							unusedTmpFileParent != wavParent ||
							unusedTmpFileParent != pcmParent
							)
						{
							break;
						}

						json[Recordings.kJsonKeyS3Host] = s3Host;
						json[Recordings.kJsonKeyS3Bucket] = bucket;



						// mp3
						{
							string s3Key = $"Recordings/{kvp.Value.Id}.mp3";
							string recordingS3HttpsURI = $"{s3Host}/{bucket}/{s3Key}";
							string s3CmdURI = $"s3://{bucket}/{s3Key}";

							string? key = SharedCode.S3.Konstants.S3_PBX_ACCESS_KEY;
							string? secret = SharedCode.S3.Konstants.S3_PBX_SECRET_KEY;

							using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
							{
								RegionEndpoint = RegionEndpoint.USWest1,
								ServiceURL = SharedCode.S3.Konstants.S3_PBX_SERVICE_URI,
							});

							var uploadRequest = new TransferUtilityUploadRequest
							{
								InputStream = System.IO.File.OpenRead(kvp.Value.TmpMP3Path),
								Key = s3Key,
								BucketName = bucket,
							};

							var fileTransferUtility = new TransferUtility(s3Client);
							await fileTransferUtility.UploadAsync(uploadRequest);

							json[Recordings.kJsonKeyS3MP3Key] = s3Key;
							json[Recordings.kJsonKeyS3MP3HTTPSURI] = recordingS3HttpsURI;
							json[Recordings.kJsonKeyS3CMDMP3URI] = s3CmdURI;

							System.IO.File.Delete(kvp.Value.TmpMP3Path);
							json[Recordings.kJsonKeyTmpMP3Path] = null;
						}

						// wav
						{
							string s3Key = $"Recordings/{kvp.Value.Id}.wav";
							string recordingS3HttpsURI = $"https://{s3Host}/{bucket}/{s3Key}";
							string s3CmdURI = $"s3://{bucket}/{s3Key}";

							string? key = SharedCode.S3.Konstants.S3_PBX_ACCESS_KEY;
							string? secret = SharedCode.S3.Konstants.S3_PBX_SECRET_KEY;

							using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
							{
								RegionEndpoint = RegionEndpoint.USWest1,
								ServiceURL = SharedCode.S3.Konstants.S3_PBX_SERVICE_URI,
							});

							var uploadRequest = new TransferUtilityUploadRequest
							{
								InputStream = System.IO.File.OpenRead(kvp.Value.TmpWAVPath),
								Key = s3Key,
								BucketName = bucket,
							};

							var fileTransferUtility = new TransferUtility(s3Client);
							await fileTransferUtility.UploadAsync(uploadRequest);

							json[Recordings.kJsonKeyS3WAVKey] = s3Key;
							json[Recordings.kJsonKeyS3WAVHTTPSURI] = recordingS3HttpsURI;
							json[Recordings.kJsonKeyS3CMDWAVURI] = s3CmdURI;

							System.IO.File.Delete(kvp.Value.TmpWAVPath);
							json[Recordings.kJsonKeyTmpWAVPath] = null;
						}


						// pcm
						{
							string s3Key = $"Recordings/{kvp.Value.Id}.pcm";
							string recordingS3HttpsURI = $"https://{s3Host}/{bucket}/{s3Key}";
							string s3CmdURI = $"s3://{bucket}/{s3Key}";

							string? key = SharedCode.S3.Konstants.S3_PBX_ACCESS_KEY;
							string? secret = SharedCode.S3.Konstants.S3_PBX_SECRET_KEY;

							using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
							{
								RegionEndpoint = RegionEndpoint.USWest1,
								ServiceURL = SharedCode.S3.Konstants.S3_PBX_SERVICE_URI,
							});

							var uploadRequest = new TransferUtilityUploadRequest
							{
								InputStream = System.IO.File.OpenRead(kvp.Value.TmpPCMPath),
								Key = s3Key,
								BucketName = bucket,
							};

							var fileTransferUtility = new TransferUtility(s3Client);
							await fileTransferUtility.UploadAsync(uploadRequest);

							json[Recordings.kJsonKeyS3PCMKey] = s3Key;
							json[Recordings.kJsonKeyS3PCMHTTPSURI] = recordingS3HttpsURI;
							json[Recordings.kJsonKeyS3CMDPCMURI] = s3CmdURI;

							System.IO.File.Delete(kvp.Value.TmpPCMPath);
							json[Recordings.kJsonKeyTmpPCMPath] = null;
						}
					}
					while (false);




					p.Recordings[kvp.Key] = kvp.Value with
					{
						Json = json.ToString(Newtonsoft.Json.Formatting.Indented),
					};

				}




				Recordings.Upsert(
					dpDBConnection,
					p.Recordings,
					out callerResponse,
					out toSendToOthers
					);


				response.Recordings.AddRange(callerResponse);
				othersMsg.Recordings.AddRange(toSendToOthers);

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


			await Clients.Caller.SendAsync("PushRecordingsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestRecordingsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestRecordingsCB", othersMsg).ConfigureAwait(false);
			}








		}
	}
}