using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformBillingPermissionsGroupsMembershipsRemoveParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Guid? BillingContactId { get; set; }
			public List<Guid> PermissionsGroupIds { get; set; } = new List<Guid>();
		}

		public class PerformBillingPermissionsGroupsMembershipsRemoveResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> Removed { get; set; } = new List<Guid>();
		}

		public async Task PerformBillingPermissionsGroupsMembershipsRemove(PerformBillingPermissionsGroupsMembershipsRemoveParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformBillingPermissionsGroupsMembershipsRemoveResponse response = new PerformBillingPermissionsGroupsMembershipsRemoveResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			//RequestProjectStatusResponse othersMsg = new RequestProjectStatusResponse
			//{
			//	IdempotencyToken = Guid.NewGuid().ToString(),
			//};



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
				//othersMsg.RoundTripRequestId = p.RoundTripRequestId;

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

				if (!permissions.Contains(EnvDatabases.kPermBillingPermissionsGroupsMembershipsDeleteAny) &&
					!permissions.Contains(EnvDatabases.kPermBillingPermissionsGroupsMembershipsDeleteCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				if (null == p.BillingContactId)
				{
					response.IsError = true;
					response.ErrorMessage = "null == p.BillingContactId";
					break;
				}

				// Do action

				Dictionary<Guid, BillingPermissionsGroupsMemberships> entries = 
					BillingPermissionsGroupsMemberships.ForBillingContactIdAndGroupId(billingConnection, p.BillingContactId.Value, p.PermissionsGroupIds);




				response.Removed = BillingPermissionsGroupsMemberships.Delete(billingConnection, entries.Keys.ToList());







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
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformBillingPermissionsGroupsMembershipsRemoveCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("PerformBillingPermissionsGroupsMembershipsRemoveCB", response).ConfigureAwait(false);
			}
		}
	}
}
