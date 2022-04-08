using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Databases.Records.CRM;
using Databases.Records.Billing;
using System.Globalization;
using SharedCode;
using SharedCode.Databases.Records;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{


		public class RegisterCreateDPDatabaseParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }

		}

		public class RegisterCreateDPDatabaseResponse : IdempotencyResponse
		{
			public bool? Created { get; set; } = false;

		}

		public async Task PerformRegisterCreateDPDatabase(RegisterCreateDPDatabaseParams p)
		{
			RegisterCreateDPDatabaseResponse response = new RegisterCreateDPDatabaseResponse
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

				// Get Billing Databsae Connection
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
					response.ErrorMessage = "Create DP Database: Can't find billing company.";
					break;
				}

				Dictionary<Guid, BillingSubscriptions> billingSubscriptions = BillingSubscriptions.ForCompanyId(billingConnection, billingCompany.Uuid.Value);
				if (null == billingSubscriptions || billingSubscriptions.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Create DP Database: Can't find any subscriptions.";
					break;
				}

				Dictionary<Guid, BillingContacts> billingContacts = BillingContacts.ForCompany(billingConnection, billingCompany.Uuid.Value);
				if (null == billingContacts || billingContacts.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Create DP Database: Can't find any contacts.";
					break;
				}

				bool createVerifyRepairDatabase = false;
				string? databaseName = null;

				// Check each subscription's package for whether we should create a database for dispatch pulse.
				foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in billingSubscriptions)
				{
					Guid? packageId = kvp.Value.PackageId;
					if (null == packageId)
						continue;

					var pkgResults = BillingPackages.ForId(billingConnection, packageId.Value);
					if (pkgResults.Count == 0)
						continue;

					BillingPackages package = pkgResults.First().Value;
					if ((null != package.ProvisionDispatchPulse && package.ProvisionDispatchPulse.Value) && (null != package.ProvisionDispatchPulseUsers && package.ProvisionDispatchPulseUsers.Value > 0))
					{
						createVerifyRepairDatabase = true;
					}

					if (!string.IsNullOrWhiteSpace(kvp.Value.ProvisionedDatabaseName))
					{
						databaseName = kvp.Value.ProvisionedDatabaseName;
					}
				}

				if (false == createVerifyRepairDatabase)
				{
					response.IsError = true;
					response.ErrorMessage = "Create DP Database: None of the packages provision dispatch pulse.";
					break;
				}







































				// Check if already provided database name exists.
				using NpgsqlConnection noDatabaseConnection = new NpgsqlConnection(Databases.Konstants.NPGSQL_CONNECTION_STRING);
				noDatabaseConnection.Open();

				{
					string cmdText = "SELECT 1 FROM pg_database WHERE datname='${databaseName}'";
					using NpgsqlCommand cmd = new NpgsqlCommand(cmdText, noDatabaseConnection);
					bool dbExists = cmd.ExecuteScalar() != null;

					if (dbExists)
						databaseName = null;
				}

				// Create a database name. 
				if (string.IsNullOrWhiteSpace(databaseName))
				{
					string abbreviation = "";
					if (!string.IsNullOrWhiteSpace(billingCompany.Abbreviation))
						abbreviation = billingCompany.Abbreviation;

					string abbrMod = RegexUtils.NotLettersNumbersRegex.Replace(abbreviation, "").ToLower(Konstants.KDefaultCulture);
					if (string.IsNullOrWhiteSpace(abbrMod))
					{
						response.IsError = true;
						response.ErrorMessage = "Create DP Database: Company abbreviation for database is empty!";
						break;
					}

					// Iterate up numbers until we find a database name that is not in use, don't go past 100 though.
					for (var i = 0; i < 100; i++)
					{
						databaseName = $"zclient_dp_{abbrMod}_{i}";

						string cmdText = "SELECT 1 FROM pg_database WHERE datname='${databaseName}'";
						using NpgsqlCommand cmd = new NpgsqlCommand(cmdText, noDatabaseConnection);
						bool dbExists = cmd.ExecuteScalar() != null;

						if (!dbExists)
							break;

						databaseName = null;
					}


				}

				if (null == databaseName)
				{
					response.IsError = true;
					response.ErrorMessage = "Create DP Database: We could not determine a proper database name.";
					break;
				}

				// Create the named database.
				{
					string cmd = @"
					CREATE DATABASE " + databaseName + @" WITH OWNER = root;
					";


					using NpgsqlCommand createDBCommand = new NpgsqlCommand(cmd, noDatabaseConnection);

					try
					{
						createDBCommand.ExecuteNonQuery();
					}
					catch (Exception)
					{
						response.IsError = true;
						response.ErrorMessage = $"Create DP Database: Exception while creating database {databaseName}.";
						break;
					}


					// 
					foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in billingSubscriptions)
					{
						Guid? packageId = kvp.Value.PackageId;
						if (null == packageId)
							continue;

						var pkgResults = BillingPackages.ForId(billingConnection, packageId.Value);
						if (pkgResults.Count == 0)
							continue;

						BillingPackages package = pkgResults.First().Value;



						if (
							(null != package.ProvisionDispatchPulse && package.ProvisionDispatchPulse.Value) && 
							(null != package.ProvisionDispatchPulseUsers && package.ProvisionDispatchPulseUsers.Value > 0) &&
							null != kvp.Value.Uuid
							)
						{
							// update provisioned database name

							BillingSubscriptions mod = kvp.Value with { ProvisionedDatabaseName = databaseName };
							BillingSubscriptions.Upsert(billingConnection, new Dictionary<Guid, BillingSubscriptions> {
								{ mod.Uuid.Value, mod }
							}, out _, out _);

						}
					}
				}


				// Populate the database.
				string connectionString = Databases.Konstants.DatabaseConnectionStringForDB(databaseName);
				using NpgsqlConnection newDatabaseConnection = new NpgsqlConnection(connectionString);
				newDatabaseConnection.Open();

				newDatabaseConnection.EnsureUUIDExtension();
				newDatabaseConnection.EnsureTimestampISO8601();

				Verification.VerifyDPClientDatabase(newDatabaseConnection, insertDefaultContents: true);

				Guid dpCompanyId = Guid.NewGuid();
				// Add an addressbook entry for this company.
				{
					JObject dpCompanyJSON = new JObject
					{
						["id"] = dpCompanyId,
						["name"] = billingCompany.FullName,
						["logoURI"] = "",
						["websiteURI"] = "",
						["lastModifiedBillingId"] = null,
					};

					Companies dpCompany = new Companies(
						Id: dpCompanyId,
						SearchString: $"{billingCompany.FullName}",
						LastModifiedIso8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
						Json: dpCompanyJSON.ToString(Formatting.Indented)
						);

					Companies.Upsert(newDatabaseConnection, new Dictionary<Guid, Companies>
					{
						{ dpCompanyId, dpCompany }
					}, out _, out _);


				}


				// Add each of the billing contacts as contacts and agents, then add these ids to the billing contact metadata.
				foreach (KeyValuePair<Guid,BillingContacts> kvp in billingContacts)
				{
					// Add contact

					Guid contactId = Guid.NewGuid();

					JObject dpContactJSON = new JObject
					{
						["id"] = contactId,
						["lastModifiedBillingId"] = null,
						["name"] = kvp.Value.FullName,
						["title"] = "",
						["companyId"] = dpCompanyId,
						["notes"] = "",
						["phoneNumbers"] = new JArray
						{
							new JObject
							{
								["id"] = Guid.NewGuid(),
								["label"] = "Work",
								["value"] = kvp.Value.Phone,
							},
						},
						["addresses"] = new JArray
						{

						},
						["emails"] = new JArray
						{
							new JObject
							{
								["id"] = Guid.NewGuid(),
								["label"] = "Work",
								["value"] = kvp.Value.Email,
							},
						},
					};

					Contacts dpContact = new Contacts(
						Id: contactId,
						SearchString: $"{kvp.Value.FullName} {kvp.Value.Phone} {kvp.Value.Email}",
						LastModifiedIso8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
						Json: dpContactJSON.ToString(Formatting.Indented)
						);
					Contacts.Upsert(newDatabaseConnection, new Dictionary<Guid, Contacts>
					{
						{ contactId, dpContact }
					}, out _, out _);







					// Add agent.


					var statusRes = AgentsEmploymentStatus.GetDefaultCurrentEmployee(newDatabaseConnection);
					Guid? statusId = null;

					if (statusRes.Count > 0)
					{
						AgentsEmploymentStatus status = statusRes.First().Value;
						statusId = status.Id == null ? null : status.Id.Value;
					}





					Guid agentID = Guid.NewGuid();

					JObject dpAgentJSON = new JObject
					{
						["id"] = agentID,
						["lastModifiedBillingId"] = null,
						["name"] = kvp.Value.FullName,
						["title"] = "",
						["employmentStatusId"] = statusId,
						["hourlyWage"] = 0,
						["notificationSMSNumber"] = kvp.Value.Phone,
					};

					Agents dpAgent = new Agents(
						Id: agentID,
						SearchString: $"{kvp.Value.FullName}",
						LastModifiedIso8601: DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
						Json: dpAgentJSON.ToString(Formatting.Indented)

						);
					Agents.Upsert(newDatabaseConnection, new Dictionary<Guid, Agents>
					{
						{ agentID, dpAgent }
					}, out _, out _);



					// add the above ids to billing contact.
					if (null != kvp.Value.ApplicationData)
					{
						JObject? appData = JsonConvert.DeserializeObject(kvp.Value.ApplicationData, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
						if (null == appData)
							continue;

						appData[BillingContacts.kApplicationDataKeyDispatchPulseContactId] = dpContact.Id;
						appData[BillingContacts.kApplicationDataKeyDispatchPulseAgentId] = dpAgent.Id;

						BillingContacts modContacts = kvp.Value with { ApplicationData = appData.ToString(Formatting.Indented) };
						BillingContacts.Upsert(billingConnection, new Dictionary<Guid, BillingContacts>
							{
								{ kvp.Key, modContacts }
							}, out _, out _);

					}



				}


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

			await Clients.Caller.SendAsync("PerformRegisterCreateDPDatabaseCB", response).ConfigureAwait(false);
		}


	}
}
