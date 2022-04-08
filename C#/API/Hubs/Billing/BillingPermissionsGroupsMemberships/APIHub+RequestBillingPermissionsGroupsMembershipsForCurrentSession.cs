using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestBillingPermissionsGroupsMembershipsParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
		}
		public class RequestBillingPermissionsGroupsMembershipsResponse : IdempotencyResponse
		{
			public List<BillingPermissionsGroupsMemberships> BillingPermissionsGroupsMemberships { get; } = new List<BillingPermissionsGroupsMemberships> { };
		}

		public async Task RequestBillingPermissionsGroupsMembershipsForCurrentSession(RequestBillingPermissionsGroupsMembershipsParams p)
		{
			RequestBillingPermissionsGroupsMembershipsResponse response = new RequestBillingPermissionsGroupsMembershipsResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			BillingContacts? billingContact = null;
			NpgsqlConnection? dpDBConnection = null;

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

				if (billingContact.CompanyId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "billingContact.CompanyId == null";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				bool permAny = permissions.Contains(Databases.Konstants.kPermBillingPermissionsGroupsMembershipsReadAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermBillingPermissionsGroupsMembershipsReadCompany);
				bool permSelf = true; //permissions.Contains(Databases.Konstants.kPermBillingPermissionsBoolReadSelf);


				if (permAny || permCompany)
				{
					// return everyone in company.

					List<Guid> billingContactIds = new List<Guid>();

					// First get a list of all the contacts in the company.

					Dictionary<Guid, BillingContacts> billingContacts = BillingContacts.ForCompany(billingConnection, billingContact.CompanyId.Value);

					billingContactIds.AddRange(billingContacts.Keys);

					// Now get the permissions for this company.

					Dictionary<Guid, BillingPermissionsGroupsMemberships> memberships = BillingPermissionsGroupsMemberships.ForBillingContactIds(
						billingConnection,
						billingContactIds
						);

					response.BillingPermissionsGroupsMemberships.AddRange(memberships.Values);
				}
				else if (permSelf)
				{
					
					if (null != billingContact.Uuid)
					{
						Dictionary<Guid, BillingPermissionsGroupsMemberships> memberships = BillingPermissionsGroupsMemberships.ForBillingContactId(
							billingConnection,
							billingContact.Uuid.Value
							);

						response.BillingPermissionsGroupsMemberships.AddRange(memberships.Values);
					}
					
				}
				else
				{
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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingPermissionsGroupsMembershipsForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingPermissionsGroupsMembershipsForCurrentSessionCB", response).ConfigureAwait(false);
			}

		}
	}
}
