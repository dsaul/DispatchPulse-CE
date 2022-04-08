using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformBillingPermissionsBoolRemoveParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Guid? BillingContactId { get; set; }
			public List<string> PermissionKeys { get; set; } = new List<string>();
		}

		public class PerformBillingPermissionsBoolRemoveResponse : IdempotencyResponse
		{
			public List<Guid> Removed { get; set; } = new List<Guid>();
		}

		public async Task PerformBillingPermissionsBoolRemove(PerformBillingPermissionsBoolRemoveParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformBillingPermissionsBoolRemoveResponse response = new PerformBillingPermissionsBoolRemoveResponse()
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

				if (!permissions.Contains(Databases.Konstants.kPermBillingPermissionsBoolDeleteAny) &&
					!permissions.Contains(Databases.Konstants.kPermBillingPermissionsBoolDeleteCompany)
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

				string[] permissionKeysUsersCanEdit = Databases.Konstants.PermissionKeysUsersCanEdit;


				// Verify we're allowed to edit these keys.

				bool abort = false;
				foreach (string key in p.PermissionKeys)
				{
					if (!permissionKeysUsersCanEdit.Contains(key))
					{
						abort = true;
						break;
					}
				}
				if (abort)
				{
					response.IsError = true;
					response.ErrorMessage = "You aren't allowed to modify that permission.";
					response.IsPermissionsError = true;
					break;
				}


				Dictionary<Guid, BillingPermissionsBool> found = BillingPermissionsBool.ForBillingContactsAndKeys(
					billingConnection,
					new List<Guid>
					{
						p.BillingContactId.Value
					},
					p.PermissionKeys
				);
				if (found.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Couldn't find anything to delete.";
					break;
				}


				BillingPermissionsBool.Delete(billingConnection, found.Keys.ToList());




				response.Removed = BillingPermissionsBool.Delete(billingConnection, found.Keys.ToList());

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
				await Clients.Caller.SendAsync("PerformBillingPermissionsBoolRemoveCB", response).ConfigureAwait(false);
				
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformBillingPermissionsBoolRemoveCB", response).ConfigureAwait(false);
			}
		}
	}
}
