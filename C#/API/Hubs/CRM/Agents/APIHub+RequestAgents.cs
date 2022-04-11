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
		public class RequestAgentsParams: IdempotencyRequest
		{
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
		}
		public class RequestAgentsResponse : PermissionsIdempotencyResponse
		{
			
			public Dictionary<Guid, Agents> Agents { get; set; } = new Dictionary<Guid, Agents>();
		}

		public async Task RequestAgents(RequestAgentsParams p)
		{
			RequestAgentsResponse response = new RequestAgentsResponse
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

				bool permAny = permissions.Contains(EnvDatabases.kPermCRMRequestAgentsAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermCRMRequestAgentsCompany);
				bool permSelf = permissions.Contains(EnvDatabases.kPermCRMRequestAgentsSelf);

				if (permAny || permCompany)
				{
					if (p.LimitToIds == null || p.LimitToIds.Count == 0)
					{
						response.Agents = Agents.All(dpDBConnection);
					}
					else
					{
						response.Agents = Agents.ForIds(dpDBConnection, p.LimitToIds);
					}
				}
				else if (permSelf)
				{
					if (null == billingContact.DPAgentId)
					{
						response.IsError = true;
						response.ErrorMessage = "No agent id in billing contact.";
						response.IsPermissionsError = true;
						break;
					}
					
					// Force the this perm to only get their own agent.
					response.Agents = Agents.ForIds(dpDBConnection, new List<Guid> { billingContact.DPAgentId.Value });
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

			await Clients.Caller.SendAsync("RequestAgentsCB", response).ConfigureAwait(false);

		}
	}
}