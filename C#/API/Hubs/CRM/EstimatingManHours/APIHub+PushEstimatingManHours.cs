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
		public class PushEstimatingManHoursParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, EstimatingManHours> EstimatingManHours { get; set; } = new Dictionary<Guid, EstimatingManHours>();
		}

		public class PushEstimatingManHoursResponse : IdempotencyResponse
		{
			public List<Guid> EstimatingManHours { get; set; } = new List<Guid>();
		}

		public async Task PushEstimatingManHours(PushEstimatingManHoursParams p)
		{
			PushEstimatingManHoursResponse response = new PushEstimatingManHoursResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestEstimatingManHoursResponse othersMsg = new RequestEstimatingManHoursResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, EstimatingManHours> toSendToOthers = new Dictionary<Guid, EstimatingManHours>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.EstimatingManHours == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.EstimatingManHours == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushEstimatingManHoursAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushEstimatingManHoursCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				EstimatingManHours.Upsert(
					dpDBConnection,
					p.EstimatingManHours,
					out callerResponse,
					out toSendToOthers
					);


				response.EstimatingManHours = callerResponse;
				othersMsg.EstimatingManHours = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushEstimatingManHoursCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestEstimatingManHoursCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestEstimatingManHoursCB", othersMsg).ConfigureAwait(false);
			}






















		}
	}
}