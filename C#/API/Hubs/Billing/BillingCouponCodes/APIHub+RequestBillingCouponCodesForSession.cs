﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestBillingCouponCodesParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
		}
		public class RequestBillingCouponCodesResponse : IdempotencyResponse
		{
			public List<BillingCouponCodes> BillingCouponCodes { get; } = new List<BillingCouponCodes> { };
		}

		public async Task RequestBillingCouponCodesForCurrentSession(RequestBillingCouponCodesParams p)
		{
			RequestBillingCouponCodesResponse response = new RequestBillingCouponCodesResponse()
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

				if (!permissions.Contains(Databases.Konstants.kPermBillingCouponCodesReadAny) &&
					!permissions.Contains(Databases.Konstants.kPermBillingCouponCodesReadCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// Get data.

#warning TODO: isn't really implemented anywhere

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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingCouponCodesForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingCouponCodesForCurrentSessionCB", response).ConfigureAwait(false);
			}

		}
	}
}
