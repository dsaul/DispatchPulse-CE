using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushProjectsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, Projects> Projects { get; set; } = new Dictionary<Guid, Projects>();
		}
		public class PushProjectsResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> Projects { get; set; } = new List<Guid>();
		}

		public async Task PushProjects(PushProjectsParams p)
		{
			PushProjectsResponse response = new PushProjectsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestProjectsResponse othersMsg = new RequestProjectsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Projects> toSendToOthers = new Dictionary<Guid, Projects>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Projects == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Projects == null";
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

				if (!permissions.Contains(EnvDatabases.kPermCRMPushProjectsAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMPushProjectsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				Projects.Upsert(
					dpDBConnection,
					p.Projects,
					out callerResponse,
					out toSendToOthers
					);


				response.Projects = callerResponse;
				othersMsg.Projects = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushProjectsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestProjectsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestProjectsCB", othersMsg).ConfigureAwait(false);
			}







		}
	}
}