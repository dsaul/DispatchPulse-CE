using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformDeleteBillingContactParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }

			public List<Guid> BillingContacts { get; set; } = new List<Guid>();
		}



		public class PerformDeleteBillingContactResponse : PermissionsIdempotencyResponse
		{

			public List<Guid> BillingContactsDelete { get; } = new List<Guid>();


		}

		public async Task PerformDeleteBillingContact(PerformDeleteBillingContactParams p)
		{
			PerformDeleteBillingContactResponse response = new PerformDeleteBillingContactResponse
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
				if (p.BillingContacts == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.BillingContacts == null";
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

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(EnvDatabases.kPermBillingContactsDeleteAny) &&
					!permissions.Contains(EnvDatabases.kPermBillingContactsDeleteCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				List<Guid> affected = BillingContacts.Delete(
					billingConnection,
					p.BillingContacts
					);

				response.BillingContactsDelete.AddRange(affected);
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
				await Clients.Caller.SendAsync("PerformDeleteBillingContactCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("PerformDeleteBillingContactCB", response).ConfigureAwait(false);
			}




		}

	}
}
