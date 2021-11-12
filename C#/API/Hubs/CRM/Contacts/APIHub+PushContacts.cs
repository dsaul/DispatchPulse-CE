using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Databases.Records.CRM;
using Databases.Records.Billing;


namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushContactsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, Contacts> Contacts { get; set; } = new Dictionary<Guid, Contacts>();
		}

		public class PushContactsResponse : IdempotencyResponse
		{
			public List<Guid> Contacts { get; set; } = new List<Guid>();
		}

		public async Task PushContacts(PushContactsParams p)
		{
			PushContactsResponse response = new PushContactsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestContactsResponse othersMsg = new RequestContactsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Contacts> toSendToOthers = new Dictionary<Guid, Contacts>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Contacts == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Contacts == null";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;
				othersMsg.RoundTripRequestId = p.RoundTripRequestId;

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


				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushContactsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushContactsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				Contacts.Upsert(
					dpDBConnection,
					p.Contacts,
					out callerResponse,
					out toSendToOthers
					);


				response.Contacts = callerResponse;
				othersMsg.Contacts = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushContactsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestContactsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestContactsCB", othersMsg).ConfigureAwait(false);
			}



















		}
	}
}