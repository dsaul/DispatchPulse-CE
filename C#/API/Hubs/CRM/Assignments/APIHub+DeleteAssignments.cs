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
		public class DeleteAssignmentsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> AssignmentsDelete { get; set; } = new List<Guid>();
		}
		public class DeleteAssignmentsResponse : IdempotencyResponse
		{
			public List<Guid> AssignmentsDelete { get; set; } = new List<Guid>();
		}
		public async Task DeleteAssignments(DeleteAssignmentsParams p)
		{
			DeleteAssignmentsResponse response = new DeleteAssignmentsResponse()
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
				if (p.AssignmentsDelete == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.AssignmentsDelete == null";
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

				if (p.AssignmentsDelete.Count == 0)
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMDeleteAssignmentsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMDeleteAssignmentsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// delete

				List<Guid> affected = Assignments.Delete(dpDBConnection, p.AssignmentsDelete);
				if (affected.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "No rows deleted?";
					break;
				}

				response.AssignmentsDelete = affected;















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
				await Clients.Caller.SendAsync("DeleteAssignmentsCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("DeleteAssignmentsCB", response).ConfigureAwait(false);
			}





		}
	}
}