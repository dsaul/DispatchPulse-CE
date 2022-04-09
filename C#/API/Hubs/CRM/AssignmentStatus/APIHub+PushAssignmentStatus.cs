using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;


namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushAssignmentStatusParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, AssignmentStatus> AssignmentStatus { get; set; } = new Dictionary<Guid, AssignmentStatus>();
		}
		public class PushAssignmentStatusResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> AssignmentStatus { get; set; } = new List<Guid>();
		}

		public async Task PushAssignmentStatus(PushAssignmentStatusParams p)
		{

			PushAssignmentStatusResponse response = new PushAssignmentStatusResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestAssignmentStatusResponse othersMsg = new RequestAssignmentStatusResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, AssignmentStatus> toSendToOthers = new Dictionary<Guid, AssignmentStatus>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.AssignmentStatus == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.AssignmentStatus == null";
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

				if (!permissions.Contains(EnvDatabases.kPermCRMPushAssignmentsStatusAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMPushAssignmentsStatusCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				AssignmentStatus.Upsert(
					dpDBConnection,
					p.AssignmentStatus,
					out callerResponse,
					out toSendToOthers
					);


				response.AssignmentStatus = callerResponse;
				othersMsg.AssignmentStatus = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushAssignmentStatusCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestAssignmentStatusCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestAssignmentStatusCB", othersMsg).ConfigureAwait(false);
			}
























		}
	}
}