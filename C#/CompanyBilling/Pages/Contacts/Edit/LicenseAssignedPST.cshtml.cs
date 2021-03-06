using System;
using System.Collections.Generic;
using System.Linq;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace CompanyBilling.Pages.Contacts.Edit
{
	public class LicenseAssignedPSTModel : PageModel
	{
		public string? ErrorMessage { get; set; } = null;

		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingContacts? Contact { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;

		[BindProperty]
		public bool Value { get; set; } = false;

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
				ErrorMessage = "contact id is null";
				return Page();
			}

			var resBC = BillingContacts.ForId(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Contact = resBC.FirstOrDefault().Value;

			Value = Contact.LicenseAssignedProjectsSchedulingTime ?? false;


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
				ErrorMessage = "contact id is null";
				return Page();
			}

			var resBC = BillingContacts.ForId(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Contact = resBC.FirstOrDefault().Value;

			JObject? json = Contact.JsonObject;
			if (null == json)
				json = new JObject();

			json[BillingContacts.kJsonKeyLicenseAssignedProjectsSchedulingTime] = Value;


			Contact = Contact with {
				Json = json.ToString(Newtonsoft.Json.Formatting.Indented),
			};

			BillingContacts.Upsert(BillingDB, new Dictionary<Guid, BillingContacts> {
				{ Id.Value, Contact }
			}, out _, out _);

			return Redirect($"/contacts/{Id.Value}");
		}
	}
}
