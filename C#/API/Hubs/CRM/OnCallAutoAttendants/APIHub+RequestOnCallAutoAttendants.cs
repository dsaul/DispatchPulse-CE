﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestOnCallAutoAttendantsParams: IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
		}
		public class RequestOnCallAutoAttendantsResponse : IdempotencyResponse
		{
			
			public Dictionary<Guid, OnCallAutoAttendants> OnCallAutoAttendants { get; set; } = new Dictionary<Guid, OnCallAutoAttendants>();
		}

		public async Task RequestOnCallAutoAttendants(RequestOnCallAutoAttendantsParams p)
		{
			RequestOnCallAutoAttendantsResponse response = new RequestOnCallAutoAttendantsResponse
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

				bool permAny = permissions.Contains(Databases.Konstants.kPermCRMRequestOnCallAutoAttendantsAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermCRMRequestOnCallAutoAttendantsCompany);

				if (permAny || permCompany)
				{
					if (p.LimitToIds == null || p.LimitToIds.Count == 0)
					{
						response.OnCallAutoAttendants = OnCallAutoAttendants.All(dpDBConnection);
					}
					else
					{
						response.OnCallAutoAttendants = OnCallAutoAttendants.ForIds(dpDBConnection, p.LimitToIds);
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

			await Clients.Caller.SendAsync("RequestOnCallAutoAttendantsCB", response).ConfigureAwait(false);

		}
	}
}