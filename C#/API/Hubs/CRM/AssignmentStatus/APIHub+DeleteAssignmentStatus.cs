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
		public class DeleteAssignmentStatusParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> AssignmentStatusDelete { get; set; } = new List<Guid>();
		}
		public class DeleteAssignmentStatusResponse : IdempotencyResponse
		{
			public List<Guid> AssignmentStatusDelete { get; set; } = new List<Guid>();
		}

		public async Task DeleteAssignmentStatus(DeleteAssignmentStatusParams p)
		{
			DeleteAssignmentStatusResponse response = new DeleteAssignmentStatusResponse()
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
				if (p.AssignmentStatusDelete == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.AgentsDelete == null";
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

				if (p.AssignmentStatusDelete.Count == 0)
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMDeleteAssignmentsStatusAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMDeleteAssignmentsStatusCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				// delete

				List<Guid> affected = AssignmentStatus.Delete(dpDBConnection, p.AssignmentStatusDelete);
				if (affected.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "No rows deleted?";
					break;
				}

				response.AssignmentStatusDelete = affected;

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
				await Clients.Caller.SendAsync("DeleteAssignmentStatusCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("DeleteAssignmentStatusCB", response).ConfigureAwait(false);
			}





		}
	}
}