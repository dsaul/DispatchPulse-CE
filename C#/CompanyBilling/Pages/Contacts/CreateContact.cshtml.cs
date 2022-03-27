using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using Npgsql;
using XKCDPasswordGen;

namespace ManuallyProcessPreAuthorizedPayments.Pages.Contacts
{
	public class CreateContactModel : PageModel
	{
		public string? ErrorMessage { get; set; } = null;

		public NpgsqlConnection? BillingDB { get; set; } = null;


		[BindProperty] public string? ContactFullName { get; set; } = null;
		[BindProperty] public string? ContactEMail { get; set; } = null;
		[BindProperty] public string? ContactInitialPassword { get; set; } = "";
		[BindProperty] public bool ContactEMailListMarketing { get; set; } = false;
		[BindProperty] public bool ContactEMailListTutorials { get; set; } = false;
		[BindProperty] public string? ContactMarketingCampaign { get; set; } = null;
		[BindProperty] public string? ContactPhoneNumber { get; set; } = null;
		[BindProperty] public string? ContactCompanyId { get; set; } = null;
		[BindProperty] public string? ContactDispatchPulseContactId { get; set; } = null;
		[BindProperty] public string? ContactDispatchPulseAgentId { get; set; } = null;
		[BindProperty] public bool LicenseAssignedOnCall { get; set; } = false;
		[BindProperty] public bool LicenseAssignedProjectsSchedulingTime { get; set; } = false;


		public List<BillingPermissionsGroups> PossibleGroups { get; set; } = new List<BillingPermissionsGroups>();
		public SelectList? GroupListOptions { get; set; }
		[BindProperty] public List<string>? SelectedGroups { get; set; }


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

			if (string.IsNullOrWhiteSpace(ContactInitialPassword)) {
				ContactInitialPassword = XkcdPasswordGen.Generate(4,"-");
			}

			var resGroups = BillingPermissionsGroups.AllMinusHidden(BillingDB);
			PossibleGroups.AddRange(resGroups.Values);

			GroupListOptions = new SelectList(PossibleGroups, nameof(BillingPermissionsGroups.Id), nameof(BillingPermissionsGroups.Name));





			return Page();
		}

		public IActionResult OnPost() {

			do {
				if (!SharedSetup())
					return Page();
				if (null == BillingDB)
					return Page();

				// Validate Form
				if (string.IsNullOrWhiteSpace(ContactFullName)) {
					ErrorMessage = "The contact name cannot be empty.";
					break;
				}

				if (string.IsNullOrWhiteSpace(ContactEMail)) {
					ErrorMessage = "The contact e-mail cannot be empty.";
					break;
				}

				if (string.IsNullOrWhiteSpace(ContactInitialPassword)) {
					ErrorMessage = "The contact initial password cannot be empty.";
					break;
				}

				// ContactEMailListMarketing
				// ContactEMailListTutorials

				if (string.IsNullOrWhiteSpace(ContactMarketingCampaign)) {
					ErrorMessage = "The contact marketing campaign cannot be empty.";
					break;
				}

				if (string.IsNullOrWhiteSpace(ContactPhoneNumber)) {
					ErrorMessage = "The contact phone number cannot be empty.";
					break;
				}



				JObject applicationData = new JObject {
					[BillingContacts.kApplicationDataKeyDispatchPulseAgentId] = null,
					[BillingContacts.kApplicationDataKeyDispatchPulseContactId] = null,
				};

				JObject json = new JObject {
					[BillingContacts.kJsonKeyLicenseAssignedOnCall] = LicenseAssignedOnCall,
					[BillingContacts.kJsonKeyLicenseAssignedProjectsSchedulingTime] = LicenseAssignedProjectsSchedulingTime,
				};


				Guid newContactId = Guid.NewGuid();
				BillingContacts newContact = new BillingContacts(
					FullName: ContactFullName,
					Email: ContactEMail,
					PasswordHash: BCrypt.Net.BCrypt.HashPassword(ContactInitialPassword),
					EmailListMarketing: ContactEMailListMarketing,
					EmailListTutorials: ContactEMailListTutorials,
					MarketingCampaign: ContactMarketingCampaign,
					Phone: ContactPhoneNumber,
					Uuid: newContactId,
					null,
					ApplicationData: applicationData.ToString(Newtonsoft.Json.Formatting.Indented),
					Json: json.ToString(Newtonsoft.Json.Formatting.Indented)
					);

				BillingContacts.Upsert(BillingDB, new Dictionary<Guid, BillingContacts>
				{
					{ newContactId, newContact }
				}, out _, out _);


				Dictionary<Guid, BillingPermissionsGroupsMemberships> groupMemberships = new ();

				if (SelectedGroups != null) {
					foreach (string str in SelectedGroups) {
						if (Guid.TryParse(str, out Guid groupId)) {
							Guid id = Guid.NewGuid();
							groupMemberships.Add(id, new BillingPermissionsGroupsMemberships(
								Id: id,
								GroupId: groupId,
								ContactId: newContactId,
								new JObject().ToString(Newtonsoft.Json.Formatting.Indented)
								));
						}
					}
				}
				

				if (groupMemberships.Count > 0) {
					BillingPermissionsGroupsMemberships.Upsert(BillingDB, groupMemberships, out _, out _);
				}





















				return Redirect($"/contacts/{newContactId}");


			} while (false);

			return Page();
		}
	}
}
