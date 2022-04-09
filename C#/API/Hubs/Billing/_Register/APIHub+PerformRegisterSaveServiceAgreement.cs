using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using Npgsql;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{


		public class PerformRegisterSaveServiceAgreementParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }

			public string? AgreementText { get; set; }
			public string? SignatureSVG { get; set; }
		}

		public class PerformRegisterSaveServiceAgreementResponse : PermissionsIdempotencyResponse
		{
			public bool? Saved { get; set; } = false;

		}

		public async Task PerformRegisterSaveServiceAgreement(PerformRegisterSaveServiceAgreementParams p)
		{
			PerformRegisterSaveServiceAgreementResponse response = new PerformRegisterSaveServiceAgreementResponse
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
					out dpDBConnection,
					true
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

				if (null == billingCompany || null == billingCompany.JsonObject || null == billingCompany.Uuid)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing company.";
					break;
				}

				if (p.AgreementText == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Save Service Agreement: Didn't recieve agreement text.";
					break;
				}

				if (p.SignatureSVG == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Save Service Agreement: Didn't recieve signature.";
					break;
				}


				JObject? root = billingCompany.JsonObject;

				root["latestServiceAgreementText"] = p.AgreementText;
				root["latestServiceAgreementSVG"] = p.SignatureSVG;
				root["latestServiceAgreementDateAndTime"] = DateTime.UtcNow.ToUniversalTime().ToString("o", Konstants.KDefaultCulture);

				billingCompany = billingCompany with {
					Json = JsonConvert.SerializeObject(root, Formatting.Indented)
				};
				BillingCompanies.Upsert(billingConnection, new Dictionary<Guid, BillingCompanies>
				{
					{ billingCompany.Uuid.Value, billingCompany }
				}, out _, out _);
				

				response.Saved = true;

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


			await Clients.Caller.SendAsync("PerformRegisterSaveServiceAgreementCB", response).ConfigureAwait(false);
		}





	}
}
