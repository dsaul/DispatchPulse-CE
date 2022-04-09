using System;
using System.Collections.Generic;
using System.Linq;
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
		public class RequestGroupViewDashboardParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
		}
		public class RequestGroupViewDashboardResponse : PermissionsIdempotencyResponse
		{
			public Dictionary<Guid, Agents> Agents { get; set; } = new Dictionary<Guid, Agents>();
			public Dictionary<Guid, Assignments> Assignments { get; set; } = new Dictionary<Guid, Assignments>();
			public Dictionary<Guid, Labour> Labour { get; set; } = new Dictionary<Guid, Labour>();
			public Dictionary<Guid, Projects> Projects { get; set; } = new Dictionary<Guid, Projects>();
		}

		public async Task RequestGroupViewDashboard(RequestGroupViewDashboardParams p)
		{


			RequestGroupViewDashboardResponse response = new RequestGroupViewDashboardResponse
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

				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

				response.Agents = Agents.All(dpDBConnection);
				response.Assignments= Assignments.All(dpDBConnection);
				response.Labour = Labour.All(dpDBConnection);
				response.Projects = Projects.All(dpDBConnection);

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

			await Clients.Caller.SendAsync("RequestGroupViewDashboardCB", response).ConfigureAwait(false);
		}
	}
}
