using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Databases.Records.Billing;
using Databases.Records.CRM;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class DeleteVoicemailsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> VoicemailsDelete { get; set; } = new List<Guid>();
		}

		public class DeleteVoicemailsResponse : IdempotencyResponse
		{

			public List<Guid> VoicemailsDelete { get; set; } = new List<Guid>();
		}

		public async Task DeleteVoicemails(DeleteVoicemailsParams p)
		{
			DeleteVoicemailsResponse response = new DeleteVoicemailsResponse()
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
				if (p.VoicemailsDelete == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.VoicemailsDelete == null";
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

				if (p.VoicemailsDelete.Count == 0)
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMDeleteVoicemailsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMDeleteVoicemailsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// delete

				List<Guid> affected = Voicemails.Delete(dpDBConnection, p.VoicemailsDelete);
				if (affected.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "No rows deleted?";
					break;
				}
				
				response.VoicemailsDelete = affected;
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
				await Clients.Caller.SendAsync("DeleteVoicemailsCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("DeleteVoicemailsCB", response).ConfigureAwait(false);
			}


		}
	}
}