using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestBillingSessionsParams : IdempotencyRequest
		{
		}
		public class RequestBillingSessionsResponse : PermissionsIdempotencyResponse
		{
			public List<BillingSessions> BillingSessions { get; } = new List<BillingSessions> { };
		}
		public async Task RequestBillingSessionsForCurrentSession(RequestBillingSessionsParams p)
		{
			RequestBillingSessionsResponse response = new RequestBillingSessionsResponse()
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

				if (null == billingContact.CompanyId)
				{
					response.IsError = true;
					response.ErrorMessage = "No company id.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (permissions.Contains(EnvDatabases.kPermBillingSessionsReadAny) ||
					permissions.Contains(EnvDatabases.kPermBillingSessionsReadCompany))
				{
					List<Guid> billingContactIds = new List<Guid>();

					// First get a list of all the contacts in the company.

					Dictionary<Guid, BillingContacts> companyContacts = BillingContacts.ForCompany(billingConnection, billingContact.CompanyId.Value);
					billingContactIds.AddRange(companyContacts.Keys);



					// Now get the sessions for this company.
					Dictionary<Guid, BillingSessions> sessions = BillingSessions.ForContactIds(billingConnection, billingContactIds);
					response.BillingSessions.AddRange(sessions.Values);
				}
				else if (permissions.Contains(EnvDatabases.kPermBillingSessionsReadSelf))
				{
					Dictionary<Guid, BillingSessions> sessions = BillingSessions.ForContactId(billingConnection, billingContact.Uuid);
					response.BillingSessions.AddRange(sessions.Values);
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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingSessionsForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingSessionsForCurrentSessionCB", response).ConfigureAwait(false);
			}


		}
	}
}
