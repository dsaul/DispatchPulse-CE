using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class PushSettingsUserParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, SettingsUser> SettingsUser { get; set; } = new Dictionary<Guid, SettingsUser>();
		}
		public class PushSettingsUserResponse : IdempotencyResponse
		{
			public List<Guid> SettingsUser { get; set; } = new List<Guid>();
		}

		public async Task PushSettingsUser(PushSettingsUserParams p)
		{
			PushSettingsUserResponse response = new PushSettingsUserResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestSettingsUserResponse othersMsg = new RequestSettingsUserResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, SettingsUser> toSendToOthers = new Dictionary<Guid, SettingsUser>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.SettingsUser == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.SettingsUser == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushSettingsUserAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushSettingsUserCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				SettingsUser.Upsert(
					dpDBConnection,
					p.SettingsUser,
					out callerResponse,
					out toSendToOthers
					);


				response.SettingsUser = callerResponse;
				othersMsg.SettingsUser = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushSettingsUserCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestSettingsUserCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestSettingsUserCB", othersMsg).ConfigureAwait(false);
			}





		}
	}
}