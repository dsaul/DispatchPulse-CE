using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Generic;
using SharedCode;
using Newtonsoft.Json.Linq;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class PerformRegisterPaymentInformationParams : IdempotencyRequest
		{

			public Guid? SessionId { get; set; }

			public string? PaymentFrequency { get; set; }

			public long? NumberOfSeats { get; set; }
			public string? Currency { get; set; }

		}

		public class PerformRegisterPaymentInformationResponse : PermissionsIdempotencyResponse
		{
			public bool? Saved { get; set; } = false;

		}

		public async Task PerformRegisterPaymentInformation(PerformRegisterPaymentInformationParams p)
		{
			PerformRegisterPaymentInformationResponse response = new PerformRegisterPaymentInformationResponse
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

				if (null == billingCompany || null == billingCompany.Uuid)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing company.";
					break;
				}

				if (p.PaymentFrequency == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Register Payment Information: Didn't recieve payment frequency.";
					break;
				}


				if (p.NumberOfSeats == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Register Payment Information: Didn't recieve number of seats.";
					break;
				}

				if (p.Currency == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Register Payment Information: Didn't recieve the currency.";
					break;
				}

				// Set the payment frequency.
				billingCompany = billingCompany with
				{
					PaymentFrequency = p.PaymentFrequency
				};
				BillingCompanies.Upsert(billingConnection, new Dictionary<Guid, BillingCompanies>
				{
					{ billingCompany.Uuid.Value, billingCompany }
				}, out _, out _);

				// Find the appropriate seat package.
				BillingPackages? billingPackage = null;
				{
					string packageName = p.Currency == "CAD" ? "CPST-USER-CAD-1" : "CPST-USER-USD-1";

					Dictionary<Guid, BillingPackages> packages = BillingPackages.ForPackageName(billingConnection, packageName);
					billingPackage = packages.FirstOrDefault().Value;
				}

				if (billingPackage == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Register Payment Information: Couldn't find the package.";
					break;
				}


				// Add a subscription to that package n times.
				for (var i = 0; i < p.NumberOfSeats; i++)
				{
					Guid subId = Guid.NewGuid();
					BillingSubscriptions sub = new BillingSubscriptions(
						Uuid: subId,
						CompanyId: billingCompany.Uuid,
						PackageId: billingPackage.Uuid,
						ProvisioningActual: "Unprovisioned",
						ProvisioningDesired: "Unprovisioned",
						TimestampAddedUtc: DateTime.UtcNow,
						ProvisionedDatabaseName: null,
						TimestampLastSettingsPushUtc: null,
						Json: "{}"
						);
					BillingSubscriptions.Upsert(billingConnection, new Dictionary<Guid, BillingSubscriptions>
					{
						{ subId, sub }
					}, out _, out _);
					
				}


				// Add the currency to the company.
				do
				{
					JObject? copy = null == billingCompany.JsonObject ? new JObject() : billingCompany.JsonObject.DeepClone() as JObject;
					if (null == copy)
						break;
					if (null == billingCompany)
						break;

					copy[BillingCompanies.kJsonKeyCurrency] = p.Currency;

					BillingCompanies mod = billingCompany with
					{
						Json = copy.ToString(Newtonsoft.Json.Formatting.Indented)
					};

					BillingCompanies.Upsert(billingConnection, new Dictionary<Guid, BillingCompanies>
					{
						{ billingCompany.Uuid.Value, mod }
					}, out _, out _);
				}
				while (false);
				
				








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


			await Clients.Caller.SendAsync("PerformRegisterPaymentInformationCB", response).ConfigureAwait(false);
		}
	}
}
