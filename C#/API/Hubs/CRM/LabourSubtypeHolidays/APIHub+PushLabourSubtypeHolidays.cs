using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushLabourSubtypeHolidaysParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, LabourSubtypeHolidays> LabourSubtypeHolidays { get; set; } = new Dictionary<Guid, LabourSubtypeHolidays>();
		}

		public class PushLabourSubtypeHolidaysResponse : IdempotencyResponse
		{
			public List<Guid> LabourSubtypeHolidays { get; set; } = new List<Guid>();
		}

		public async Task PushLabourSubtypeHolidays(PushLabourSubtypeHolidaysParams p)
		{
			PushLabourSubtypeHolidaysResponse response = new PushLabourSubtypeHolidaysResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestLabourSubtypeHolidaysResponse othersMsg = new RequestLabourSubtypeHolidaysResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, LabourSubtypeHolidays> toSendToOthers = new Dictionary<Guid, LabourSubtypeHolidays>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.LabourSubtypeHolidays == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.LabourSubtypeHolidays == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushLabourSubtypeHolidaysAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushLabourSubtypeHolidaysCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				LabourSubtypeHolidays.Upsert(
					dpDBConnection,
					p.LabourSubtypeHolidays,
					out callerResponse,
					out toSendToOthers
					);


				response.LabourSubtypeHolidays = callerResponse;
				othersMsg.LabourSubtypeHolidays = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushLabourSubtypeHolidaysCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestLabourSubtypeHolidaysCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestLabourSubtypeHolidaysCB", othersMsg).ConfigureAwait(false);
			}




























		}
	}
}