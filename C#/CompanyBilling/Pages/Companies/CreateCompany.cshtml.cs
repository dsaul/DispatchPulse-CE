using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace ManuallyProcessPreAuthorizedPayments.Pages.Company
{
	public class CreateCompanyModel : PageModel
	{

		public string? ErrorMessage { get; set; } = null;


		[BindProperty] public string? CompanyFullName { get; set; } = null;
		[BindProperty] public string? CompanyAbbreviation { get; set; } = null;
		[BindProperty] public string? CompanyIndustry { get; set; } = null;
		[BindProperty] public string? CompanyMarketingCampaign { get; set; } = null;
		[BindProperty] public string? CompanyAddressCity { get; set; } = null;
		[BindProperty] public string? CompanyAddressCountry { get; set; } = null;
		[BindProperty] public string? CompanyAddressLine1 { get; set; } = null;
		[BindProperty] public string? CompanyAddressLine2 { get; set; } = null;
		[BindProperty] public string? CompanyAddressPostalCode { get; set; } = null;
		[BindProperty] public string? CompanyAddressProvince { get; set; } = null;
		[BindProperty] public string? CompanyPaymentMethod { get; set; } = BillingCompanies.kPaymentMethodValueInvoice;
		[BindProperty] public string? CompanyInvoiceContactId { get; set; } = null;
		[BindProperty] public string? CompanyPaymentFrequency { get; set; } = BillingCompanies.kPaymentFrequenciesValueAnnually;
		[BindProperty] public string? CompanyPhoneId { get; set; } = null;
		[BindProperty] public string? CompanyS3BucketName { get; set; } = null;
		[BindProperty] public string? SquareCustomerId { get; set; } = null;
		[BindProperty] public string? CompanyCurrency { get; set; } = null;


		public IActionResult OnGet()
		{
			return Page();
		}

		public IActionResult OnPost() {
			
			do {
				NpgsqlConnection? billingConnection = null;

				// Validate other stuff.
				string billingConnectionString = Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME);
				if (string.IsNullOrWhiteSpace(billingConnectionString)) {
					ErrorMessage = "Couldn't get the billing connection string.";
					break;
				}

				billingConnection = new NpgsqlConnection(billingConnectionString);
				if (null == billingConnection) {
					ErrorMessage = "Couldn't create the Npgsql instance.";
					break;
				}

				billingConnection.Open();
				if (billingConnection.State != System.Data.ConnectionState.Open) {
					ErrorMessage = "Couldn't open a connection to the database";
					break;
				}

				// Validate Form
				if (string.IsNullOrWhiteSpace(CompanyFullName)) {
					ErrorMessage = "The company name cannot be empty.";
					break;
				}
				if (string.IsNullOrWhiteSpace(CompanyAbbreviation)) {
					ErrorMessage = "The company abbreviation cannot be empty.";
					break;
				}

				string abbrLower = CompanyAbbreviation.Trim().ToLower(Konstants.KDefaultCulture);

				// Make sure the abbreviation isn't already used.
				Dictionary<Guid, BillingCompanies> existingForAbbr = BillingCompanies.ForAbbreviation(billingConnection, abbrLower);
				if (existingForAbbr.Count > 0) {
					ErrorMessage = "The company abbreviation already exists.";
					break;
				}

				if (!string.IsNullOrWhiteSpace(CompanyIndustry)) {
					CompanyIndustry = CompanyIndustry.Trim();
				}

				if (!string.IsNullOrWhiteSpace(CompanyMarketingCampaign)) {
					CompanyMarketingCampaign = CompanyMarketingCampaign.Trim();
				}

				// Address
				if (string.IsNullOrWhiteSpace(CompanyAddressLine1)) {
					ErrorMessage = "We require your company's address line 1.";
					break;
				}

				CompanyAddressLine1 = CompanyAddressLine1.Trim();

				if (!string.IsNullOrWhiteSpace(CompanyAddressLine2)) {
					CompanyAddressLine2 = CompanyAddressLine2.Trim();
				}

				if (string.IsNullOrWhiteSpace(CompanyAddressCity)) {
					ErrorMessage = "We require your company's address city.";
					break;
				}

				CompanyAddressCity = CompanyAddressCity.Trim();

				if (string.IsNullOrWhiteSpace(CompanyAddressProvince)) {
					ErrorMessage = "We require your company's address province.";
					break;
				}
				CompanyAddressProvince = CompanyAddressProvince.Trim();

				if (string.IsNullOrWhiteSpace(CompanyAddressPostalCode)) {
					ErrorMessage = "We require your company's address postal code.";
					break;
				}

				CompanyAddressPostalCode = CompanyAddressPostalCode.Trim();

				if (string.IsNullOrWhiteSpace(CompanyAddressCountry)) {
					ErrorMessage = "We require your company's address country.";
					break;
				}

				CompanyAddressCountry = CompanyAddressCountry.Trim();



				JObject json = new JObject {
					[BillingCompanies.kJsonKeyPhoneId] = CompanyPhoneId,
					[BillingCompanies.kJsonKeyS3BucketName] = CompanyS3BucketName,
					[BillingCompanies.kJsonKeySquareCustomerId] = SquareCustomerId,
					[BillingCompanies.kJsonKeyCurrency] = CompanyCurrency,
					[BillingCompanies.kJsonKeyIANATimezone] = BillingCompanies.kJsonValueIANATimezoneDefault,
				};

				// Create the company entry.
				Guid newCompanyId = Guid.NewGuid();
				BillingCompanies newCompany = new BillingCompanies(
					Uuid: newCompanyId,
					FullName: CompanyFullName,
					Abbreviation: abbrLower,
					Industry: CompanyIndustry,
					MarketingCampaign: CompanyMarketingCampaign,
					AddressLine1: CompanyAddressLine1,
					AddressLine2: CompanyAddressLine2,
					AddressCity: CompanyAddressCity,
					AddressProvince: CompanyAddressProvince,
					AddressPostalCode: CompanyAddressPostalCode,
					AddressCountry: CompanyAddressCountry,
					StripeCustomerId: null,
					PaymentMethod: CompanyPaymentMethod,
					PaymentFrequency: CompanyPaymentFrequency,
					InvoiceContactId: CompanyInvoiceContactId == null ? null : Guid.Parse(CompanyInvoiceContactId),
					Json: json.ToString(Newtonsoft.Json.Formatting.Indented)
					);
				BillingCompanies.Upsert(billingConnection, new Dictionary<Guid, BillingCompanies>
				{
					{ newCompanyId, newCompany }
				}, out _, out _);

				return Redirect($"/companies/{newCompanyId}");


			} while (false);

			return Page();
		}

	}
}
