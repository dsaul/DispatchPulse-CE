using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace ManuallyProcessPreAuthorizedPayments.Pages
{
	public record SubscriptionDisplayRow(
		BillingSubscriptions Subscription,
		BillingPackages Package
		);

	public class CompanyModel : PageModel {
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;

		public BillingContacts? InvoiceContact { get; set; } = null;
		public Dictionary<Guid, BillingJournalEntries>? JournalEntries { get; set;} = null;
		public List<SubscriptionDisplayRow>? Subscriptions { get; set; } = null;
		public Dictionary<Guid, BillingPackages>? Packages { get; set; } = null;
		public Dictionary<Guid, BillingContacts>? Contacts { get; set; } = null;

		public string? AddressForDisplay { get; set; } = null;

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
			if (null == BillingDB)
				return Page();
			if (null == Id)
				return Page();

			var resBC = BillingCompanies.ForIds(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Company = resBC.FirstOrDefault().Value;

			StringBuilder sb = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(Company.AddressLine1)) {
				sb.AppendLine(Company.AddressLine1);
			}
			if (!string.IsNullOrWhiteSpace(Company.AddressLine2)) {
				sb.AppendLine(Company.AddressLine2);
			}
			if (!string.IsNullOrWhiteSpace(Company.AddressCity)) {
				sb.Append(Company.AddressCity);
				sb.Append(' ');
			}
			if (!string.IsNullOrWhiteSpace(Company.AddressProvince)) {
				sb.Append(Company.AddressProvince);
				sb.Append(' ');
			}
			if (!string.IsNullOrWhiteSpace(Company.AddressCountry)) {
				sb.Append(Company.AddressCountry);
				sb.Append(' ');
			}
			if (!string.IsNullOrWhiteSpace(Company.AddressPostalCode)) {
				sb.Append(Company.AddressPostalCode);
				sb.Append(' ');
			}
			AddressForDisplay = sb.ToString();

			if (null != Company.InvoiceContactId) {
				var resContact = BillingContacts.ForId(BillingDB, Company.InvoiceContactId.Value);
				if (0 != resContact.Count) {
					InvoiceContact = resContact.FirstOrDefault().Value;
				}
			}

			if (null != Company.Uuid) {
				var resJE = BillingJournalEntries.ForCompanyId(BillingDB, Company.Uuid.Value);
				if (0 != resJE.Count) {
					JournalEntries = resJE;
				}

			}

			

			if (null != Company.Uuid) {
				var resPkg = BillingPackages.All(BillingDB);
				if (0 != resPkg.Count) {
					Packages = resPkg;
				}

			}

			if (null != Company.Uuid) {
				var resSub = BillingSubscriptions.ForCompanyId(BillingDB, Company.Uuid.Value);
				if (0 != resSub.Count) {

					Subscriptions = new List<SubscriptionDisplayRow>();

					foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in resSub) {

						var set = from row in Packages
								  where row.Value.Uuid == kvp.Value.PackageId
								  select row;

						BillingPackages pkg = set.FirstOrDefault().Value;

						if (null == pkg)
							continue;

						Subscriptions.Add(new SubscriptionDisplayRow(
							Subscription: kvp.Value,
							Package: pkg
							));
					}





				}

			}

			if (null != Company.Uuid) {

				var resContacts = BillingContacts.ForCompany(BillingDB, Company.Uuid.Value);
				if (0 != resContacts.Count) {
					Contacts = resContacts;
				}
			}


				



			return Page();
		}
	}
}
