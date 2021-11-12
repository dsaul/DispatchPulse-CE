using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Databases.Records.CRM;
using Databases.Records.Billing;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using API.Utility;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushProjectStatusParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, ProjectStatus> ProjectStatus { get; set; } = new Dictionary<Guid, ProjectStatus>();
		}
		public class PushProjectStatusResponse : IdempotencyResponse
		{
			public List<Guid> ProjectStatus { get; set; } = new List<Guid>();
		}

		public async Task PushProjectStatus(PushProjectStatusParams p)
		{
			PushProjectStatusResponse response = new PushProjectStatusResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestProjectStatusResponse othersMsg = new RequestProjectStatusResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, ProjectStatus> toSendToOthers = new Dictionary<Guid, ProjectStatus>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.ProjectStatus == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.ProjectStatus == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushProjectStatusAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushProjectStatusCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				ProjectStatus.Upsert(
					dpDBConnection,
					p.ProjectStatus,
					out callerResponse,
					out toSendToOthers
					);


				response.ProjectStatus = callerResponse;
				othersMsg.ProjectStatus = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushProjectStatusCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestProjectStatusCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestProjectStatusCB", othersMsg).ConfigureAwait(false);
			}







		}
	}
}