using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Databases.Records.CRM;
using Databases.Records.Billing;
using Microsoft.EntityFrameworkCore;
using API.Utility;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformCreateBillingSessionForCredentialsParams : IdempotencyRequest
		{
			public string? CompanyAbbreviation { get; set; }
			public string? ContactEMail { get; set; }
			public string? ContactPassword { get; set; }
			public string? TzIANA { get; set; }

		}
		public class PerformCreateBillingSessionForCredentialsResponse : IdempotencyResponse
		{
			public Guid? SessionId { get; set; }
		}
		public async Task PerformCreateBillingSessionForCredentials(PerformCreateBillingSessionForCredentialsParams p)
		{
			PerformCreateBillingSessionForCredentialsResponse response = new PerformCreateBillingSessionForCredentialsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			BillingContacts? billingContact = null;
			NpgsqlConnection? dpDBConnection = null;


			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				if (string.IsNullOrWhiteSpace(p.CompanyAbbreviation))
				{
					response.IsError = true;
					response.ErrorMessage = "Abbreviation wasn't provided.";
					break;
				}

				if (string.IsNullOrWhiteSpace(p.ContactEMail))
				{
					response.IsError = true;
					response.ErrorMessage = "Abbreviation wasn't provided.";
					break;
				}

				if (string.IsNullOrWhiteSpace(p.ContactPassword))
				{
					response.IsError = true;
					response.ErrorMessage = "Password wasn't provided.";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;

				BillingSessions? session = null;
				BillingCompanies? billingCompany = null;

				
				SessionUtils.CreateSession(
					hub: this,
					response: response,
					companyAbbreviation: p.CompanyAbbreviation,
					contactEMail: p.ContactEMail,
					contactPassword: p.ContactPassword,
					tzIANA: p.TzIANA,
					billingConnectionString: out _,
					billingConnection: out billingConnection,
					session: out session,
					billingContact: out billingContact,
					billingCompany: out billingCompany,
					dpDBName: out _,
					dpDBConnectionString: out _,
					dpDBConnection: out dpDBConnection
					);


				if (null != response.IsError && response.IsError.Value)
					break;

				if (null != session)
					response.SessionId = session.Uuid;

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









			await Clients.Caller.SendAsync("PerformCreateBillingSessionForCredentialsCB", response).ConfigureAwait(false);


		}
	}
}
