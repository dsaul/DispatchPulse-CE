using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestLabourSubtypeHolidaysParams : IdempotencyRequest
		{
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
		}

		public class RequestLabourSubtypeHolidaysResponse : PermissionsIdempotencyResponse
		{

			public Dictionary<Guid, LabourSubtypeHolidays> LabourSubtypeHolidays { get; set; } = new Dictionary<Guid, LabourSubtypeHolidays>();
		}

		public async Task RequestLabourSubtypeHolidays(RequestLabourSubtypeHolidaysParams p)
		{

			RequestLabourSubtypeHolidaysResponse response = new RequestLabourSubtypeHolidaysResponse
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

				if (!permissions.Contains(EnvDatabases.kPermCRMRequestLabourSubtypeHolidaysAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMRequestLabourSubtypeHolidaysCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				if (p.LimitToIds == null || p.LimitToIds.Count == 0)
				{
					response.LabourSubtypeHolidays = LabourSubtypeHolidays.All(dpDBConnection);
				}
				else
				{
					response.LabourSubtypeHolidays = LabourSubtypeHolidays.ForIds(dpDBConnection, p.LimitToIds);
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

			await Clients.Caller.SendAsync("RequestLabourSubtypeHolidaysCB", response).ConfigureAwait(false);

		}
	}
}