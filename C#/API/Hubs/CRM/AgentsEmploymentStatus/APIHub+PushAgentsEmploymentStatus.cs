using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushAgentsEmploymentStatusParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, AgentsEmploymentStatus> AgentsEmploymentStatus { get; set; } = new Dictionary<Guid, AgentsEmploymentStatus>();
		}

		public class PushAgentsEmploymentStatusResponse : IdempotencyResponse
		{
			public List<Guid> AgentsEmploymentStatus { get; set; } = new List<Guid>();
		}

		public async Task PushAgentsEmploymentStatus(PushAgentsEmploymentStatusParams p)
		{
			PushAgentsEmploymentStatusResponse response = new PushAgentsEmploymentStatusResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestAgentsEmploymentStatusResponse othersMsg = new RequestAgentsEmploymentStatusResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, AgentsEmploymentStatus> toSendToOthers = new Dictionary<Guid, AgentsEmploymentStatus>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.AgentsEmploymentStatus == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.AgentsEmploymentStatus == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushEmploymentStatusAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushEmploymentStatusCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				AgentsEmploymentStatus.Upsert(
					dpDBConnection,
					p.AgentsEmploymentStatus,
					out callerResponse,
					out toSendToOthers
					);


				response.AgentsEmploymentStatus = callerResponse;
				othersMsg.AgentsEmploymentStatus = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushAgentsEmploymentStatusCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestAgentsEmploymentStatusCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestAgentsEmploymentStatusCB", othersMsg).ConfigureAwait(false);
			}









		}
	}
}