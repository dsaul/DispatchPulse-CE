using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SharedCode.DatabaseSchemas;
using SharedCode;
using Npgsql;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		
		public class PerformChangeSessionPasswordParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
			public string? CurrentPassword { get; set; }
			public string? NewHash { get; set; }
		}

		public class PerformChangeSessionPasswordResponse : PermissionsIdempotencyResponse
		{
			public bool PasswordChanged { get; set; }

		}

		public async Task PerformChangeSessionPassword(PerformChangeSessionPasswordParams p)
		{
			PerformChangeSessionPasswordResponse response = new PerformChangeSessionPasswordResponse()
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



				if (null == p.CurrentPassword)
				{
					response.IsError = true;
					response.ErrorMessage = "No password.";
					break;
				}
				if (null == p.NewHash)
				{
					response.IsError = true;
					response.ErrorMessage = "No new password hash.";
					break;
				}
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

				bool permAll = permissions.Contains(EnvDatabases.kPermBillingContactsModifyAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermBillingContactsModifyCompany);
				bool permSelf = permissions.Contains(EnvDatabases.kPermBillingContactsModifySelf);

				if (permAll || permCompany || permSelf)
				{
					if (!BCrypt.Net.BCrypt.Verify(p.CurrentPassword, billingContact.PasswordHash))
					{
						response.IsError = true;
						response.ErrorMessage = "Current password is not correct.";
						break;
					}

					if (null != p.NewHash && null != billingContact.Uuid)
					{
						BillingContacts mod = billingContact with { PasswordHash = p.NewHash };
						BillingContacts.Upsert(billingConnection, new Dictionary<Guid, BillingContacts>
						{
							{ billingContact.Uuid.Value, billingContact }
						}, out _, out _);
					}
					


					response.PasswordChanged = true;
					break;
				}
				else
				{
					response.IsError = true;
					response.IsPermissionsError = true;
					response.ErrorMessage = "You don't have permissions to change the password.";
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


			if (null != billingContact)
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformChangeSessionPasswordCB", response).ConfigureAwait(false);
			}
			
		}




	}
}
