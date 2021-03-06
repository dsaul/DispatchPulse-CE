using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class DeleteCalendarsParams : IdempotencyRequest
		{
			public List<Guid> CalendarsDelete { get; set; } = new List<Guid>();
		}

		public class DeleteCalendarsResponse : PermissionsIdempotencyResponse
		{

			public List<Guid> CalendarsDelete { get; set; } = new List<Guid>();
		}

		public async Task DeleteCalendars(DeleteCalendarsParams p)
		{
			DeleteCalendarsResponse response = new DeleteCalendarsResponse()
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
				if (p.CalendarsDelete == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.CalendarsDelete == null";
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

				if (p.CalendarsDelete.Count == 0)
					break;

				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

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

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(EnvDatabases.kPermCRMDeleteCalendarsAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMDeleteCalendarsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// delete

				List<Guid> affected = Calendars.Delete(dpDBConnection, p.CalendarsDelete);
				if (affected.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "No rows deleted?";
					break;
				}
				
				response.CalendarsDelete = affected;
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

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("DeleteCalendarsCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("DeleteCalendarsCB", response).ConfigureAwait(false);
			}


		}
	}
}