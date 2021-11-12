using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Databases.Records.CRM;
using Databases.Records.Billing;
using Npgsql;
using API.Utility;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class LogOutSessionParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }

		}
		public class LogOutSessionResponse : IdempotencyResponse
		{
			public bool? LoggedOut { get; set; } = false;
		}

		public async Task PerformLogOutSession(LogOutSessionParams p)
		{
			LogOutSessionResponse response = new LogOutSessionResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			BillingContacts? billingContact = null;
			NpgsqlConnection? dpDBConnection = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;

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

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				bool permAny = permissions.Contains(Databases.Konstants.kPermBillingSessionsDeleteAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermBillingSessionsDeleteCompany);
				bool permSelf = permissions.Contains(Databases.Konstants.kPermBillingSessionsDeleteSelf);

				// see if the user can delete their own session
				if (permAny || permCompany || permSelf)
				{

					if (null != session && null != session.Uuid)
						BillingSessions.Delete(billingConnection, new List<Guid> { session.Uuid.Value });

					response.LoggedOut = true;
					break;
				}
				else
				{
					response.IsError = true;
					response.IsPermissionsError = true;
					response.ErrorMessage = "You don't have permissions to delete this session.";
					break;
				}










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


			if (null == billingContact)
			{
				await Clients.Caller.SendAsync("PerformLogOutSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformLogOutSessionCB", response).ConfigureAwait(false);
			}

			


		}
	}
}
