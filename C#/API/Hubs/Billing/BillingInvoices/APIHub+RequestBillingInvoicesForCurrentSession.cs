using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Databases.Records.Billing;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestBillingInvoicesParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
		}
		public class RequestBillingInvoicesResponse : IdempotencyResponse
		{
			public List<BillingInvoices> BillingInvoices { get; } = new List<BillingInvoices> { };
		}
		public async Task RequestBillingInvoicesForCurrentSession(RequestBillingInvoicesParams p)
		{
			RequestBillingInvoicesResponse response = new RequestBillingInvoicesResponse()
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

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(Databases.Konstants.kPermBillingInvoicesReadAny) &&
					!permissions.Contains(Databases.Konstants.kPermBillingInvoicesReadCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				// Get data.
				if (null == billingContact.CompanyId)
				{
					response.IsError = true;
					response.ErrorMessage = "No company id.";
					break;
				}

				Dictionary<Guid, BillingInvoices> results = BillingInvoices.ForCompanyId(billingConnection, billingContact.CompanyId.Value);

				response.BillingInvoices.AddRange(results.Values);



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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingInvoicesForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingInvoicesForCurrentSessionCB", response).ConfigureAwait(false);
			}


		}
	}
}
