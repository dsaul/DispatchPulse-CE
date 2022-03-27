using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace ManuallyProcessPreAuthorizedPayments.Pages.Contacts
{
	public class IndexModel : PageModel
	{
		public record ContactRowData(BillingContacts Contact, BillingCompanies? Company);


		public NpgsqlConnection? BillingDB { get; set; } = null;
		public List<ContactRowData> AllContacts { get; set; } = new List<ContactRowData>();


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
			if (null == BillingDB)
				return Page();

			var resBC = BillingContacts.All(BillingDB);
			foreach (KeyValuePair<Guid, BillingContacts> kvp in resBC) {
				BillingContacts bc = kvp.Value;
				BillingCompanies? comp = null;
				do {
					if (null == bc.CompanyId) {
						break;
					}

					var resComp = BillingCompanies.ForIds(BillingDB, bc.CompanyId.Value);
					if (resComp.Count == 0)
						break;

					comp = resComp.FirstOrDefault().Value;


				} while (false);

				AllContacts.Add(new ContactRowData(bc, comp));

			}

			return Page();
		}
	}
}
