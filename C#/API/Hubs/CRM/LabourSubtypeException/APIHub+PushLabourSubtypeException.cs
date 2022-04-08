using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushLabourSubtypeExceptionParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, LabourSubtypeException> LabourSubtypeException { get; set; } = new Dictionary<Guid, LabourSubtypeException>();
		}
		public class PushLabourSubtypeExceptionResponse : IdempotencyResponse
		{
			public List<Guid> LabourSubtypeException { get; set; } = new List<Guid>();
		}

		public async Task PushLabourSubtypeException(PushLabourSubtypeExceptionParams p)
		{
			PushLabourSubtypeExceptionResponse response = new PushLabourSubtypeExceptionResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestLabourSubtypeExceptionResponse othersMsg = new RequestLabourSubtypeExceptionResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, LabourSubtypeException> toSendToOthers = new Dictionary<Guid, LabourSubtypeException>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.LabourSubtypeException == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.LabourSubtypeException == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushLabourSubtypeExceptionAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushLabourSubtypeExceptionCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				LabourSubtypeException.Upsert(
					dpDBConnection,
					p.LabourSubtypeException,
					out callerResponse,
					out toSendToOthers
					);


				response.LabourSubtypeException = callerResponse;
				othersMsg.LabourSubtypeException = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushLabourSubtypeExceptionCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestLabourSubtypeExceptionCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestLabourSubtypeExceptionCB", othersMsg).ConfigureAwait(false);
			}



























		}
	}
}