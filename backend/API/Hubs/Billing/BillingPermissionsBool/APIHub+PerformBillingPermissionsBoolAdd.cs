using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformBillingPermissionsBoolAddParams : IdempotencyRequest
		{
			public Guid? BillingContactId { get; set; }
			public List<string> PermissionKeys { get; set; } = new List<string>();
		}

		

		public class PerformBillingPermissionsBoolAddResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> BillingPermissionsBool { get; } = new List<Guid>();

		}

		public async Task PerformBillingPermissionsBoolAdd(PerformBillingPermissionsBoolAddParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformBillingPermissionsBoolAddResponse response = new ()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestBillingPermissionsBoolResponse othersMsg = new ()
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

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(EnvDatabases.kPermBillingPermissionsBoolAddAny) &&
					!permissions.Contains(EnvDatabases.kPermBillingPermissionsBoolAddCompany)
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

				string[] permissionKeysUsersCanEdit = EnvDatabases.PermissionKeysUsersCanEdit;


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

				foreach (string key in p.PermissionKeys)
				{
					// Check and see if there is an existing entry for this permission.

					Dictionary<Guid, BillingPermissionsBool> existing = BillingPermissionsBool.ForBillingContactsAndKeys(
						billingConnection,
						new List<Guid>
						{
							p.BillingContactId.Value
						},
						new List<string>
						{
							key
						}
					);
					if (existing.Count > 0)
						continue;


					Guid id = Guid.NewGuid();

					BillingPermissionsBool obj = new BillingPermissionsBool(
						Uuid: id,
						Key: key,
						Value: true,
						ContactId: p.BillingContactId.Value,
						GroupId: null,
						Json: "{}"
						);
					
					
					// Add new entry.

					List<Guid> callerResponse;
					Dictionary<Guid, BillingPermissionsBool> toSendToOthers;
					BillingPermissionsBool.Upsert(
						billingConnection,
						new Dictionary<Guid, BillingPermissionsBool>
						{
							{ id, obj }
						},
						out callerResponse,
						out toSendToOthers
					);

					response.BillingPermissionsBool.AddRange(callerResponse);
					othersMsg.BillingPermissionsBool.AddRange(toSendToOthers.Values);
				}


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

			await Clients.Caller.SendAsync("PerformBillingPermissionsBoolAddCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestBillingPermissionsBoolForCurrentSessionCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingPermissionsBoolForCurrentSessionCB", othersMsg).ConfigureAwait(false);
			}


			if (null != p && response.IsError == false && p.SessionId != null)
			{
				await RequestBillingPermissionsBoolForCurrentSession(new RequestBillingPermissionsBoolParams()
				{
					SessionId = p.SessionId.Value
				}).ConfigureAwait(false);
			}
			
		}

	}
}
