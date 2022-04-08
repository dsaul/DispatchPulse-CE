using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Databases.Records.CRM;
using Databases.Records.Billing;
using System.Linq;
using SharedCode;
using SharedCode.Databases.Records.CRM;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class RequestRecordingsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
		}
		public class RequestRecordingsResponse : IdempotencyResponse 
		{

			public Dictionary<Guid, Recordings> Recordings { get; } = new Dictionary<Guid, Recordings>();
		}

		public async Task RequestRecordings(RequestRecordingsParams p)
		{

			RequestRecordingsResponse response = new RequestRecordingsResponse
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

				bool permAny = permissions.Contains(Databases.Konstants.kPermCRMRequestRecordingsAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermCRMRequestRecordingsCompany);


				if (permAny || permCompany)
				{
					if (p.LimitToIds == null || p.LimitToIds.Count == 0)
					{
						response.Recordings.AddRange(Recordings.All(dpDBConnection));
					}
					else
					{
						response.Recordings.AddRange(Recordings.ForIds(dpDBConnection, p.LimitToIds));
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

			await Clients.Caller.SendAsync("RequestRecordingsCB", response).ConfigureAwait(false);

		}
	}
}