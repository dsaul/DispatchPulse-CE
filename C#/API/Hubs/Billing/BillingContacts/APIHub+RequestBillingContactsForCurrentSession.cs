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
		public class RequestBillingContactsParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
		}

		public class RequestBillingContactsResponse : IdempotencyResponse
		{
			public List<BillingContacts> BillingContacts { get; } = new List<BillingContacts> { };
		}

		public async Task RequestBillingContactsForCurrentSession(RequestBillingContactsParams p)
		{
			RequestBillingContactsResponse response = new RequestBillingContactsResponse()
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

				if (billingContact.CompanyId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No company id on billing contact.";
					break;
				}



				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (permissions.Contains(Databases.Konstants.kPermBillingContactsReadAny) ||
					permissions.Contains(Databases.Konstants.kPermBillingContactsReadCompany)
					)
				{
					// return everyone in company.
					Dictionary<Guid, BillingContacts> results = BillingContacts.ForCompany(billingConnection, billingContact.CompanyId.Value);

					response.BillingContacts.AddRange(results.Values);
				}
				else if (permissions.Contains(Databases.Konstants.kPermBillingContactsReadSelf))
				{
					// return just billing contact

					response.BillingContacts.Add(billingContact);
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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingContactsForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingContactsForCurrentSessionCB", response).ConfigureAwait(false);
			}




		}

		


		
	}
}
