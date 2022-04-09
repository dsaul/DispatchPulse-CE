using System;
using System.Collections.Generic;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace ManuallyProcessPreAuthorizedPayments.Pages.Company
{
	public class IndexModel : PageModel
	{
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public Dictionary<Guid, BillingCompanies>? AllCompanies { get; set; } = null;
		public Dictionary<Guid, BillingCompanies>? CompaniesPreAuth { get; set; } = null;
		public Dictionary<Guid, BillingCompanies>? CompaniesInvoice { get; set; } = null;

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
			if (null == BillingDB)
				return Page();

			AllCompanies = BillingCompanies.All(BillingDB);

			CompaniesPreAuth = BillingCompanies.ForPaymentMethod(BillingDB, BillingCompanies.kPaymentMethodValueSquarePreAuthorized);

			CompaniesInvoice = BillingCompanies.ForPaymentMethod(BillingDB, BillingCompanies.kPaymentMethodValueInvoice);

			return Page();
		}
		
	}
}
