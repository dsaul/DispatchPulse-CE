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
		public class PushBillingSessionAgentIdParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Guid? AgentId { get; set; }
		}
		public class PushBillingSessionAgentIdResponse : IdempotencyResponse
		{
			public bool? Saved { get; set; } = false;
		}
		public async Task PushBillingSessionAgentId(PushBillingSessionAgentIdParams p)
		{
			if (p == null)
				return;

			PushBillingSessionAgentIdResponse response = new PushBillingSessionAgentIdResponse()
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

				if (p.AgentId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No agentId provided.";
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


				Guid billingContactId = session.ContactId.Value;

				// Register signal r connection for billing contact id
				await Groups.AddToGroupAsync(Context.ConnectionId, BillingContacts.GroupNameForContactId(billingContactId)).ConfigureAwait(false);

				// Get existing billing contact.
				Dictionary<Guid, BillingContacts> billingContacts = BillingContacts.ForId(connection, billingContactId);
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

				JObject? applicationData = billingContact.ApplicationDataObject;
				if (null == applicationData)
					applicationData = new JObject();

				applicationData["dispatchPulseAgentId"] = p.AgentId;

				string newJSON = applicationData.ToString();

				// Update AgentId
				string sql = @"
					UPDATE ""billing-contacts"" 
					SET
						""application-data"" = @applicationData ::json
					WHERE 
						""uuid"" = @uuid
					; ";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@applicationData", newJSON);
				cmd.Parameters.AddWithValue("@uuid", billingContactId);
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
				billingContacts = BillingContacts.ForId(connection, billingContactId);
				if (billingContacts.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Billing contact not found. #3";
					break;
				}

				billingContact = billingContacts.First().Value;
				if (billingContact == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Billing contact not found. #4";
					break;
				}


				// Register company id for signalr
				if (null != billingContact.CompanyId)
				{
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
				"PushBillingSessionAgentIdCB", response).ConfigureAwait(false);
		}
	}
}
