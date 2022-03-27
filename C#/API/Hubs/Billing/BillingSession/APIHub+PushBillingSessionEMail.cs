using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Databases.Records.Billing;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushBillingSessionEMailParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public string? EMail { get; set; }
		}
		public class PushBillingSessionEMailResponse : IdempotencyResponse
		{
			public bool? Saved { get; set; } = false;
		}
		public async Task PushBillingSessionEMail(PushBillingSessionEMailParams p)
		{
			if (p == null)
				return;

			PushBillingSessionEMailResponse response = new PushBillingSessionEMailResponse()
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

				if (string.IsNullOrWhiteSpace(p.EMail))
				{
					response.IsError = true;
					response.ErrorMessage = "No eMail provided.";
					break;
				}

				string connectionString = Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME);
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

				if (session.ContactId == null)
				{
					response.IsError = true;
					response.ErrorMessage = $"Session has no contact id {p.SessionId}";
					break;
				}

				Guid contactId = session.ContactId.Value;

				// Register signal r connection for billing contact id
				await Groups.AddToGroupAsync(Context.ConnectionId, BillingContacts.GroupNameForContactId(contactId)).ConfigureAwait(false);

				// Update EMail
				string sql = @"
					UPDATE ""billing-contacts"" 
					SET
						""email"" = @email
					WHERE 
						""uuid"" = @uuid
					; ";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@email", p.EMail);
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

				if (billingContact.CompanyId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Billing contact has no company id.";
					break;
				}


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

			} while (false);

			await Clients.Caller.SendAsync(
				"PushBillingSessionEMailCB", response).ConfigureAwait(false);
		}
	}
}
