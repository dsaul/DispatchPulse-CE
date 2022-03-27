using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace ManuallyProcessPreAuthorizedPayments.Pages.Company.Edit
{
    public class AddressModel : PageModel
    {
		public string? ErrorMessage { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;

		[BindProperty] public string? ValueCity { get; set; } = null;
		[BindProperty] public string? ValueCountry { get; set; } = null;
		[BindProperty] public string? ValueAddressLine1 { get; set; } = null;
		[BindProperty] public string? ValueAddressLine2 { get; set; } = null;
		[BindProperty] public string? ValuePostalCode { get; set; } = null;
		[BindProperty] public string? ValueProvince { get; set; } = null;

		private bool SharedSetup() {


			BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
			if (null == BillingDB)
				return false;
			HttpContext.Response.RegisterForDispose(BillingDB);
			BillingDB.Open();




			return true;
		}

		public IActionResult OnGet() {
			if (!SharedSetup())
				return Page();
			if (null == BillingDB) {
				ErrorMessage = "null == BillingDB";
				return Page();
			}
			if (null == Id) {
				ErrorMessage = "company id is null";
				return Page();
			}

			var resBC = BillingCompanies.ForIds(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;

			ValueCity = Company.AddressCity;
			ValueCountry = Company.AddressCountry;
			ValueAddressLine1 = Company.AddressLine1;
			ValueAddressLine2 = Company.AddressLine2;
			ValuePostalCode = Company.AddressPostalCode;
			ValueProvince = Company.AddressProvince;

			return Page();
		}

		public IActionResult OnPost() {
			if (!SharedSetup())
				return Page();
			if (null == BillingDB) {
				ErrorMessage = "null == BillingDB";
				return Page();
			}
			if (null == Id) {
				ErrorMessage = "company id is null";
				return Page();
			}

			if (string.IsNullOrWhiteSpace(ValueCity)) {
				ErrorMessage = "city can not be empty";
				return Page();
			}

			if (string.IsNullOrWhiteSpace(ValueCountry)) {
				ErrorMessage = "country can not be empty";
				return Page();
			}

			if (string.IsNullOrWhiteSpace(ValueAddressLine1)) {
				ErrorMessage = "address line 1 can not be empty";
				return Page();
			}
			if (string.IsNullOrWhiteSpace(ValuePostalCode)) {
				ErrorMessage = "postal code can not be empty";
				return Page();
			}
			if (string.IsNullOrWhiteSpace(ValueProvince)) {
				ErrorMessage = "province can not be empty";
				return Page();
			}

			var resBC = BillingCompanies.ForIds(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;

			
			Company = Company with
			{
				AddressCity = ValueCity,
				AddressCountry = ValueCountry,
				AddressLine1 = ValueAddressLine1,
				AddressLine2 = ValueAddressLine2,
				AddressPostalCode = ValuePostalCode,
				AddressProvince = ValueProvince,
			};

			BillingCompanies.Upsert(BillingDB, new Dictionary<Guid, BillingCompanies> {
				{ Id.Value, Company }
			}, out _, out _);

			return Redirect($"/companies/{Id.Value}");
		}
	}
}
