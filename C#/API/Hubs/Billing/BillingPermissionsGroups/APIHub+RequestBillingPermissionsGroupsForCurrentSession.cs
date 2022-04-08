using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestBillingPermissionsGroupsParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
		}
		public class RequestBillingPermissionsGroupsResponse : IdempotencyResponse
		{
			public List<BillingPermissionsGroups> BillingPermissionsGroups { get; } = new List<BillingPermissionsGroups> { };
		}

		public async Task RequestBillingPermissionsGroupsForCurrentSession(RequestBillingPermissionsBoolParams p)
		{
			RequestBillingPermissionsGroupsResponse response = new RequestBillingPermissionsGroupsResponse()
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

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				bool permAll = permissions.Contains(Databases.Konstants.kPermBillingPermissionsGroupsReadAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermBillingPermissionsGroupsReadCompany);
				bool permSelf = true;

				if (permAll || permCompany || permSelf)
				{
					response.BillingPermissionsGroups.AddRange(BillingPermissionsGroups.AllMinusHidden(billingConnection).Values);
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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingPermissionsGroupsForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingPermissionsGroupsForCurrentSessionCB", response).ConfigureAwait(false);
			}
		}
	}
}
