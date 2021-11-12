using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Databases.Records.Billing;
using Databases.Records.CRM;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushVoicemailsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, Voicemails> Voicemails { get; set; } = new Dictionary<Guid, Voicemails>();
		}
		public class PushVoicemailsResponse : IdempotencyResponse
		{
			public List<Guid> Voicemails { get; set; } = new List<Guid>();
		}
		public async Task PushVoicemails(PushVoicemailsParams p)
		{
			PushVoicemailsResponse response = new PushVoicemailsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestVoicemailsResponse othersMsg = new RequestVoicemailsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Voicemails> toSendToOthers = new Dictionary<Guid, Voicemails>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Voicemails == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Voicemails == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushVoicemailsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushVoicemailsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				
				Voicemails.Upsert(
					dpDBConnection,
					p.Voicemails,
					out callerResponse,
					out toSendToOthers
					);


				response.Voicemails = callerResponse;
				othersMsg.Voicemails = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushVoicemailsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestVoicemailsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestVoicemailsCB", othersMsg).ConfigureAwait(false);
			}







		}
	}
}