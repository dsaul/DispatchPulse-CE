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
		public class RequestBillingJournalEntriesParams : IdempotencyRequest
		{
		}
		public class RequestBillingJournalEntriesResponse : PermissionsIdempotencyResponse
		{
			public List<BillingJournalEntries> BillingJournalEntries { get; } = new List<BillingJournalEntries> { };
		}
		public async Task RequestBillingJournalEntriesForCurrentSession(RequestBillingJournalEntriesParams p)
		{

			RequestBillingJournalEntriesResponse response = new RequestBillingJournalEntriesResponse()
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

				if (!permissions.Contains(EnvDatabases.kPermBillingJournalEntriesReadAny) &&
					!permissions.Contains(EnvDatabases.kPermBillingJournalEntriesReadCompany)
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


				Dictionary<Guid, BillingJournalEntries> results = BillingJournalEntries.ForCompanyId(billingConnection, billingContact.CompanyId.Value);

				response.BillingJournalEntries.AddRange(results.Values);


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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingJournalEntriesForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingJournalEntriesForCurrentSessionCB", response).ConfigureAwait(false);
			}

		}
	}
}
