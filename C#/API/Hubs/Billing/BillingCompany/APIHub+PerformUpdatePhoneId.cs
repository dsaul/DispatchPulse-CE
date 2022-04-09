using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class PerformUpdatePhoneIdParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
			public string? NewPhoneId { get; set; }
		}

		public class PerformUpdatePhoneIdResponse : PermissionsIdempotencyResponse
		{
			public bool Saved { get; set; } = false;

		}


		public async Task PerformUpdatePhoneId(PerformUpdatePhoneIdParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformUpdatePhoneIdResponse response = new PerformUpdatePhoneIdResponse();


			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			BillingContacts? billingContact = null;


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
				if (null == billingCompany || null == billingCompany.Uuid || null == billingCompany.JsonObject)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing company.";
					break;
				}


				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(EnvDatabases.kPermBillingCompaniesModifyCompanyPhoneId)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				if (p.NewPhoneId == null || string.IsNullOrWhiteSpace(p.NewPhoneId))
				{
					response.IsError = true;
					response.ErrorMessage = "Did not recieve a new phone ID.";
					break;
				}



				JObject json = billingCompany.JsonObject;
				if (null == json)
					json = new JObject();

				json[BillingCompanies.kJsonKeyPhoneId] = p.NewPhoneId;

				string newJSON = json.ToString();


				string sql = @"
					UPDATE ""billing-companies"" 
					SET
						""json"" = @json ::json
					WHERE 
						""uuid"" = @uuid
					; ";


				using NpgsqlCommand cmd = new NpgsqlCommand(sql, billingConnection);
				cmd.Parameters.AddWithValue("@json", newJSON);
				cmd.Parameters.AddWithValue("@uuid", billingCompany.Uuid);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Update had no effect.";
					break;
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

			if (null == billingContact)
			{
				await Clients.Caller.SendAsync("PerformUpdatePhoneIdCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformUpdatePhoneIdCB", response).ConfigureAwait(false);
			}

			
			if (null != p)
			{
				await RequestBillingCompanyForCurrentSession(new RequestBillingCompanyForSessionParams
				{
					SessionId = p.SessionId,
				}).ConfigureAwait(false);
			}
			




		}
	}
}
