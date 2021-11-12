using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Databases.Records.Billing;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestBillingCompanyForSessionParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
		}

		public class RequestBillingCompanyResponse : IdempotencyResponse
		{
			public BillingCompanies? BillingCompany { get; set; }
		}


		public async Task RequestBillingCompanyForCurrentSession(RequestBillingCompanyForSessionParams p)
		{
			RequestBillingCompanyResponse response = new RequestBillingCompanyResponse()
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

				if (!permissions.Contains(Databases.Konstants.kPermBillingCompaniesReadAny) &&
					!permissions.Contains(Databases.Konstants.kPermBillingCompaniesReadCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				// Get data.
				
				if (billingContact.CompanyId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No company.";
					break;
				}


				Dictionary<Guid, BillingCompanies> results = BillingCompanies.ForIds(billingConnection, billingContact.CompanyId.Value);
				if (results.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "No company.";
					break;
				}

				response.BillingCompany = results.First().Value;

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


			if (null != billingContact)
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingCompanyForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingCompanyForCurrentSessionCB", response).ConfigureAwait(false);
			}



		}






		


























	}
}
