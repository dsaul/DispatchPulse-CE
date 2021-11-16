using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Databases.Records.CRM;
using Databases.Records.Billing;
using Stripe;
using System.Text.RegularExpressions;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class PerformRegisterNewCompanyParams : IdempotencyRequest
		{
			public string? Name { get; set; }
			public string? Abbreviation { get; set; }
			public string? Industry { get; set; }
			public string? MarketingCampaign { get; set; }
			public string? AddressLine1 { get; set; }
			public string? AddressLine2 { get; set; }
			public string? AddressCity { get; set; }
			public string? AddressProvince { get; set; }
			public string? AddressPostalCode { get; set; }
			public string? AddressCountry { get; set; }
			public string? MainContactEMail { get; set; }
			public string? MainContactPhoneNumber { get; set; }
		}
		public class PerformRegisterNewCompanyResponse : IdempotencyResponse
		{
			public bool? Created { get; set; }
			public Guid? BillingCompanyId { get; set; }
			public Guid? BillingContactId { get; set; }
			public Guid? BillingSessionId { get; set; }
			public string? StripeIntentClientSecret { get; set; }

		}


		public async Task PerformRegisterNewCompany(PerformRegisterNewCompanyParams p)
		{

			PerformRegisterNewCompanyResponse response = new PerformRegisterNewCompanyResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
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


				string billingConnectionString = Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.KBillingDatabaseName);
				if (string.IsNullOrWhiteSpace(billingConnectionString))
				{
					response.IsError = true;
					response.ErrorMessage = "Couldn't get connection information for the billing system.";
					break;
				}

				billingConnection = new NpgsqlConnection(billingConnectionString);
				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Couldn't open a connection to the billing system. #1";
					break;
				}


				billingConnection.Open();
				if (billingConnection.State != System.Data.ConnectionState.Open)
				{
					response.IsError = true;
					response.ErrorMessage = "Couldn't open a connection to the billing system. #2";
					break;
				}

				// Verify params.
				string name = p.Name == null ? "" : p.Name.Trim();

				if (string.IsNullOrWhiteSpace(p.Name))
				{
					response.IsError = true;
					response.ErrorMessage = "The company name cannot be empty.";
					break;
				}
				if (string.IsNullOrWhiteSpace(p.Abbreviation))
				{
					response.IsError = true;
					response.ErrorMessage = "The company abbreviation cannot be empty.";
					break;
				}

				string abbrLower = p.Abbreviation.Trim().ToLower(Konstants.KDefaultCulture);

				// Make sure the abbreviation isn't already used.
				Dictionary<Guid, BillingCompanies> existingForAbbr = BillingCompanies.ForAbbreviation(billingConnection, abbrLower);
				if (existingForAbbr.Count > 0)
				{
					response.IsError = true;
					response.ErrorMessage = "The company abbreviation already exists.";
					break;
				}

				// Industry
				string industry = p.Industry == null ? "" : p.Industry.Trim();

				// MarketingCampaign
				string marketingCampaign = p.MarketingCampaign == null ? "" : p.MarketingCampaign.Trim();

				string addrL1 = p.AddressLine1 == null ? "" : p.AddressLine1.Trim();
				if (string.IsNullOrWhiteSpace(addrL1))
				{
					response.IsError = true;
					response.ErrorMessage = "We require your company's address.";
					break;
				}

				// AddressLine2
				string addrL2 = p.AddressLine2 == null ? "" : p.AddressLine2.Trim();


				string addrCity = p.AddressCity == null ? "" : p.AddressCity.Trim();
				if (string.IsNullOrWhiteSpace(addrCity))
				{
					response.IsError = true;
					response.ErrorMessage = "We require your company's address.";
					break;
				}

				string addrProvince = p.AddressProvince == null ? "" : p.AddressProvince.Trim();
				if (string.IsNullOrWhiteSpace(addrProvince))
				{
					response.IsError = true;
					response.ErrorMessage = "We require your company's address.";
					break;
				}

				string addrPostalCode = p.AddressPostalCode == null ? "" : p.AddressPostalCode.Trim();
				if (string.IsNullOrWhiteSpace(addrPostalCode))
				{
					response.IsError = true;
					response.ErrorMessage = "We require your company's address.";
					break;
				}

				string addrCountry = p.AddressCountry == null ? "" : p.AddressCountry.Trim();
				if (string.IsNullOrWhiteSpace(addrCountry))
				{
					response.IsError = true;
					response.ErrorMessage = "We require your company's address.";
					break;
				}

				string mainContactEmail = p.MainContactEMail == null ? "" : p.MainContactEMail.Trim();
				if (string.IsNullOrWhiteSpace(mainContactEmail))
				{
					response.IsError = true;
					response.ErrorMessage = "We require a way to contact you by e-mail.";
					break;
				}

				if (false == IsValidEmail(mainContactEmail))
				{
					response.IsError = true;
					response.ErrorMessage = "The e-mail provided is not valid.";
					break;
				}

				string mainContactPhoneNumber = p.MainContactPhoneNumber == null ? "" : p.MainContactPhoneNumber.Trim();
				if (string.IsNullOrWhiteSpace(p.MainContactPhoneNumber))
				{
					response.IsError = true;
					response.ErrorMessage = "We require a way to contact you by phone.";
					break;
				}

				// Create the company entry.
				Guid newCompanyId = Guid.NewGuid();
				BillingCompanies newCompany = new BillingCompanies(
					Uuid: newCompanyId,
					FullName: name,
					Abbreviation: abbrLower,
					Industry: industry,
					MarketingCampaign: marketingCampaign,
					AddressLine1: addrL1,
					AddressLine2: addrL2,
					AddressCity: addrCity,
					AddressProvince: addrProvince,
					AddressPostalCode: addrPostalCode,
					AddressCountry: addrCountry,
					StripeCustomerId: null,
					PaymentMethod: "Invoice",
					PaymentFrequency: BillingCompanies.kPaymentFrequenciesValueAnnually,
					InvoiceContactId: null,
					Json: "{}"
					);
				BillingCompanies.Upsert(billingConnection, new Dictionary<Guid, BillingCompanies>
				{
					{ newCompanyId, newCompany }
				}, out _, out _);

				response.BillingCompanyId = newCompany.Uuid;

				Guid newContactId = Guid.NewGuid();
				BillingContacts newContact = new BillingContacts(
					Uuid: newContactId,
					FullName: $"Unamed Primary Contact for {newCompany.FullName}",
					Email: mainContactEmail,
					PasswordHash: "",
					EmailListMarketing: false,
					EmailListTutorials: false,
					MarketingCampaign: marketingCampaign,
					Phone: mainContactPhoneNumber,
					CompanyId: newCompany.Uuid,
					ApplicationData: "{}",
					Json: "{}"
					);
				BillingContacts.Upsert(billingConnection, new Dictionary<Guid, BillingContacts>
				{
					{ newContactId, newContact }
				}, out _, out _);
				
				response.BillingContactId = newContact.Uuid;

				// Update company with new invoice contact id.
				newCompany = newCompany with
				{
					InvoiceContactId = newContact.Uuid
				};
				BillingCompanies.Upsert(billingConnection, new Dictionary<Guid, BillingCompanies>
				{
					{ newCompanyId, newCompany }
				}, out _, out _);

				// Add default user to company owner group
				Guid newGroupMembershipId = Guid.NewGuid();
				BillingPermissionsGroupsMemberships newGroupMembership = new BillingPermissionsGroupsMemberships(
					Id: newGroupMembershipId,
					GroupId: Konstants.KCompanyOwnerGroupId, // Company Owner
					ContactId: newContact.Uuid,
					Json: "{}"
					);
				BillingPermissionsGroupsMemberships.Upsert(billingConnection, new Dictionary<Guid, BillingPermissionsGroupsMemberships>
				{
					{ newGroupMembershipId, newGroupMembership }
				}, out _, out _);

				// Create Session
				Guid newSessionId = Guid.NewGuid();
				BillingSessions newSession = new BillingSessions(
					Uuid: newSessionId,
					ContactId: newContact.Uuid,
					AgentDescription: "New Register",
					IpAddress: "",
					CreatedUtc: DateTime.UtcNow,
					LastAccessUtc: DateTime.UtcNow,
					Json: "{}"
					);
				BillingSessions.Upsert(billingConnection, new Dictionary<Guid, BillingSessions> {
					{ newSessionId, newSession }
				}, out _, out _);
				

				response.BillingSessionId = newSession.Uuid;

				response.Created = true;


				

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




			await Clients.Caller.SendAsync("PerformRegisterNewCompanyCB", response).ConfigureAwait(false);
		}

		bool IsValidEmail(string email)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				bool mailClassSuccess = addr.Address == email;



				Regex stripeEmailRegex = new Regex(@".*@.*\..*");
				Match stripeEmailRegexMatch = stripeEmailRegex.Match(email);
				return mailClassSuccess && stripeEmailRegexMatch.Success;

			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch
			{
				return false;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}


	}
}
