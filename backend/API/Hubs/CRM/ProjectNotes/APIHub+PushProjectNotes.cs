using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushProjectNotesParams : IdempotencyRequest
		{
			public Dictionary<Guid, ProjectNotes> ProjectNotes { get; set; } = new Dictionary<Guid, ProjectNotes>();
		}
		public class PushProjectNotesResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> ProjectNotes { get; set; } = new List<Guid>();
		}

		public async Task PushProjectNotes(PushProjectNotesParams p)
		{
			PushProjectNotesResponse response = new PushProjectNotesResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestProjectNotesResponse othersMsg = new RequestProjectNotesResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, ProjectNotes> toSendToOthers = new Dictionary<Guid, ProjectNotes>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.ProjectNotes == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.ProjectNotes == null";
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

				if (!permissions.Contains(EnvDatabases.kPermCRMPushProjectNotesAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMPushProjectNotesCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				ProjectNotes.Upsert(
					dpDBConnection,
					p.ProjectNotes,
					out callerResponse,
					out toSendToOthers
					);


				response.ProjectNotes = callerResponse;
				othersMsg.ProjectNotes = toSendToOthers;

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


			await Clients.Caller.SendAsync("PushProjectNotesCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestProjectNotesCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestProjectNotesCB", othersMsg).ConfigureAwait(false);
			}








		}
	}
}