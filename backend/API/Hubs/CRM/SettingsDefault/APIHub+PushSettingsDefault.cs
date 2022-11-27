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
		public class PushSettingsDefaultParams : IdempotencyRequest
		{
			public Dictionary<Guid, SettingsDefault> SettingsDefault { get; set; } = new Dictionary<Guid, SettingsDefault>();
		}
		public class PushSettingsDefaultResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> SettingsDefault { get; set; } = new List<Guid>();
		}

		public async Task PushSettingsDefault(PushSettingsDefaultParams p)
		{

			PushSettingsDefaultResponse response = new PushSettingsDefaultResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestSettingsDefaultResponse othersMsg = new RequestSettingsDefaultResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, SettingsDefault> toSendToOthers = new Dictionary<Guid, SettingsDefault>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.SettingsDefault == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.SettingsDefault == null";
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

				if (!permissions.Contains(EnvDatabases.kPermCRMPushSettingsDefaultAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMPushSettingsDefaultCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				SettingsDefault.Upsert(
					dpDBConnection,
					p.SettingsDefault,
					out callerResponse,
					out toSendToOthers
					);


				response.SettingsDefault = callerResponse;
				othersMsg.SettingsDefault = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushSettingsDefaultCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestSettingsDefaultCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestSettingsDefaultCB", othersMsg).ConfigureAwait(false);
			}




		}
	}
}