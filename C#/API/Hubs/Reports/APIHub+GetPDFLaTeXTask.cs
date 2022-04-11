using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.IO;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using System.Web;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class GetPDFLaTeXTaskParams : IdempotencyRequest
		{
			public Guid? TaskId { get; set; }
		}

		public class GetPDFLaTeXTaskResponse : PermissionsIdempotencyResponse
		{
			public bool? IsCompleted { get; set; }
			public string? Status { get; set; }
			public string? TempLink { get; set; }

		}


		public async Task GetPDFLaTeXTask(GetPDFLaTeXTaskParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			GetPDFLaTeXTaskResponse response = new GetPDFLaTeXTaskResponse();


			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			BillingContacts? billingContact = null;
			string? dpDBName = null;

			do
			{
				string? s3PDFLaTeXAccessKeyFile = System.Environment.GetEnvironmentVariable("S3_PDFLATEX_ACCESS_KEY_FILE");
				if (string.IsNullOrWhiteSpace(s3PDFLaTeXAccessKeyFile))
				{
					response.IsError = true;
					response.ErrorMessage = "Can't access S3 #1.";
					break;
				}
				string? s3PDFLaTeXAccessKey = File.ReadAllText(s3PDFLaTeXAccessKeyFile);
				if (string.IsNullOrWhiteSpace(s3PDFLaTeXAccessKey))
				{
					response.IsError = true;
					response.ErrorMessage = "Can't access S3 #2.";
					break;
				}

				string? s3PDFLaTeXSecretKeyFile = System.Environment.GetEnvironmentVariable("S3_PDFLATEX_SECRET_KEY_FILE");
				if (string.IsNullOrWhiteSpace(s3PDFLaTeXSecretKeyFile))
				{
					response.IsError = true;
					response.ErrorMessage = "Can't access S3 #3.";
					break;
				}
				string s3PDFLaTeXSecretKey = File.ReadAllText(s3PDFLaTeXSecretKeyFile);
				if (string.IsNullOrWhiteSpace(File.ReadAllText(s3PDFLaTeXSecretKeyFile)))
				{
					response.IsError = true;
					response.ErrorMessage = "Can't access S3 #4.";
					break;
				}




				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				if (p.TaskId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No task id provided.";
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
					out dpDBName,
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


				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

				// Get the requested task from the database.
				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);


				// Make sure that the user has any report permissions.
				bool canReportContacts = permissions.Contains(EnvDatabases.kPermCRMReportContactsPDF);
				bool canReportCompanies = permissions.Contains(EnvDatabases.kPermCRMReportCompaniesPDF);
				bool canReportProjects = permissions.Contains(EnvDatabases.kPermCRMReportProjectsPDF);
				bool canReportAssignments = permissions.Contains(EnvDatabases.kPermCRMReportAssignmentsPDF);
				bool canReportMaterials = permissions.Contains(EnvDatabases.kPermCRMReportMaterialsPDF);
				bool canReportLabour = permissions.Contains(EnvDatabases.kPermCRMReportLabourPDF);
				bool canReportOnCallResponder30Day = permissions.Contains(EnvDatabases.kPermCRMReportOnCallResponder30DayPDF);

				if (
					!canReportContacts && 
					!canReportCompanies &&
					!canReportProjects &&
					!canReportAssignments &&
					!canReportMaterials &&
					!canReportLabour &&
					!canReportOnCallResponder30Day
					)
				{
					response.IsError = true;
					response.ErrorMessage = "You don't have permissions to access these reports.";
					response.IsPermissionsError = true;
					break;
				}




				using NpgsqlConnection pdfLatexDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(PDFLaTeXTask.kPDFLaTeXDBName));
				pdfLatexDB.Open();

				var resTask = PDFLaTeXTask.ForId(pdfLatexDB, p.TaskId.Value);
				if (resTask.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Can't find the task.";
					break;
				}

				PDFLaTeXTask task = resTask.FirstOrDefault().Value;

				string? reportType = task.ReportType;

				switch (reportType)
				{
					case PDFLaTeXTask.kLaTeXJsonReportTypeValueAssignments:

						if (!canReportAssignments)
						{
							response.IsError = true;
							response.ErrorMessage = "You don't have permissions to access these reports.";
							response.IsPermissionsError = true;
							break;
						}

						break;
					case PDFLaTeXTask.kLaTeXJsonReportTypeValueCompanies:
						if (!canReportCompanies)
						{
							response.IsError = true;
							response.ErrorMessage = "You don't have permissions to access these reports.";
							response.IsPermissionsError = true;
							break;
						}
						break;
					case PDFLaTeXTask.kLaTeXJsonReportTypeValueContacts:
						if (!canReportContacts)
						{
							response.IsError = true;
							response.ErrorMessage = "You don't have permissions to access these reports.";
							response.IsPermissionsError = true;
							break;
						}
						break;
					case PDFLaTeXTask.kLaTeXJsonReportTypeValueLabour:
						if (!canReportLabour)
						{
							response.IsError = true;
							response.ErrorMessage = "You don't have permissions to access these reports.";
							response.IsPermissionsError = true;
							break;
						}
						break;
					case PDFLaTeXTask.kLaTeXJsonReportTypeValueMaterials:
						if (!canReportMaterials)
						{
							response.IsError = true;
							response.ErrorMessage = "You don't have permissions to access these reports.";
							response.IsPermissionsError = true;
							break;
						}
						break;
					case PDFLaTeXTask.kLaTeXJsonReportTypeValueProjects:
						if (!canReportProjects)
						{
							response.IsError = true;
							response.ErrorMessage = "You don't have permissions to access these reports.";
							response.IsPermissionsError = true;
							break;
						}
						break;
					case PDFLaTeXTask.kLaTeXJsonReportTypeValueOnCallResponder30Days:
						if (!canReportOnCallResponder30Day)
						{
							response.IsError = true;
							response.ErrorMessage = "You don't have permissions to access these reports.";
							response.IsPermissionsError = true;
							break;
						}
						break;
					default:
						response.IsError = true;
						response.ErrorMessage = "Unknown report type.";
						break;
				}
				if (null != response.IsError && response.IsError.Value)
					break;

				// If it isn't completed, exit early.
				response.Status = task.Status;
				response.IsCompleted = task.Status == PDFLaTeXTask.kLaTeXJsonStatusValueCompleted;
				if (!response.IsCompleted.Value)
					break;

				// At this point it should be completed.

				if (string.IsNullOrWhiteSpace(task.S3URIPdf))
				{
					response.IsError = true;
					response.ErrorMessage = "Can't find report file link on server.";
					break;
				}

				Uri pdfUri = new Uri(task.S3URIPdf);
				string path = pdfUri.LocalPath;
				List<string> pathComponents = path.Split('/').ToList();

				// [0] should be blank.
				if (!string.IsNullOrWhiteSpace(pathComponents[0]))
				{
					response.IsError = true;
					response.ErrorMessage = "System error: !string.IsNullOrWhiteSpace(pathComponents[0])";
					break;
				}

				// [1] should be the bucket name
				if (string.IsNullOrWhiteSpace(pathComponents[1]))
				{
					response.IsError = true;
					response.ErrorMessage = "System error: string.IsNullOrWhiteSpace(pathComponents[1])";
					break;
				}

				string bucketName = pathComponents[1];

				// Remove non file components.
				pathComponents.RemoveRange(0, 2);

				if (pathComponents.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "System error: pathComponents.Count == 0";
					break;
				}


				string s3key = string.Join('/', pathComponents);
				if (string.IsNullOrWhiteSpace(s3key))
				{
					response.IsError = true;
					response.ErrorMessage = "System error: string.IsNullOrWhiteSpace(s3key)";
					break;
				}

				// Create S3 Client
				using var s3Client = new AmazonS3Client(s3PDFLaTeXAccessKey, s3PDFLaTeXSecretKey, new AmazonS3Config
				{
					RegionEndpoint = RegionEndpoint.USWest1,
					ServiceURL = EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI,
					ForcePathStyle = true
				});


				// Create a GetPreSignedUrlRequest request
				GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
				{
					BucketName = bucketName,
					Key = s3key,
					Expires = DateTime.Now.AddMinutes(5),
					ResponseHeaderOverrides = new ResponseHeaderOverrides
					{
						ContentType = "application/pdf",
						ContentDisposition = $"attachment; filename={task.ReportType} Report {p.TaskId}.pdf",
						CacheControl = "No-cache",
						Expires = "Thu, 01 Dec 1994 16:00:00 GMT",
					}
				};

				

				// Get path for request
				response.TempLink = s3Client.GetPreSignedURL(request);

			} while (false);

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
				await Clients.Caller.SendAsync("GetPDFLaTeXTaskCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("GetPDFLaTeXTaskCB", response).ConfigureAwait(false);
			}


			

		}












	}

}
