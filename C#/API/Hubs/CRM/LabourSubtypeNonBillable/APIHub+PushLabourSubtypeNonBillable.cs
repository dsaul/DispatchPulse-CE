using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Databases.Records.CRM;
using Databases.Records.Billing;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushLabourSubtypeNonBillableParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, LabourSubtypeNonBillable> LabourSubtypeNonBillable { get; set; } = new Dictionary<Guid, LabourSubtypeNonBillable>();
		}
		public class PushLabourSubtypeNonBillableResponse : IdempotencyResponse
		{
			public List<Guid> LabourSubtypeNonBillable { get; set; } = new List<Guid>();
		}

		public async Task PushLabourSubtypeNonBillable(PushLabourSubtypeNonBillableParams p)
		{
			PushLabourSubtypeNonBillableResponse response = new PushLabourSubtypeNonBillableResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestLabourSubtypeNonBillableResponse othersMsg = new RequestLabourSubtypeNonBillableResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, LabourSubtypeNonBillable> toSendToOthers = new Dictionary<Guid, LabourSubtypeNonBillable>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.LabourSubtypeNonBillable == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.LabourSubtypeNonBillable == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushLabourSubtypeNonBillableAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushLabourSubtypeNonBillableCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				LabourSubtypeNonBillable.Upsert(
					dpDBConnection,
					p.LabourSubtypeNonBillable,
					out callerResponse,
					out toSendToOthers
					);


				response.LabourSubtypeNonBillable = callerResponse;
				othersMsg.LabourSubtypeNonBillable = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushLabourSubtypeNonBillableCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestLabourSubtypeNonBillableCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestLabourSubtypeNonBillableCB", othersMsg).ConfigureAwait(false);
			}














		}
	}
}