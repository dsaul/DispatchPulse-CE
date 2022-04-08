using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;
using Square;
using Square.Exceptions;
using Square.Models;
using SharedCode;

namespace ManuallyProcessPreAuthorizedPayments.Pages
{
	
	[IgnoreAntiforgeryToken]
	public class PostPaymentModel : PageModel
	{
		private bool SharedSetup() {

			BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
			if (null == BillingDB)
				return false;
			HttpContext.Response.RegisterForDispose(BillingDB);
			BillingDB.Open();

			return true;
		}

		static SquareClient SquareClient { get; set; } = new SquareClient.Builder()
				//.Environment(Square.Environment.Sandbox)
				//.AccessToken(SharedCode.Square.Konstants.SQUARE_SANDBOX_ACCESS_TOKEN)
				.Environment(Square.Environment.Production)
				.AccessToken(EnvSquare.SQUARE_PRODUCTION_ACCESS_TOKEN)
				.Build();

		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;
		public string? ErrorMessage { get; set; } = null;
		public bool PostSuccess { get; set; } = false;

		public IActionResult OnPost(Guid? billingCompanyId, decimal? amount) {
			if (!SharedSetup()) {
				ErrorMessage = "Setup Failed";
				return Page();
			}
			if (null == BillingDB) {
				ErrorMessage = "Can't connect to DB.";
				return Page();
			}
			if (null == billingCompanyId) {
				ErrorMessage = "Did not recieve a company id.";
				return Page();
			}

			var paymentsApi = SquareClient.PaymentsApi;

			var resBC = BillingCompanies.ForIds(BillingDB, billingCompanyId.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;

			Company = BillingCompanies.EnsureCompanyHasSquareAccount(SquareClient, Company).Result;

			if (null == Company) {
				ErrorMessage = "Can't find company info.";
				return Page();
			}

			if (null == amount) {
				ErrorMessage = "Unable to understand the deposit amount.";
				return Page();
			}

			if (amount < 1m) {
				ErrorMessage = "The minimum credit card payment is $1.";
				return Page();
			}

			long chargeAmountL = Convert.ToInt64(amount * 100);


			try {

				CreatePaymentRequest createPaymentRequest = new CreatePaymentRequest(
					sourceId: Company.SquareCardId,
					idempotencyKey: Guid.NewGuid().ToString(),
					amountMoney:new Money(chargeAmountL, Company.Currency),
					customerId: Company.SquareCustomerId,
					note: "Account Payment",
					statementDescriptionIdentifier: "SandP Acct Pmt"
					);

				CreatePaymentResponse response = paymentsApi.CreatePayment(createPaymentRequest);
				Payment payment = response.Payment;

				decimal squareFeePercent = 2.9m / 100m;
				decimal squareFeeFixed = 0.3m;

				decimal feesAmt = decimal.Round(amount.Value * squareFeePercent, 2, MidpointRounding.AwayFromZero) + squareFeeFixed;



				Guid accountDepositSubFeesId = Guid.NewGuid();
				BillingJournalEntries accountDepositSubFees = new BillingJournalEntries(
					Uuid: accountDepositSubFeesId,
					Timestamp: DateTime.UtcNow,
					Type: BillingJournalEntries.kTypeValuePayment,
					OtherEntryId: null,
					Description: "Payment Recieved - Thank You",
					Quantity: 1m,
					UnitPrice: (amount.Value - feesAmt) * -1m,
					Currency: payment.ApprovedMoney.Currency,
					TaxPercentage: 0m,
					TaxActual: 0m,
					Total: (amount.Value - feesAmt) * -1m,
					InvoiceId: null,
					PackageId: null,
					CompanyId: Company.Uuid,
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

				Guid preAuthorizedFeesCreditId = Guid.NewGuid();
				BillingJournalEntries preAuthorizedFeesCredit = new BillingJournalEntries(
					Uuid: preAuthorizedFeesCreditId,
					Timestamp: DateTime.UtcNow,
					Type: BillingJournalEntries.kTypeValueAccountCredit,
					OtherEntryId: null,
					Description: "Crediting Credit Card Fees Due to Pre-authorized payments",
					Quantity: 1m,
					UnitPrice: (feesAmt) * -1m,
					Currency: payment.ApprovedMoney.Currency,
					TaxPercentage: 0m,
					TaxActual: 0m,
					Total: (feesAmt) * -1m,
					InvoiceId: null,
					PackageId: null,
					CompanyId: Company.Uuid,
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
						[BillingJournalEntries.kJsonKeyCreditCardFeeCredit] = true,

					}.ToString(Newtonsoft.Json.Formatting.Indented)
				);

				BillingJournalEntries.Upsert(BillingDB, new Dictionary<Guid, BillingJournalEntries> {
					{ accountDepositSubFeesId, accountDepositSubFees },
					{ preAuthorizedFeesCreditId, preAuthorizedFeesCredit }
				}, out _, out _);


				PostSuccess = true;

				//if (null != Program.SignalRConnection) {
				//	var payload = new PerformNotifyReloadBillingCompanyParams(
				//		SharedSecret: SharedCode.Hubs.Konstants.SQUARE_PAYMENTS_AND_API_SHARED_SECRET,
				//		SessionId: SessionId
				//	);
				//	Log.Debug($"payload: {payload}");

				//	_ = Program.SignalRConnection.InvokeAsync<PerformNotifyReloadBillingCompanyResponse>(
				//		"PerformNotifyReloadBillingCompany", payload).Result;
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
