using API.Hubs;
using API.Utility;
using Databases.Records.Billing;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;
using Square;
using Square.Exceptions;
using Square.Models;
using SquarePayments.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using SharedCode;

namespace SquarePayments.Pages
{
	[IgnoreAntiforgeryToken(Order = 2000)]
	public class OneTimePaymentModel : PageModel
	{
		private readonly ILogger<OneTimePaymentModel> _logger;

		public OneTimePaymentModel(ILogger<OneTimePaymentModel> logger)
		{
			_logger = logger;
		}


		[BindProperty(SupportsGet = true)]
		public Guid? SessionId { get; set; }
		[BindProperty(SupportsGet = true)]
		public string? Nonce { get; set; }
		[BindProperty]
		public string? FormDepositAmount { get; set; } = null;


		public NpgsqlConnection? BillingDB { get; set; } = null;
		public NpgsqlConnection? DPDB { get; set; } = null;
		public BillingSessions? BillingSession { get; set; } = null;
		public BillingContacts? BillingContact { get; set; } = null;

		public BillingCompanies? BillingCompany { get; set; } = null;
		public BillingSubscriptions? BillingSubscription { get; set; } = null;
		public HashSet<string> Permissions { get; } = new HashSet<string>();
		public bool CanAccessOneTimePayments { get; set; } = false;
		public string? ErrorMessage { get; set; } = null;

		static SquareClient SquareClient { get; set; } = new SquareClient.Builder()
				//.Environment(Square.Environment.Sandbox)
				//.AccessToken(SharedCode.Square.Konstants.SQUARE_SANDBOX_ACCESS_TOKEN)
				.Environment(Square.Environment.Production)
				.AccessToken(EnvSquare.SQUARE_PRODUCTION_ACCESS_TOKEN)
				.Build();
		
		private bool SharedSetup() {

			if (null == SessionId) {
				return false;
			}

			//BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
			//if (null == BillingDB)
			//	return false;
			//BillingDB.Open();

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

			CanAccessOneTimePayments = Permissions.Contains(Databases.Konstants.kPermBillingCanMakeOneTimeCompanyCreditCardPayments);

			if (!CanAccessOneTimePayments) {
				return false;
			}


			return true;
		}


		public IActionResult OnGet() {
			if (!SharedSetup())
				return Page();

			return Page();
		}

		public IActionResult OnPost() {
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

			var paymentsApi = SquareClient.PaymentsApi;

			if (null == BillingDB)
				return Page();

			if (null == BillingCompany)
				return Page();

			BillingCompany = BillingCompanies.EnsureCompanyHasSquareAccount(SquareClient, BillingCompany).Result;


			decimal? depositAmountD = null;
			if (decimal.TryParse(FormDepositAmount, out decimal tmpD))
				depositAmountD = tmpD;

			if (null == depositAmountD) {
				ErrorMessage = "Unable to understand the deposit amount.";
				return Page();
			}

			if (depositAmountD < 10m) {
				ErrorMessage = "The minimum credit card payment is $10.";
				return Page();
			}

			decimal fixedFee = 0.30m;
			decimal percentageFee = 2.9m;

			decimal totalWithFees = decimal.Round((depositAmountD.Value + fixedFee) / (1 - (percentageFee / 100)), 2);
			

			// Convert to log for api, (requires converting to cents (*100)).
			long chargeAmountL = Convert.ToInt64(totalWithFees * 100);

			try {

				
				if (null == BillingCompany) {
					throw new Exception("No company information.");
				}

				string? currency = BillingCompany.Currency;
				if (null == currency) {
					throw new Exception("No currency information.");
				}

				CreatePaymentRequest createPaymentRequest = new CreatePaymentRequest(
					sourceId: Nonce,
					idempotencyKey: Guid.NewGuid().ToString(),
					amountMoney:new Money(chargeAmountL, currency),
					customerId: BillingCompany.SquareCustomerId,
					note: "Account Payment",
					statementDescriptionIdentifier: "SandP Acct Pmt"
					);

				CreatePaymentResponse response = paymentsApi.CreatePayment(createPaymentRequest);
				Payment payment = response.Payment;


				


				Guid entryId = Guid.NewGuid();
				BillingJournalEntries entry = new BillingJournalEntries(
					Uuid: entryId,
					Timestamp: DateTime.UtcNow,
					Type: BillingJournalEntries.kTypeValuePayment,
					OtherEntryId: null,
					Description: "Payment - Thank You",
					Quantity: 1m,
					UnitPrice: depositAmountD.Value * -1m,
					Currency: payment.ApprovedMoney.Currency,
					TaxPercentage: 0m,
					TaxActual: 0m,
					Total: depositAmountD.Value * -1m,
					InvoiceId: null,
					PackageId: null,
					CompanyId: BillingCompany.Uuid,
					Json: new JObject { 
						[BillingJournalEntries.kJsonKeySquareCardBrand] = payment.CardDetails.Card.CardBrand,
						[BillingJournalEntries.kJsonKeySquareCardType] = payment.CardDetails.Card.CardType,
						[BillingJournalEntries.kJsonKeySquareCardholderName] = payment.CardDetails.Card.CardholderName,
						[BillingJournalEntries.kJsonKeySquareExpMonth] = payment.CardDetails.Card.ExpMonth,
						[BillingJournalEntries.kJsonKeySquareExpYear] = payment.CardDetails.Card.ExpYear,
						[BillingJournalEntries.kJsonKeySquareFingerprint] = payment.CardDetails.Card.Fingerprint,
						[BillingJournalEntries.kJsonKeySquareLast4] = payment.CardDetails.Card.Last4,
						[BillingJournalEntries.kJsonKeySquareCardPaymentTimelineAuthorizedAt] = payment.CardDetails.CardPaymentTimeline.AuthorizedAt,
						[BillingJournalEntries.kJsonKeySquareCardPaymentTimelineCapturedAt] = payment.CardDetails.CardPaymentTimeline.CapturedAt,
						[BillingJournalEntries.kJsonKeySquareRecieptUrl] = payment.ReceiptUrl,
						[BillingJournalEntries.kJsonKeySquarePaymentId] = payment.Id,
						[BillingJournalEntries.kJsonKeySquareStatus] = payment.Status,
					}.ToString(Newtonsoft.Json.Formatting.Indented)
				);

				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"A Payment was just recieved on account {entry.CompanyId}.");
				sb.AppendLine($"Total posted to account: {entry.Total}");

				//string template = Resources.CCPaymentPostedEmailTemplate;

				Email
					.From(SharedCode.OnCallResponder.Konstants.ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS, "Square Payments")
					.To(SharedCode.Konstants.ACCOUNTS_RECEIVABLE_EMAIL)
					.Subject("Square Payment Recieved")
					.Body(sb.ToString())
					.Send();



				BillingJournalEntries.Upsert(BillingDB, new Dictionary<Guid, BillingJournalEntries> {
					{ entryId, entry }
				}, out _, out _);


				if (null != Program.SignalRConnection) {
					var payload = new PerformNotifyReloadBillingCompanyParams(
						SharedSecret: SharedCode.Hubs.Konstants.SQUARE_PAYMENTS_AND_API_SHARED_SECRET,
						SessionId: SessionId
					);
					Log.Debug($"payload: {payload}");

					_ = Program.SignalRConnection.InvokeAsync<PerformNotifyReloadBillingCompanyResponse>(
						"PerformNotifyReloadBillingCompany", payload).Result;
				}



			}
			catch (ApiException e) {
				var errors = e.Errors;
				var statusCode = e.ResponseCode;
				var httpContext = e.HttpContext;

				ErrorMessage = $"Square error: ${e.Message}";
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
				ErrorMessage = $"Generic system error: ${e.Message}";
				return Page();
				// Your error handling code
			}

			return Page();
		}
	}
}
