using System;
using System.Collections.Generic;
using System.Linq;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace ManuallyProcessPreAuthorizedPayments.Pages.Company.Edit
{
	public class MarketingCampaignModel : PageModel
	{
		public string? ErrorMessage { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;

		[BindProperty]
		public string? Value { get; set; } = null;

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

			var resBC = BillingCompanies.ForIds(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;

			Value = Company.MarketingCampaign;

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
				MarketingCampaign = Value,
			};

			BillingCompanies.Upsert(BillingDB, new Dictionary<Guid, BillingCompanies> {
				{ Id.Value, Company }
			}, out _, out _);

			return Redirect($"/companies/{Id.Value}");
		}
	}
}
