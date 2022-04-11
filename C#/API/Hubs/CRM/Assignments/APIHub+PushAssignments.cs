using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;


namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushAssignmentsParams : IdempotencyRequest
		{
			public Dictionary<Guid, Assignments> Assignments { get; set; } = new Dictionary<Guid, Assignments>();
		}
		public class PushAssignmentsResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> Assignments { get; set; } = new List<Guid>();
		}

		public async Task PushAssignments(PushAssignmentsParams p)
		{
			PushAssignmentsResponse response = new PushAssignmentsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestAssignmentsResponse othersMsg = new RequestAssignmentsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Assignments> toSendToOthers = new Dictionary<Guid, Assignments>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Assignments == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Assignments == null";
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

				if (!permissions.Contains(EnvDatabases.kPermCRMPushAssignmentsAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMPushAssignmentsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				Assignments.Upsert(
					dpDBConnection,
					p.Assignments,
					out callerResponse,
					out toSendToOthers
					);


				response.Assignments = callerResponse;
				othersMsg.Assignments = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushAssignmentsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestAssignmentsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestAssignmentsCB", othersMsg).ConfigureAwait(false);
			}











		}
	}
}