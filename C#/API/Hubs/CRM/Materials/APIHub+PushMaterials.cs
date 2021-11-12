using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Databases.Records.CRM;
using Databases.Records.Billing;
using API.Utility;
using SharedCode.Extensions;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushMaterialsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, Materials> Materials { get; set; } = new Dictionary<Guid, Materials>();
		}
		public class PushMaterialsResponse : IdempotencyResponse
		{
			public List<Guid> Materials { get; } = new List<Guid>();
		}

		public async Task PushMaterials(PushMaterialsParams p)
		{
			PushMaterialsResponse response = new PushMaterialsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestMaterialsResponse othersMsg = new RequestMaterialsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Materials> toSendToOthers = new Dictionary<Guid, Materials>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Materials == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Materials == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushMaterialsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushMaterialsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				Materials.Upsert(
					dpDBConnection,
					p.Materials,
					out callerResponse,
					out toSendToOthers
					);


				response.Materials.AddRange(callerResponse);
				othersMsg.Materials.AddRange(toSendToOthers);

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


			await Clients.Caller.SendAsync("PushMaterialsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestMaterialsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestMaterialsCB", othersMsg).ConfigureAwait(false);
			}








		}
	}
}