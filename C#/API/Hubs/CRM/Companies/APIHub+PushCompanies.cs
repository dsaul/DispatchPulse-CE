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
		public class PushCompaniesParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, Companies> Companies { get; set; } = new Dictionary<Guid, Companies>();
		}

		public class PushCompaniesResponse : IdempotencyResponse
		{
			public List<Guid> Companies { get; set; } = new List<Guid>();
		}

		public async Task PushCompanies(PushCompaniesParams p)
		{
			PushCompaniesResponse response = new PushCompaniesResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestCompaniesResponse othersMsg = new RequestCompaniesResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Companies> toSendToOthers = new Dictionary<Guid, Companies>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Companies == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Companies == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushCompaniesAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushCompaniesCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				Companies.Upsert(
					dpDBConnection,
					p.Companies,
					out callerResponse,
					out toSendToOthers
					);


				response.Companies = callerResponse;
				othersMsg.Companies = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushCompaniesCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestCompaniesCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestCompaniesCB", othersMsg).ConfigureAwait(false);
			}


















		}
	}
}