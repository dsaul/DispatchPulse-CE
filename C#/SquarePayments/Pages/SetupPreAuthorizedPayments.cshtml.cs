using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Hubs;
using API.Utility;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using Square;
using Square.Exceptions;
using Square.Models;
using System.Text;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using System.IO;
using Amazon.S3;
using Amazon;
using Amazon.S3.Transfer;
using HeyRed.Mime;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

namespace SquarePayments.Pages
{
	[IgnoreAntiforgeryToken(Order = 2000)]
	public class SetupPreAuthorizedPaymentsModel : PageModel
    {
		[BindProperty(SupportsGet = true)]
		public Guid? SessionId { get; set; }
		[BindProperty(SupportsGet = true)]
		public string? Nonce { get; set; }


		public NpgsqlConnection? BillingDB { get; set; } = null;
		public NpgsqlConnection? DPDB { get; set; } = null;
		public BillingSessions? BillingSession { get; set; } = null;
		public BillingContacts? BillingContact { get; set; } = null;

		public BillingCompanies? BillingCompany { get; set; } = null;
		public BillingSubscriptions? BillingSubscription { get; set; } = null;
		public HashSet<string> Permissions { get; } = new HashSet<string>();
		public bool CanAccessPreAuthorizedPayments { get; set; } = false;
		public string? ErrorMessage { get; set; } = null;

		static SquareClient SquareClient { get; set; } = new SquareClient.Builder()
				//.Environment(Square.Environment.Sandbox)
				//.AccessToken(SharedCode.Square.Konstants.SQUARE_SANDBOX_ACCESS_TOKEN)
				.Environment(Square.Environment.Production)
				.AccessToken(SharedCode.Square.Konstants.SQUARE_PRODUCTION_ACCESS_TOKEN)
				.Build();

		private bool SharedSetup() {

			if (null == SessionId) {
				return false;
			}

			IdempotencyResponse response = new IdempotencyResponse();

			SessionUtils.GetSessionInformation(
					null,
					response,
					SessionId.Value,
					out _,
					out var tmpBillingDB,
					out var tmpSession,
					out var tmpContact,
					out var tmpCompany,
					out _,
					out _,
					out var tmpDPDB
					);
			if (null != response.IsError && response.IsError.Value)
				return false;

			BillingDB = tmpBillingDB;
			BillingSession = tmpSession;
			BillingContact = tmpContact;
			BillingCompany = tmpCompany;
			DPDB = tmpDPDB;

			if (null == BillingDB)
				return false;
			if (null == BillingSession)
				return false;
			if (null == BillingContact)
				return false;
			if (null == BillingCompany)
				return false;
			if (null == DPDB)
				return false;

			Permissions.UnionWith(BillingPermissionsBool.GrantedForBillingContact(BillingDB, BillingContact));

			CanAccessPreAuthorizedPayments = Permissions.Contains(Databases.Konstants.kPermBillingCanSetupPreAuthorizedCreditCardPayments);

			if (!CanAccessPreAuthorizedPayments) {
				return false;
			}


			return true;
		}


		public IActionResult OnGet() {
			if (!SharedSetup())
				return Page();

			return Page();
		}
		public IActionResult OnPost(List<IFormFile> authorizationFormFile) {

			if (!SharedSetup())
				return Page();
			if (null == Program.SignalRConnection) {
				ErrorMessage = "Not connected to API #1.";
				return Page();
			}
			if (Program.SignalRConnection.State != HubConnectionState.Connected) {
				ErrorMessage = "Not connected to API #2.";
				return Page();
			}

			if (authorizationFormFile.Count == 0) {
				ErrorMessage = "We're missing the pre-authorized credit card payment form.";
				return Page();
			}
			if (null == Program.SignalRConnection) {
				ErrorMessage = "Not connected to API #1.";
				return Page();
			}
			if (Program.SignalRConnection.State != HubConnectionState.Connected) {
				ErrorMessage = "Not connected to API #2.";
				return Page();
			}


			IFormFile file = authorizationFormFile.First();

			var customersApi = SquareClient.CustomersApi;

			try {
				if (null == BillingCompany)
					throw new Exception("Can't find company information.");


				BillingCompany = BillingCompanies.EnsureCompanyHasSquareAccount(SquareClient, BillingCompany).Result;

				if (null == BillingCompany) {
					throw new Exception("No company information.");
				}

				if (null == BillingCompany.JsonObject) {
					throw new Exception("Can't get additional company information.");
				}

				if (null == BillingCompany.Uuid) {
					throw new Exception("Can't get billing company id.");
				}

				if (null == BillingDB) {
					throw new Exception("Can't access billing database.");
				}

				string? squareCustomerId = BillingCompany.SquareCustomerId;
				if (string.IsNullOrWhiteSpace(squareCustomerId)) {
					ErrorMessage = "Can't find square customer id.";
					return Page();
				}

				Log.Information("Begin creating stripe credit card.");
				CreateCustomerCardResponse resCreateCC = customersApi.CreateCustomerCard(
					squareCustomerId,
					new CreateCustomerCardRequest(
						cardNonce: Nonce
					)
				);
				Log.Information("Done creating stripe credit card.");

				Log.Information("Creating email message.");
				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"Pre-authorized payments were just setup on {BillingCompany.FullName} {BillingCompany.Uuid}.");

				Log.Information("Begin write uploaded file authorization file to temp file.");
				string tmpFile = Path.GetTempFileName();
				{
					using var writeTmpStream = System.IO.File.Open(tmpFile, FileMode.Create);
					file.CopyTo(writeTmpStream);
				}
				Log.Information("Done write uploaded file authorization file to temp file.");

				DateTime dt = DateTime.UtcNow;
				Regex reg = new Regex("[^a-zA-Z0-9']");
				string? authorizationS3Key = $"{BillingCompany.Uuid}-{reg.Replace(dt.ToString("o"), "-")}.{MimeTypesMap.GetExtension(file.ContentType)}";
				Task s3Task = Task.Run(delegate {
					Log.Information("Begin send authorization file to S3.");
					
					{
						var config = new AmazonS3Config
						{
							RegionEndpoint = RegionEndpoint.USWest1,
							ServiceURL = SharedCode.S3.Konstants.S3_DISPATCH_PULSE_SERVICE_URI,
							ForcePathStyle = true
						};
						var s3Client = new AmazonS3Client(
						SharedCode.S3.Konstants.S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY,
						SharedCode.S3.Konstants.S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY,
						config);
						var fileTransferUtility = new TransferUtility(s3Client);

						using var stream = System.IO.File.OpenRead(tmpFile);

						fileTransferUtility.Upload(stream, SharedCode.S3.Konstants.S3_CARD_ON_FILE_BUCKET_NAME, authorizationS3Key);
					}
					Log.Information("Done send authorization file to S3.");
				});

				Task emailTask = Task.Run(delegate {
					Log.Information("Begin send e-mail.");

					using var stream = System.IO.File.OpenRead(tmpFile);



					// Send to accounts receivable to be verified.
					Email
						.From(SharedCode.OnCallResponder.Konstants.ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS, "Square Pre-Authorized")
						.To(SharedCode.Konstants.ACCOUNTS_RECEIVABLE_EMAIL)
						.Subject("Square Pre-Authorized")
						.Body(sb.ToString())
						.Attach(new Attachment {
							Data = stream,
							Filename = file.FileName,
							ContentType = file.ContentType
						})
						.Send();

					Log.Information("Done send e-mail.");
				});

				Task.WhenAll(s3Task, emailTask).ContinueWith(delegate {
					Log.Information("Begin delete temp file.");
					System.IO.File.Delete(tmpFile);
					Log.Information("Done delete temp file.");
				});

				Log.Information("Begin update postgres.");
				JObject? modJson = BillingCompany.JsonObject.DeepClone() as JObject;
				if (null == modJson) {
					throw new Exception("Can't get additional company information. #2");
				}
				modJson[BillingCompanies.kJsonKeySquareCardBrand] = resCreateCC.Card.CardBrand;
				modJson[BillingCompanies.kJsonKeySquareCardExpMonth] = resCreateCC.Card.ExpMonth;
				modJson[BillingCompanies.kJsonKeySquareCardExpYear] = resCreateCC.Card.ExpYear;
				modJson[BillingCompanies.kJsonKeySquareCardLast4] = resCreateCC.Card.Last4;
				modJson[BillingCompanies.kJsonKeySquareCardId] = resCreateCC.Card.Id;
				modJson[BillingCompanies.kJsonKeySquareCardAuthorizationS3ServiceURL] = SharedCode.S3.Konstants.S3_DISPATCH_PULSE_SERVICE_URI;
				modJson[BillingCompanies.kJsonKeySquareCardAuthorizationS3BucketName] = SharedCode.S3.Konstants.S3_CARD_ON_FILE_BUCKET_NAME;
				modJson[BillingCompanies.kJsonKeySquareCardAuthorizationS3Key] = authorizationS3Key;

				BillingCompany = BillingCompany with
				{
					PaymentMethod = BillingPaymentMethod.kValueSquarePreAuthorized,
					Json = modJson.ToString(Newtonsoft.Json.Formatting.Indented)
				};

				BillingCompanies.Upsert(BillingDB, new Dictionary<Guid, BillingCompanies> {
					{ BillingCompany.Uuid.Value, BillingCompany }
				}, out _, out _);
				Log.Information("Done update postgres.");

				Log.Information("Begin notify clients.");
				if (null != Program.SignalRConnection) {
						var payload = new PerformNotifyReloadBillingCompanyParams(
						SharedSecret: SharedCode.Hubs.Konstants.SQUARE_PAYMENTS_AND_API_SHARED_SECRET,
						SessionId: SessionId
					);
					Log.Debug($"payload: {payload}");

					_ = Program.SignalRConnection.InvokeAsync<PerformNotifyReloadBillingCompanyResponse>(
						"PerformNotifyReloadBillingCompany", payload).Result;
				}
				Log.Information("Done notify clients.");


			}
			catch (ApiException e) {
				var errors = e.Errors;
				var statusCode = e.ResponseCode;
				var httpContext = e.HttpContext;

				ErrorMessage = $"Square error: {e.Message}";
				return Page();
				//Log.Debug("ApiException occurred:");
				//Log.Debug("Headers:");
				//foreach (var item in httpContext.Request.Headers) {
				//	//Display all the headers except Authorization
				//	if (item.Key != "Authorization") {
				//		Log.Debug("\t{0}: {1}", item.Key, item.Value);
				//	}
				//}
				//Log.Debug("Status Code: {0}", statusCode);
				//foreach (Error error in errors) {
				//	Log.Debug("Error Category:{0} Code:{1} Detail:{2}", error.Category, error.Code, error.Detail);
				//}


			}
			catch (Exception e) {
				ErrorMessage = $"Generic system error: {e.Message}";
				return Page();
				// Your error handling code
			}

			return Page();
		}

	}
}
