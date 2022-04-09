using System;
using System.Collections.Generic;
using System.Linq;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace CompanyBilling.Pages.Contacts.Edit
{
	public class PermissionsGroupsModel : PageModel
	{
		public string? ErrorMessage { get; set; } = null;

		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingContacts? Contact { get; set; } = null;

		[BindProperty(SupportsGet = true)]
		public Guid? Id { get; set; } = null;

		[BindProperty]
		public bool Value { get; set; } = false;


		public List<BillingPermissionsGroups> PossibleGroups { get; set; } = new List<BillingPermissionsGroups>();
		public SelectList? GroupListOptions { get; set; }
		[BindProperty] public List<string> SelectedGroups { get; set; } = new List<string>();

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
				ErrorMessage = "contact id is null";
				return Page();
			}

			var resBC = BillingContacts.ForId(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Contact = resBC.FirstOrDefault().Value;


			var resGroups = BillingPermissionsGroups.AllMinusHidden(BillingDB);
			PossibleGroups.AddRange(resGroups.Values);

			GroupListOptions = new SelectList(PossibleGroups, nameof(BillingPermissionsGroups.Id), nameof(BillingPermissionsGroups.Name));

			var resGroupMembership = BillingPermissionsGroupsMemberships.ForBillingContactId(BillingDB, Id.Value);
			foreach (BillingPermissionsGroupsMemberships mem in resGroupMembership.Values) {
				if (null == mem.GroupId)
					continue;
				SelectedGroups.Add(mem.GroupId.Value.ToString());
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
			if (null == Id) {
				ErrorMessage = "contact id is null";
				return Page();
			}

			var resBC = BillingContacts.ForId(BillingDB, Id.Value);
			if (0 == resBC.Count)
				return Page();

			Contact = resBC.FirstOrDefault().Value;

			// Parse form data
			List<Guid> parsed = new List<Guid>();
			foreach (string str in SelectedGroups) {
				if (Guid.TryParse(str, out Guid result)) {
					parsed.Add(result);
				}
			}

			// Delete the items not in the list.
			var resDL = BillingPermissionsGroupsMemberships.ForDeletionListForContact(BillingDB, contactId: Id.Value, excludeGroupIds: parsed);
			BillingPermissionsGroupsMemberships.Delete(BillingDB, resDL.Keys);

			var membershipsAfterDeletion = BillingPermissionsGroupsMemberships.ForBillingContactId(BillingDB, Id.Value);


			Dictionary<Guid, BillingPermissionsGroupsMemberships> groupsToAdd = new ();

			foreach (Guid groupId in parsed) {

				var res = from row in membershipsAfterDeletion
						  where row.Value.GroupId == groupId
						  select row;
				if (res.Any()) {
					continue;
				}

				Guid newId = Guid.NewGuid();
				groupsToAdd.Add(newId, new BillingPermissionsGroupsMemberships(
					newId, groupId, Id.Value, new JObject().ToString(Newtonsoft.Json.Formatting.Indented)
					));

			}




			BillingPermissionsGroupsMemberships.Upsert(BillingDB, groupsToAdd, out _, out _);

			return Redirect($"/contacts/{Id.Value}");
		}



	}
}
