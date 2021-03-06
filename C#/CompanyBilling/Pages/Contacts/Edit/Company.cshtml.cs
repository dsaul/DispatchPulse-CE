using System;
using System.Collections.Generic;
using System.Linq;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace CompanyBilling.Pages.Contacts.Edit
{
	public class CompanyModel : PageModel
	{
		public string? ErrorMessage { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingContacts? Contact { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;

		[BindProperty]
		public string? Value { get; set; } = null;

		public Dictionary<Guid, BillingCompanies> AllCompanies { get; set; } = new Dictionary<Guid, BillingCompanies>();

		private bool SharedSetup() {


			BillingDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(EnvDatabases.BILLING_DATABASE_NAME));
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

			AllCompanies = BillingCompanies.All(BillingDB);

			var resContacts = BillingContacts.ForId(BillingDB, Id.Value);
			if (0 == resContacts.Count)
				return Page();

			Contact = resContacts.FirstOrDefault().Value;

			Value = null == Contact.CompanyId ? null : Contact.CompanyId.ToString();

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



			var resContacts = BillingContacts.ForId(BillingDB, Id.Value);
			if (0 == resContacts.Count)
				return Page();

			Contact = resContacts.FirstOrDefault().Value;


			Contact = Contact with {
				CompanyId = Guid.Parse(Value),
			};

			BillingContacts.Upsert(BillingDB, new Dictionary<Guid, BillingContacts> {
				{ Id.Value, Contact }
			}, out _, out _);

			return Redirect($"/contacts/{Id.Value}");
		}
	}
}
