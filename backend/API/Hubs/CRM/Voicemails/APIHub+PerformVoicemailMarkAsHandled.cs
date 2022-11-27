using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;
using API.Properties;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformVoicemailMarkAsHandledParams : IdempotencyRequest
		{
			public Guid? VoicemailId { get; set; }
		}

		public class PerformVoicemailMarkAsHandledResponse : PermissionsIdempotencyResponse
		{
			
		}

		public async Task PerformVoicemailMarkAsHandled(PerformVoicemailMarkAsHandledParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformVoicemailMarkAsHandledResponse response = new PerformVoicemailMarkAsHandledResponse()
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

				if (p.VoicemailId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No voicemail id provided.";
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

				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dp database.";
					break;
				}

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}

				if (null == billingCompany)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing company.";
					break;
				}

				

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				bool permAny = permissions.Contains(EnvDatabases.kPermCRMRequestVoicemailsAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermCRMRequestVoicemailsCompany);

				if (!permAny && !permCompany)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				// Do action.

				var resVM = Voicemails.ForId(dpDBConnection, p.VoicemailId.Value);
				if (0 == resVM.Count)
					break;

				Voicemails vm = resVM.FirstOrDefault().Value;
				vm = vm.MarkHandled(dpDBConnection, $"{billingContact.FullName} using {(string.IsNullOrWhiteSpace(SharedCode.Konstants.APP_BASE_URI) ? Konstants.kAppBaseURINotSetErrorMessage : SharedCode.Konstants.APP_BASE_URI)}", Resources.MarkedHandledNotificationEmailTemplate, billingCompany.Uuid, billingCompany) ?? vm;
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
				await Clients.Caller.SendAsync("PerformVoicemailMarkAsHandledCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformVoicemailMarkAsHandledCB", response).ConfigureAwait(false);
			}

			if (null != p && null != p.VoicemailId)
			{
				_ = RequestVoicemails(new RequestVoicemailsParams
				{
					SessionId = p.SessionId,
					LimitToIds = new List<Guid> { p.VoicemailId.Value },
					IdempotencyToken = Guid.NewGuid().ToString(),
					RoundTripRequestId = Guid.NewGuid().ToString(),
				});
			}

















		}
	}
}
