using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Databases.Records.Billing;
using Newtonsoft.Json.Linq;
using Databases.Records.CRM;
using System.Net.Http;
using System.Net;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformRetrieveCalendarParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Guid? CalendarId { get; set; }
		}

		public class PerformRetrieveCalendarResponse : IdempotencyResponse
		{
			public bool? Complete { get; set; } = null;
		}

		public async Task PerformRetrieveCalendar(PerformRetrieveCalendarParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformRetrieveCalendarResponse response = new PerformRetrieveCalendarResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

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

				if (p.CalendarId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No calendar id provided.";
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

				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				bool permAny = permissions.Contains(Databases.Konstants.kPermCRMPushCalendarsAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermCRMPushCalendarsCompany);

				if (!permAny && !permCompany)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				// Do action.

				await Calendars.RetrieveCalendar(dpDBConnection, response, p.CalendarId.Value);
				if (false == response.IsError)
					break;

				await RequestCalendars(new RequestCalendarsParams
				{
					IdempotencyToken = Guid.NewGuid().ToString(),
					SessionId = p.SessionId,
				});


				response.Complete = true;
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
				await Clients.Caller.SendAsync("PerformRetrieveCalendarCB", response).ConfigureAwait(false);

			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformRetrieveCalendarCB", response).ConfigureAwait(false);
			}
		}
	}
}
