using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace CompanyBilling.Pages.Contacts.Edit
{
	public class DispatchPulseAgentIdModel : PageModel
	{
		public string? ErrorMessage { get; set; } = null;

		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingContacts? Contact { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;

		[BindProperty]
		public string? Value { get; set; } = null;

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
				ErrorMessage = "contact id is null";
				return Page();
			}

			var resBC = BillingContacts.ForId(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Contact = resBC.FirstOrDefault().Value;

			Value = Contact.DPAgentId == null ? null : Contact.DPAgentId.ToString();


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

			Contact = Contact with {
				DPAgentId = string.IsNullOrWhiteSpace(Value) ? null : Guid.Parse(Value),
			};

			BillingContacts.Upsert(BillingDB, new Dictionary<Guid, BillingContacts> {
				{ Id.Value, Contact }
			}, out _, out _);

			return Redirect($"/contacts/{Id.Value}");
		}
	}
}
