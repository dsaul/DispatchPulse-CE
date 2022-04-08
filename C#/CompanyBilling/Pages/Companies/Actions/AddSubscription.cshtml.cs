using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Npgsql;
using SharedCode;

namespace CompanyBilling.Pages.Companies.Actions
{
	public class AddSubscriptionModel : PageModel
	{
		public string? ErrorMessage { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? CompanyId { get; set; } = null;

		[BindProperty]
		public Guid? SelectedPackageId { get; set; } = null;


		public Dictionary<Guid, BillingPackages> AssignablePackages { get; set; } = new Dictionary<Guid, BillingPackages>();

		private bool SharedSetup() {


			BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
			HttpContext.Response.RegisterForDispose(BillingDB);
			if (null == BillingDB)
				return false;
			BillingDB.Open();




			return true;
		}

		public IActionResult OnGet() 
			{
			if (!SharedSetup())
				return Page();
			if (null == BillingDB)
				return Page();
			if (null == CompanyId)
				return Page();

			var resBC = BillingCompanies.ForIds(BillingDB, CompanyId.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;

			if (null != Company && null != Company.Uuid) {
				var resPkg = BillingPackages.ForAllowNewAssignment(BillingDB, true);
				if (null != resPkg && 0 != resPkg.Count) {
					AssignablePackages.AddRange(resPkg);
				}

			}






			return Page();
		}

		public IActionResult OnPost() {
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

			if (null == SelectedPackageId) {
				ErrorMessage = "Selected package id";
				return Page();
			}

			var resBC = BillingCompanies.ForIds(BillingDB, CompanyId.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;

			Guid subId = Guid.NewGuid();
			

			BillingSubscriptions subscription = new BillingSubscriptions(
				Uuid: subId,
				CompanyId: Company.Uuid,
				PackageId: SelectedPackageId,
				TimestampAddedUtc: DateTime.UtcNow,
				ProvisioningActual: "Manual",
				ProvisioningDesired: "Manual",
				ProvisionedDatabaseName: null,
				TimestampLastSettingsPushUtc: null,
				Json: new JObject {
					
				}.ToString(Newtonsoft.Json.Formatting.Indented)
			);

			BillingSubscriptions.Upsert(BillingDB, new Dictionary<Guid, BillingSubscriptions> {
				{ subId, subscription }
			}, out _, out _);

			return Redirect($"/companies/{CompanyId.Value}");
		}



	}
}
