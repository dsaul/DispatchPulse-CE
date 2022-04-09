﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushBillingSessionEMailListMarketingParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public bool? EMailListMarketing { get; set; } = null;
		}
		public class PushBillingSessionEMailListMarketingResponse : PermissionsIdempotencyResponse
		{
			public bool Saved { get; set; } = false;
		}
		public async Task PushBillingSessionEMailListMarketing(PushBillingSessionEMailListMarketingParams p)
		{
			if (p == null)
				return;

			PushBillingSessionEMailListMarketingResponse response = new PushBillingSessionEMailListMarketingResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
				RoundTripRequestId = p.RoundTripRequestId,
			};

			do
			{
				if (p.SessionId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No sessionId provided.";
					break;
				}

				if (p.EMailListMarketing == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No eMailListMarketing provided.";
					break;
				}

				string connectionString = EnvDatabases.DatabaseConnectionStringForDB(EnvDatabases.BILLING_DATABASE_NAME);
				using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
				connection.Open();


				Dictionary<Guid, BillingSessions> sessions = BillingSessions.ForId(connection, p.SessionId.Value);
				if (sessions.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = $"Session not found. #1 {p.SessionId}";
					break;
				}

				BillingSessions session = sessions.First().Value;
				if (session == null)
				{
					response.IsError = true;
					response.ErrorMessage = $"Session not found. #2 {p.SessionId}";
					break;
				}

				if (null == session.ContactId)
				{
					response.IsError = true;
					response.ErrorMessage = $"Session has no contact id {p.SessionId}";
					break;
				}

				Guid contactId = session.ContactId.Value;

				// Register signal r connection for billing contact id
				await Groups.AddToGroupAsync(Context.ConnectionId, BillingContacts.GroupNameForContactId(contactId)).ConfigureAwait(false);

				// Update EMailListMarketing
				string sql = @"
					UPDATE ""billing-contacts"" 
					SET
						""email-list-marketing"" = @eMailListMarketing
					WHERE 
						""uuid"" = @uuid
					; ";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@eMailListMarketing", p.EMailListMarketing);
				cmd.Parameters.AddWithValue("@uuid", contactId);
				int rowsAffected = cmd.ExecuteNonQuery();
				//using NpgsqlDataReader reader = cmd.ExecuteReader();

				if (rowsAffected == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Update had no effect.";
					break;
				}

				response.Saved = true;


				// Get Updated Contact
				Dictionary<Guid, BillingContacts> billingContacts = BillingContacts.ForId(connection, contactId);
				if (billingContacts.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Billing contact not found. #1";
					break;
				}

				BillingContacts billingContact = billingContacts.First().Value;
				if (billingContact == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Billing contact not found. #2";
					break;
				}


				if (null != billingContact.CompanyId)
				{
					// Register company id for signalr
					string notifyGroupName = BillingCompanies.GroupNameForCompanyId(billingContact.CompanyId.Value);
					await Groups.AddToGroupAsync(Context.ConnectionId, notifyGroupName).ConfigureAwait(false);

					// Send to everyone.
					RequestBillingContactsResponse notifyOthers = new RequestBillingContactsResponse
					{
						IdempotencyToken = Guid.NewGuid().ToString(),
						RoundTripRequestId = Guid.NewGuid().ToString(),
					};
					notifyOthers.BillingContacts.Add(billingContact);

					await Clients.Group(notifyGroupName)
						.SendAsync("RequestBillingContactsForCurrentSessionCB", notifyOthers).ConfigureAwait(false);
				}
				

			} while (false);

			await Clients.Caller.SendAsync(
				"PushBillingSessionEMailListMarketingCB", response).ConfigureAwait(false);
		}
	}
}
