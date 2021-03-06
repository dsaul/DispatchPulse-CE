using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using SharedCode;

namespace CompanyBilling.Pages.Companies.Actions
{
	public class VerifyDatabaseModel : PageModel
	{
		public List<string> Log { get; set; } = new List<string>();
		public string? ErrorMessage { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;
		[BindProperty(SupportsGet = true)]
		public Guid? CompanyId { get; set; } = null;


		[BindProperty(SupportsGet = true)]
		public string? DatabaseName { get; set; } = null;



		[BindProperty]
		public bool InsertDefaultContents { get; set; } = false;

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
			if (null == CompanyId) {
				ErrorMessage = "company id is null";
				return Page();
			}

			var resBC = BillingCompanies.ForIds(BillingDB, CompanyId.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;


			return Page();
		}


		public IActionResult OnPost() {
			if(!SharedSetup())
				return Page();
			if (null == BillingDB) {
				ErrorMessage = "null == BillingDB";
				return Page();
			}
			if (null == CompanyId) {
				ErrorMessage = "company id is null";
				return Page();
			}
			if (string.IsNullOrWhiteSpace(DatabaseName)) {
				ErrorMessage = "database name is null";
				return Page();
			}


			Log.Add($"---- DPVerify.Verify {DatabaseName}");

			using NpgsqlConnection db = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(DatabaseName));
			db.Open();

			Log.Add("---- Ensuring UUID extension exists.");
			db.EnsureUUIDExtension();


			Log.Add("---- Ensuring timestamp_iso8601 exists");
			db.EnsureTimestampISO8601();

			Log.Add("---- Verify Tables:");
			Verification.VerifyDPClientDatabase(db, insertDefaultContents: InsertDefaultContents);







			return Page();
		}


	}
}
