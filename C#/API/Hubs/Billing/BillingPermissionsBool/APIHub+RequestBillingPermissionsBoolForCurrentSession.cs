using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Databases.Records.Billing;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestBillingPermissionsBoolParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
		}
		public class RequestBillingPermissionsBoolResponse : IdempotencyResponse
		{
			public List<BillingPermissionsBool> BillingPermissionsBool { get; } = new List<BillingPermissionsBool> { };
		}
		public async Task RequestBillingPermissionsBoolForCurrentSession(RequestBillingPermissionsBoolParams p)
		{

			RequestBillingPermissionsBoolResponse response = new RequestBillingPermissionsBoolResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;

				BillingSessions? session = null;
				BillingCompanies? billingCompany = null;

				SessionUtils.GetSessionInformation(
					this,
					response,
					p.SessionId,
					out _,
					out billingConnection,
					out session,
					out billingContact,
					out billingCompany,
					out _,
					out _,
					out dpDBConnection
					);

				if (null != response.IsError && response.IsError.Value)
					break;

				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to billing database.";
					break;
				}

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}

				if (null == billingContact || null == billingContact.CompanyId)
				{
					response.IsError = true;
					response.ErrorMessage = "No company id.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				bool permAll = permissions.Contains(Databases.Konstants.kPermBillingPermissionsBoolReadAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermBillingPermissionsBoolReadCompany);
				bool permSelf = true; // permissions.Contains(Databases.Konstants.kPermBillingPermissionsBoolReadSelf)

				if (permAll || permCompany)
				{
					// return everyone in company.

					List<Guid> billingContactIds = new List<Guid>();

					// First get a list of all the contacts in the company.
					Dictionary<Guid, BillingContacts> allContacts = 
						BillingContacts.ForCompany(billingConnection, billingContact.CompanyId.Value);

					billingContactIds.AddRange(allContacts.Keys);

					// Now find a list of all the groups people in this company have.
					List<Guid> groupIds = new List<Guid>();


					Dictionary<Guid, BillingPermissionsGroupsMemberships> allCompanyMemberships =
						BillingPermissionsGroupsMemberships.ForBillingContactIds(billingConnection, billingContactIds);

					foreach (KeyValuePair<Guid, BillingPermissionsGroupsMemberships> kvp in allCompanyMemberships)
					{
						if (null != kvp.Value.GroupId && !groupIds.Contains(kvp.Value.GroupId.Value))
							groupIds.Add(kvp.Value.GroupId.Value);
					}

					// Now get the permissions for all of these contacts and groups.
					Dictionary<Guid, BillingPermissionsBool> perms = BillingPermissionsBool.ForBillingContactsOrGroups(billingConnection, billingContactIds, groupIds);

					response.BillingPermissionsBool.AddRange(perms.Values);
				}
				else if (permSelf)
				{
					


					// Now find a list of all the groups this person has.
					List<Guid> groupIds = new List<Guid>();


					Dictionary<Guid, BillingPermissionsGroupsMemberships> memberships = BillingPermissionsGroupsMemberships.ForBillingContactId(
						billingConnection, billingContact.Uuid);

					foreach (KeyValuePair<Guid, BillingPermissionsGroupsMemberships> kvp in memberships)
					{
						if (null != kvp.Value.GroupId && !groupIds.Contains(kvp.Value.GroupId.Value))
							groupIds.Add(kvp.Value.GroupId.Value);
					}

					// First all the permissions for this contact.
					if (null != billingContact.Uuid)
					{
						Dictionary<Guid, BillingPermissionsBool> perms = BillingPermissionsBool.ForBillingContactsOrGroups(
						billingConnection,
						new List<Guid> { billingContact.Uuid.Value },
						groupIds);

						response.BillingPermissionsBool.AddRange(perms.Values);
					}
					
				}
				else
				{
					// no permissions
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}





			}
			while (false);


			if (billingConnection != null)
			{
				billingConnection.Dispose();
				billingConnection = null;
			}

			if (dpDBConnection != null)
			{
				dpDBConnection.Dispose();
				dpDBConnection = null;
			}


			if (null != billingContact)
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingPermissionsBoolForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingPermissionsBoolForCurrentSessionCB", response).ConfigureAwait(false);
			}


		}
	}
}
