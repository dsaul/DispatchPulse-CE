using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using SharedCode.Databases.Records;
using SharedCode.Extensions;

namespace CompanyBilling.Pages.Companies.Actions
{
	public class CreateDatabaseModel : PageModel
	{
		public string? ErrorMessage { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;
		public BillingSubscriptions? Subscription { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? CompanyId { get; set; } = null;
		[BindProperty(SupportsGet = true)]
		public Guid? SubscriptionId { get; set; } = null;

		[BindProperty]
		public string DatabaseNameUniqueSearchFragment { get; set; } = "";


		private bool SharedSetup() {


			BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.KBillingDatabaseName));
			
			if (null == BillingDB)
				return false;

			HttpContext.Response.RegisterForDispose(BillingDB);

			if (null == CompanyId)
				return false;
			if (null == SubscriptionId)
				return false;

			BillingDB.Open();

			var resBC = BillingCompanies.ForIds(BillingDB, CompanyId.Value);
			if (0 == resBC.Count) {
				return false;
			}

			Company = resBC.FirstOrDefault().Value;

			var resSub = BillingSubscriptions.ForId(BillingDB, SubscriptionId.Value);
			if (0 == resSub.Count) {
				return false;
			}

			Subscription = resSub.FirstOrDefault().Value;

			return true;
		}


		public IActionResult OnGet() {
			if (!SharedSetup())
				return Page();

			if (null == Company)
				return Page();
			if (null == Subscription)
				return Page();



			DatabaseNameUniqueSearchFragment = string.IsNullOrWhiteSpace(Company.Abbreviation) ? "no_abbreviation" : Company.Abbreviation.ToLower();






			return Page();
		}

		public IActionResult OnPost() {
			if (!SharedSetup())
				return Page();
			if (null == Company)
				return Page();
			if (null == Company.Uuid)
				return Page();
			if (null == Subscription)
				return Page();
			if (null == Subscription.Uuid)
				return Page();
			if (null == BillingDB)
				return Page();

			string? actualDBName = null;
			using NpgsqlConnection? noDatabaseConnection = new NpgsqlConnection(Databases.Konstants.NPGSQL_CONNECTION_STRING);
			noDatabaseConnection.Open();
			{
				actualDBName = noDatabaseConnection.CreateDatabase(DatabaseNameUniqueSearchFragment);
			}
			noDatabaseConnection.Close();

			if (string.IsNullOrWhiteSpace(actualDBName)) {
				ErrorMessage = "Couldn't find a database name that wasn't used.";
				return Page();
			}

			using NpgsqlConnection db = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(actualDBName));
			db.Open();
			{
				db.EnsureUUIDExtension();
				db.EnsureTimestampISO8601();
				Verification.RunAllDispatchPulseVerifications(db, insertDefaultContents: true);
			}
			db.Close();


			var resSubs = BillingSubscriptions.ForCompanyId(BillingDB, Company.Uuid.Value);
			foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in resSubs) {

				Guid? packageId = kvp.Value.PackageId;
				if (null == packageId)
					continue;

				var pkgResults = BillingPackages.ForId(BillingDB, packageId.Value);
				if (pkgResults.Count == 0)
					continue;

				BillingPackages package = pkgResults.First().Value;


				if (
					(
						(null != package.ProvisionDispatchPulse && package.ProvisionDispatchPulse.Value) ||
						(null != package.ProvisionOnCallAutoAttendants && package.ProvisionOnCallAutoAttendants.Value)
					)
					&&
					null != kvp.Value.Uuid
					) {
					// update provisioned database name

					BillingSubscriptions mod = kvp.Value with { ProvisionedDatabaseName = actualDBName };
					BillingSubscriptions.Upsert(BillingDB, new Dictionary<Guid, BillingSubscriptions> {
						{ mod.Uuid.Value, mod }
					}, out _, out _);

				}


			}



			return Redirect($"/companies/{Company.Uuid.Value}");
		}



	}
}
