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
		public class RequestLabourTypesParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
		}
		public class RequestLabourTypesResponse : IdempotencyResponse
		{

			public Dictionary<Guid, LabourTypes> LabourTypes { get; set; } = new Dictionary<Guid, LabourTypes>();
		}

		public async Task RequestLabourTypes(RequestLabourTypesParams p)
		{
			RequestLabourTypesResponse response = new RequestLabourTypesResponse
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMRequestLabourTypesAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMRequestLabourTypesCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				if (p.LimitToIds == null || p.LimitToIds.Count == 0)
				{
					response.LabourTypes = LabourTypes.All(dpDBConnection);
				}
				else
				{
					response.LabourTypes = LabourTypes.ForIds(dpDBConnection, p.LimitToIds);
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

			await Clients.Caller.SendAsync("RequestLabourTypesCB", response).ConfigureAwait(false);
		}
	}
}