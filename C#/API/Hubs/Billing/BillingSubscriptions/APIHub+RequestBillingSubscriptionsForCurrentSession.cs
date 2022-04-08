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
		public class RequestBillingSubscriptionsParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
		}
		public class RequestBillingSubscriptionsResponse : IdempotencyResponse
		{
			public List<BillingSubscriptions> BillingSubscriptions { get; } = new List<BillingSubscriptions> { };
		}
		public async Task RequestBillingSubscriptionsForCurrentSession(RequestBillingSubscriptionsParams p)
		{
			RequestBillingSubscriptionsResponse response = new RequestBillingSubscriptionsResponse()
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

				if (!permissions.Contains(Databases.Konstants.kPermBillingSubscriptionReadAny) &&
					!permissions.Contains(Databases.Konstants.kPermBillingSubscriptionReadCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				if (null == billingContact.CompanyId)
				{
					response.IsError = true;
					response.ErrorMessage = "No company id.";
					break;
				}

				// Get data.

				Dictionary<Guid, BillingSubscriptions> subscriptions = BillingSubscriptions.ForCompanyId(billingConnection, billingContact.CompanyId.Value);
				response.BillingSubscriptions.AddRange(subscriptions.Values);

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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingSubscriptionsForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingSubscriptionsForCurrentSessionCB", response).ConfigureAwait(false);
			}




		}
	}
}
