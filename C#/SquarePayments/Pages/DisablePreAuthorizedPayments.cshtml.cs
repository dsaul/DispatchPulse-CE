using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Hubs;
using API.Utility;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;
using Square;
using Square.Exceptions;
using Square.Models;

namespace SquarePayments.Pages
{
	[IgnoreAntiforgeryToken(Order = 2000)]
	public class DisablePreAuthorizedPaymentsModel : PageModel
	{
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public NpgsqlConnection? DPDB { get; set; } = null;
		public BillingSessions? BillingSession { get; set; } = null;
		public BillingContacts? BillingContact { get; set; } = null;

		public BillingCompanies? BillingCompany { get; set; } = null;
		public BillingSubscriptions? BillingSubscription { get; set; } = null;
		public HashSet<string> Permissions { get; } = new HashSet<string>();
		public bool CanAccessPreAuthorizedPayments { get; set; } = false;


		[BindProperty(SupportsGet = true)]
		public Guid? SessionId { get; set; }


		public bool IsPostSuccess { get; set; } = false;
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

			//BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.KBillingDatabaseName));
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

		public IActionResult OnPost(bool flag) {
			if (!SharedSetup())
				return Page();

			if (false == CanAccessPreAuthorizedPayments) {
				ErrorMessage = "You can't access pre-authorized payments.";
				return Page();
			}

			if (null == BillingCompany) {
				ErrorMessage = "Can't find billing company.";
				return Page();
			}

			if (null == BillingCompany.Uuid) {
				ErrorMessage = "Can't find billing company id.";
				return Page();
			}

			if (null == BillingDB) {
				ErrorMessage = "Can't access database.";
				return Page();
			}


			string? squareCustomerId = BillingCompany.SquareCustomerId;
			if (string.IsNullOrWhiteSpace(squareCustomerId)) {
				ErrorMessage = "Can't find square customer id.";
				return Page();
			}

			string? squareCardId = BillingCompany.SquareCardId;
			if (string.IsNullOrWhiteSpace(squareCardId)) {
				ErrorMessage = "Can't find square card id.";
				return Page();
			}

			var customersApi = SquareClient.CustomersApi;

			DeleteCustomerCardResponse resDeleteCC = customersApi.DeleteCustomerCard(
					squareCustomerId,
					squareCardId
				);



			JObject? jObject = null;

			if (null != BillingCompany.JsonObject) {
				jObject = BillingCompany.JsonObject.DeepClone() as JObject;
			}
			if (null == jObject) {
				jObject = new JObject();
			}

			jObject[BillingCompanies.kJsonKeySquareCardBrand] = null;
			jObject[BillingCompanies.kJsonKeySquareCardExpMonth] = null;
			jObject[BillingCompanies.kJsonKeySquareCardExpYear] = null;
			jObject[BillingCompanies.kJsonKeySquareCardLast4] = null;
			jObject[BillingCompanies.kJsonKeySquareCardId] = null;

			BillingCompany = BillingCompany with
			{
				PaymentMethod = BillingPaymentMethod.kValueInvoice,
				Json = jObject.ToString(Newtonsoft.Json.Formatting.Indented)
			};

			BillingCompanies.Upsert(BillingDB, new Dictionary<Guid, BillingCompanies> {
					{ BillingCompany.Uuid.Value, BillingCompany }
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

			IsPostSuccess = true;

			return Page();
		}
	}
}
