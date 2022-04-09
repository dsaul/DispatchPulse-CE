using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushOnCallAutoAttendantsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, OnCallAutoAttendants> OnCallAutoAttendants { get; set; } = new Dictionary<Guid, OnCallAutoAttendants>();
		}
		public class PushOnCallAutoAttendantsResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> OnCallAutoAttendants { get; set; } = new List<Guid>();
		}
		public async Task PushOnCallAutoAttendants(PushOnCallAutoAttendantsParams p)
		{
			PushOnCallAutoAttendantsResponse response = new PushOnCallAutoAttendantsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestOnCallAutoAttendantsResponse othersMsg = new RequestOnCallAutoAttendantsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, OnCallAutoAttendants> toSendToOthers = new Dictionary<Guid, OnCallAutoAttendants>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.OnCallAutoAttendants == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.OnCallAutoAttendants == null";
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

				if (!permissions.Contains(EnvDatabases.kPermCRMPushOnCallAutoAttendantsAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMPushOnCallAutoAttendantsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				
				OnCallAutoAttendants.Upsert(
					dpDBConnection,
					p.OnCallAutoAttendants,
					out callerResponse,
					out toSendToOthers
					);


				response.OnCallAutoAttendants = callerResponse;
				othersMsg.OnCallAutoAttendants = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushOnCallAutoAttendantsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestOnCallAutoAttendantsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestOnCallAutoAttendantsCB", othersMsg).ConfigureAwait(false);
			}







		}
	}
}