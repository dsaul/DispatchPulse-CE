using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class DeleteSettingsProvisioningParams : IdempotencyRequest
		{
			public List<Guid> SettingsProvisioningDelete { get; set; } = new List<Guid>();
		}
		public class DeleteSettingsProvisioningResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> SettingsProvisioningDelete { get; set; } = new List<Guid>();
		}

		public async Task DeleteSettingsProvisioning(DeleteSettingsProvisioningParams p)
		{


			DeleteSettingsProvisioningResponse response = new DeleteSettingsProvisioningResponse()
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
				if (p.SettingsProvisioningDelete == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.SettingsProvisioningDelete == null";
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

				if (p.SettingsProvisioningDelete.Count == 0)
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

				if (!permissions.Contains(EnvDatabases.kPermCRMDeleteSettingsProvisioningAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMDeleteSettingsProvisioningCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// delete

				List<Guid> affected = SettingsProvisioning.Delete(dpDBConnection, p.SettingsProvisioningDelete);
				if (affected.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "No rows deleted?";
					break;
				}

				response.SettingsProvisioningDelete = affected;
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
				await Clients.Caller.SendAsync("DeleteSettingsProvisioningCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("DeleteSettingsProvisioningCB", response).ConfigureAwait(false);
			}








		}
	}
}