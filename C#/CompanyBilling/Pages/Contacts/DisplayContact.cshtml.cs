using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace ManuallyProcessPreAuthorizedPayments.Pages.Contacts
{
	public class DisplayContactModel : PageModel
	{
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingContacts? Contact { get; set; } = null;
		public BillingCompanies? Company { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;


		public bool DisplayContactEMailListMarketing { get; set; } = false;
		public bool DisplayContactEMailListTutorials { get; set; } = false;
		public bool DisplayLicenseAssignedPST { get; set; } = false;
		public bool DisplayLicenseAssignedOnCall { get; set; } = false;
		public string DisplayGroupsStr { get; set; } = "asdasd";
		public string DisplayContactCompany { get; set; } = "(none)";


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

			var resContact = BillingContacts.ForId(BillingDB, Id.Value);
			if (0 == resContact.Count)
				return Page();

			Contact = resContact.FirstOrDefault().Value;

			if (null != Contact && null != Contact.CompanyId) {
				DisplayContactCompany = $"{Contact.CompanyId}";
			}

			
			if (null != Contact && null != Contact.CompanyId) {
				var resCompany = BillingCompanies.ForIds(BillingDB, Contact.CompanyId.Value);
				if (0 != resCompany.Count) {
					Company = resCompany.FirstOrDefault().Value;
					DisplayContactCompany = $"{Company.FullName}";
				}
			}
			

			DisplayContactEMailListMarketing = null == Contact || Contact.EmailListMarketing == null ? false : Contact.EmailListMarketing.Value;
			DisplayContactEMailListTutorials = null == Contact || Contact.EmailListTutorials == null ? false : Contact.EmailListTutorials.Value;

			DisplayLicenseAssignedPST = null == Contact || Contact.LicenseAssignedProjectsSchedulingTime == null ? false : Contact.LicenseAssignedProjectsSchedulingTime.Value;
			DisplayLicenseAssignedOnCall = null == Contact || Contact.LicenseAssignedOnCall == null ? false : Contact.LicenseAssignedOnCall.Value;

			

			StringBuilder sb = new StringBuilder();
			var resGroupMembership = BillingPermissionsGroupsMemberships.ForBillingContactId(BillingDB, Id.Value);
			var keys = resGroupMembership.Keys.ToList();
			for (int i = 0; i < keys.Count; i++) {
				BillingPermissionsGroupsMemberships membership = resGroupMembership[keys[i]];
				Guid? groupId = membership.GroupId;
				if (null == groupId)
					continue;

				var resGroup = BillingPermissionsGroups.ForId(BillingDB, groupId.Value);

				BillingPermissionsGroups group = resGroup.FirstOrDefault().Value;

				sb.Append(group.Name);

				if (i != keys.Count - 1)
					sb.Append(", ");
				if (i == keys.Count - 2)
					sb.Append("and ");

			}

			DisplayGroupsStr = sb.ToString();


			return Page();
		}
	}
}
