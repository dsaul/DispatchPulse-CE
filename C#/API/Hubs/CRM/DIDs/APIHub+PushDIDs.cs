using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using API.Utility;
using Databases.Records.Billing;
using Databases.Records.CRM;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushDIDsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			[JsonProperty(PropertyName = "dids")]
			[DataMember(Name = "dids")]
			public Dictionary<Guid, DIDs> DIDs { get; set; } = new Dictionary<Guid, DIDs>();
		}
		public class PushDIDsResponse : IdempotencyResponse
		{
			[JsonProperty(PropertyName = "dids")]
			[DataMember(Name = "dids")]
			public List<Guid> DIDs { get; set; } = new List<Guid>();
		}
		public async Task PushDIDs(PushDIDsParams p)
		{
			PushDIDsResponse response = new PushDIDsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestDIDsResponse othersMsg = new RequestDIDsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, DIDs> toSendToOthers = new Dictionary<Guid, DIDs>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.DIDs == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.DIDs == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushDIDsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushDIDsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				
				DIDs.Upsert(
					dpDBConnection,
					p.DIDs,
					out callerResponse,
					out toSendToOthers
					);


				response.DIDs = callerResponse;
				othersMsg.DIDs = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushDIDsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestDIDsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestDIDsCB", othersMsg).ConfigureAwait(false);
			}







		}
	}
}