using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Databases.Records.Billing;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformUpdateBillingContactDetailsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }

			public Dictionary<Guid, BillingContacts> BillingContacts { get; set; } = new Dictionary<Guid, BillingContacts>();
		}



		public class PerformUpdateBillingContactDetailsResponse : IdempotencyResponse
		{

			public List<Guid> BillingContacts { get; } = new List<Guid>();


		}


		public async Task PerformUpdateBillingContactDetails(PerformUpdateBillingContactDetailsParams p)
		{
			PerformUpdateBillingContactDetailsResponse response = new PerformUpdateBillingContactDetailsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestBillingContactsResponse othersMsg = new RequestBillingContactsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, BillingContacts> toSendToOthers = new Dictionary<Guid, BillingContacts>();
			BillingContacts? billingContact = null;


			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.BillingContacts == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.BillingContacts == null";
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

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(Databases.Konstants.kPermBillingContactsModifyAny) &&
					!permissions.Contains(Databases.Konstants.kPermBillingContactsModifyCompany) &&
					!permissions.Contains(Databases.Konstants.kPermBillingContactsAddAny) &&
					!permissions.Contains(Databases.Konstants.kPermBillingContactsAddCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// If it is just company, make sure that the company id matches.
				if (
					(permissions.Contains(Databases.Konstants.kPermBillingContactsModifyCompany) && !permissions.Contains(Databases.Konstants.kPermBillingContactsModifyAny))
					||
					(permissions.Contains(Databases.Konstants.kPermBillingContactsAddCompany) && !permissions.Contains(Databases.Konstants.kPermBillingContactsAddAny))
					)
				{
					bool abort = false;
					foreach (KeyValuePair<Guid, BillingContacts> kvp in p.BillingContacts)
					{
						if (kvp.Value.CompanyId != billingContact.CompanyId)
						{
							abort = true;
							break;
						}
					}
					if (abort)
					{
						response.IsError = true;
						response.ErrorMessage = "No permissions for that company.";
						response.IsPermissionsError = true;
						break;
					}


				}


				BillingContacts.Upsert(
					billingConnection,
					p.BillingContacts,
					out callerResponse,
					out toSendToOthers
					);


				response.BillingContacts.AddRange(callerResponse);
				othersMsg.BillingContacts.AddRange(toSendToOthers.Values);


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


			await Clients.Caller.SendAsync("PerformUpdateBillingContactDetailsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestBillingContactsForCurrentSessionCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingContactsForCurrentSessionCB", othersMsg).ConfigureAwait(false);
			}

























		}

	}
}
