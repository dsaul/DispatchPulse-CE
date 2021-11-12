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
    public class InvoiceContactModel : PageModel
    {
		public string? ErrorMessage { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;

		[BindProperty]
		public string? Value { get; set; } = null;

		public Dictionary<Guid, BillingContacts> AllCompanyContacts { get; set; } = new Dictionary<Guid, BillingContacts>();

		private bool SharedSetup() {


			BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.KBillingDatabaseName));
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

			AllCompanyContacts = BillingContacts.ForCompany(BillingDB, Id.Value);

			var resBC = BillingCompanies.ForIds(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;

			Value = null == Company.InvoiceContactId ? null : Company.InvoiceContactId.ToString();

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

			if (string.IsNullOrWhiteSpace(Value)) {
				ErrorMessage = "abbreviation can not be empty";
				return Page();
			}

			var resBC = BillingCompanies.ForIds(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;
			

			Company = Company with
			{
				InvoiceContactId = Guid.Parse(Value),
			};

			BillingCompanies.Upsert(BillingDB, new Dictionary<Guid, BillingCompanies> {
				{ Id.Value, Company }
			}, out _, out _);

			return Redirect($"/companies/{Id.Value}");
		}
	}
}
