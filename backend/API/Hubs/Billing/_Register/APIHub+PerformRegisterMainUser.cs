using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SharedCode.DatabaseSchemas;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class PerformRegisterMainUserParams : IdempotencyRequest
		{
			public Guid? ContactId { get; set; }
			public string? FullName { get; set; }
			public string? PasswordHash { get; set; }
			public bool? EMailMarketing { get; set; }
			public bool? EMailTutorials { get; set; }

		}

		public class PerformRegisterMainUserResponse : PermissionsIdempotencyResponse
		{
			public bool? Saved { get; set; }

		}

		public async Task PerformRegisterMainUser(PerformRegisterMainUserParams p)
		{
			PerformRegisterMainUserResponse response = new PerformRegisterMainUserResponse()
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
					out dpDBConnection,
					true
					);

				if (null != response.IsError && response.IsError.Value)
					break;

				// Make sure that the contact exists.
				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Register Main User: Invalid contact.";
					break;
				}

				if (string.IsNullOrWhiteSpace(p.FullName))
				{
					response.IsError = true;
					response.ErrorMessage = "No name provided.";
					break;
				}

				if (string.IsNullOrWhiteSpace(p.PasswordHash))
				{
					response.IsError = true;
					response.ErrorMessage = "No password provided.";
					break;
				}

				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to billing database.";
					break;
				}

				if (null == session)
				{
					response.IsError = true;
					response.ErrorMessage = "Can't get session #1.";
					break;
				}

				if (null == session.ContactId)
				{
					response.IsError = true;
					response.ErrorMessage = "Can't get session #2.";
					break;
				}


				// Make sure that the session and the contact belong to the same contact.

				BillingContacts contactResults = BillingContacts.ForId(billingConnection, session.ContactId.Value).First().Value;

				if (contactResults.CompanyId != billingContact.CompanyId)
				{
					response.IsError = true;
					response.ErrorMessage = "Trying to modify different company!";
					break;
				}

				// We only allow this method to do anything if the existing password is null or whitespace (new user).
				if (!string.IsNullOrWhiteSpace(billingContact.PasswordHash))
				{
					response.IsError = true;
					response.ErrorMessage = "Register Main User: This shoudn't be called on existing users.";
					break;
				}

				if (null != billingContact.Uuid)
				{
					BillingContacts mod = billingContact with
					{
						PasswordHash = p.PasswordHash,
						EmailListMarketing = p.EMailMarketing,
						EmailListTutorials = p.EMailTutorials,
						FullName = p.FullName
					};
					BillingContacts.Upsert(billingConnection, new Dictionary<Guid, BillingContacts>
					{
						{ mod.Uuid.Value, mod }
					}, out _, out _);
				}
				

				response.Saved = true;
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








			await Clients.Caller.SendAsync("PerformRegisterMainUserCB", response).ConfigureAwait(false);
		}
	}
}
