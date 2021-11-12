using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Databases.Records.CRM;
using Databases.Records.Billing;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestProjectStatusParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
		}
		public class RequestProjectStatusResponse : IdempotencyResponse
		{

			public Dictionary<Guid, ProjectStatus> ProjectStatus { get; set; } = new Dictionary<Guid, ProjectStatus>();
		}

		public async Task RequestProjectStatus(RequestProjectStatusParams p)
		{

			RequestProjectStatusResponse response = new RequestProjectStatusResponse
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMRequestProjectStatusAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMRequestProjectStatusCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}



				if (p.LimitToIds == null || p.LimitToIds.Count == 0)
				{
					response.ProjectStatus = ProjectStatus.All(dpDBConnection);
				}
				else
				{
					response.ProjectStatus = ProjectStatus.ForIds(dpDBConnection, p.LimitToIds);
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

			await Clients.Caller.SendAsync("RequestProjectStatusCB", response).ConfigureAwait(false);

		}
	}
}