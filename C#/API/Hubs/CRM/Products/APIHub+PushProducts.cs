using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Databases.Records.CRM;
using Databases.Records.Billing;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushProductsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, Products> Products { get; set; } = new Dictionary<Guid, Products>();
		}
		public class PushProductsResponse : IdempotencyResponse
		{
			public List<Guid> Products { get; set; } = new List<Guid>();
		}

		public async Task PushProducts(PushProductsParams p)
		{
			PushProductsResponse response = new PushProductsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestProductsResponse othersMsg = new RequestProductsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Products> toSendToOthers = new Dictionary<Guid, Products>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Products == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Products == null";
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMPushProductsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMPushProductsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				Products.Upsert(
					dpDBConnection,
					p.Products,
					out callerResponse,
					out toSendToOthers
					);


				response.Products = callerResponse;
				othersMsg.Products = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushProductsCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestProductsCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestProductsCB", othersMsg).ConfigureAwait(false);
			}








		}
	}
}