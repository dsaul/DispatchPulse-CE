using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestVoicemailsParams: IdempotencyRequest
		{
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
		}
		public class RequestVoicemailsResponse : PermissionsIdempotencyResponse
		{
			
			public Dictionary<Guid, Voicemails> Voicemails { get; set; } = new Dictionary<Guid, Voicemails>();
		}

		public async Task RequestVoicemails(RequestVoicemailsParams p)
		{
			RequestVoicemailsResponse response = new RequestVoicemailsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
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
				BillingContacts? billingContact = null;
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

				bool permAny = permissions.Contains(EnvDatabases.kPermCRMRequestVoicemailsAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermCRMRequestVoicemailsCompany);

				if (permAny || permCompany)
				{
					if (p.LimitToIds == null || p.LimitToIds.Count == 0)
					{
						response.Voicemails = Voicemails.All(dpDBConnection);
					}
					else
					{
						response.Voicemails = Voicemails.ForIds(dpDBConnection, p.LimitToIds);
					}
				}
				else
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				





				



				

			} while (false);

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

			await Clients.Caller.SendAsync("RequestVoicemailsCB", response).ConfigureAwait(false);

		}
	}
}