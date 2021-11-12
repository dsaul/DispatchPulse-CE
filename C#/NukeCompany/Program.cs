using Databases.Records.Billing;
using Npgsql;
using System;
using System.Linq;
using System.Collections.Generic;
using Serilog;
using Serilog.Events;

namespace NukeCompany
{
	class Program
	{
		// This isn't secure, it just prevents those who don't understand programs from accidently using the program.
		const string programPasscode = "Oja4TjjhHvKawyN0hpLHvsFNI5Ba8Tl1lpLmp3a9WrunDNQorEEmGyaCDc82YVRsS2nOYw8K2fzOq2RkNAVFTmwuaN6cj8uJkV5i";

		static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.Enrich.WithMachineName()
				.Enrich.FromLogContext()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.WithMachineName()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
				.WriteTo.Console()
				//.WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), SERILOG_LOG_FILE)
				.CreateLogger();

			Log.Debug("Company Nuke");

			Console.Write("Enter Password: ");
			if (programPasscode != Console.ReadLine()) {
				Log.Debug("Not Authorized.");
				return;
			}

			string? NPGSQL_CONNECTION_STRING = Databases.Konstants.NPGSQL_CONNECTION_STRING;
			if (string.IsNullOrWhiteSpace(NPGSQL_CONNECTION_STRING)) {
				Log.Debug("NPGSQL_CONNECTION_STRING_FILE must be set");
				return;
			}
			string? PGPASSFILE = System.Environment.GetEnvironmentVariable("PGPASSFILE");
			if (string.IsNullOrWhiteSpace(PGPASSFILE)) {
				Log.Debug("PGPASSFILE must be set");
				return;
			}

			Console.Write("Company UUID: ");
			string? companyIdStr = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(companyIdStr)) {
				Log.Debug("string.IsNullOrWhiteSpace(companyId)");
				return;
			}

			HashSet<string> databaseNames = new HashSet<string>();

			using NpgsqlConnection billingDB = new NpgsqlConnection(Databases.Konstants.KBillingDatabaseConnectionString+"Include Error Detail=true;");
			billingDB.Open();

			var transaction = billingDB.BeginTransaction();
			try {
				Guid companyId = Guid.Parse(companyIdStr);

				var resCompanyId = BillingCompanies.ForIds(billingDB, companyId);
				if (0 == resCompanyId.Count) {
					Log.Debug("0 == resCompanyId.Count");
					return;
				}

				BillingCompanies billingCompany = resCompanyId.FirstOrDefault().Value;


				var resSubscriptions = BillingSubscriptions.ForCompanyId(billingDB, companyId);
				

				if (0 != resSubscriptions.Count) {
					foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in resSubscriptions) {
						if (!string.IsNullOrWhiteSpace(kvp.Value.ProvisionedDatabaseName)) {
							databaseNames.Add(kvp.Value.ProvisionedDatabaseName);
						}
					}
				}












				Log.Debug($"We found this entry:");
				Log.Debug($"FullName: {billingCompany.FullName}");
				Log.Debug($"Abbreviation: {billingCompany.Abbreviation}");
				Log.Debug($"Database Names: {string.Join(' ', databaseNames)}");
				Console.Write("Is this correct (y/n)? ");

				if ("y" != Console.ReadLine()) {
					return;
				}

				Console.Write("Type the Company UUID again to confirm deleting: ");
				if (billingCompany.Uuid.ToString() != Console.ReadLine()) {
					return;
				}

				Log.Debug("Beginning to delete all info for the above company.");
				// BillingCompanies is first because we need to delete the 'invoice-contact-id'

				BillingCompanies mod = billingCompany with
				{
					InvoiceContactId = null
				};
				if (null != mod.Uuid) {
					BillingCompanies.Upsert(billingDB, new Dictionary<Guid, BillingCompanies> {
					{ mod.Uuid.Value, mod }
				}, out _, out _);
				}



				var resContacts = BillingContacts.ForCompany(billingDB, companyId);

				List<Guid> contactIds = resContacts.Keys.ToList();

				// BillingSubscriptionsProvisioningStatus None
				Console.Write("BillingSubscriptions...");
				BillingSubscriptions.DeleteForCompanyIds(billingDB, new List<Guid> { companyId });
				Log.Debug("done.");
				Console.Write("BillingSessions...");
				BillingSessions.DeleteForContactIds(billingDB, contactIds);
				Log.Debug("done.");
				Console.Write("BillingPermissionsGroupsMemberships...");
				BillingPermissionsGroupsMemberships.DeleteForContactIds(billingDB, contactIds);
				Log.Debug("done.");
				// BillingPermissionsGroups None
				Console.Write("BillingPermissionsBool...");
				BillingPermissionsBool.DeleteForContactIds(billingDB, contactIds);
				Log.Debug("done.");
				// BillingPaymentMethod
				// BillingPaymentFrequencies
				// BillingPackagesType
				// BillingPackages
				// BillingJournalEntriesType
				Console.Write("BillingJournalEntries...");
				BillingJournalEntries.DeleteForCompanyId(billingDB, new List<Guid> { companyId });
				Log.Debug("done.");
				Console.Write("BillingInvoices...");
				BillingInvoices.DeleteForCompanyId(billingDB, new List<Guid> { companyId });
				Log.Debug("done.");
				// BillingIndustries
				// BillingCurrency
				// BillingCouponCodes
				Console.Write("BillingContacts...");
				BillingContacts.DeleteForCompanyId(billingDB, new List<Guid> { companyId });
				Log.Debug("done.");
				Console.Write("BillingCompanies...");
				BillingCompanies.Delete(billingDB, new List<Guid> { companyId }); ;
				Log.Debug("done.");

				
			}
			catch {
				transaction.Rollback();
				throw;
			}

			transaction.Commit();

			transaction = billingDB.BeginTransaction();
			try {
				Console.Write("Killing connections to databases that will be deleted...");
				using NpgsqlConnection noDBConnection = new NpgsqlConnection(Databases.Konstants.NPGSQL_CONNECTION_STRING);
				noDBConnection.Open();

				foreach (string dbName in databaseNames) {
					Console.Write($" {dbName} ");
					string sql = @"
					SELECT 
						pg_terminate_backend(pid) 
					FROM 
						pg_stat_activity 
					WHERE 
						-- don't kill my own connection!
						pid <> pg_backend_pid()
						-- don't kill the connections to other databases
						AND datname = '"+dbName+@"'
						;
					";
					using NpgsqlCommand cmd = new NpgsqlCommand(sql, noDBConnection);
					cmd.ExecuteNonQuery();
				}
				Log.Debug("done.");
				Console.Write("Dropping databases...");
				foreach (string dbName in databaseNames) {
					Console.Write($" {dbName} ");
					string sql = $"DROP DATABASE {dbName};";
					using NpgsqlCommand cmd = new NpgsqlCommand(sql, noDBConnection);
					cmd.ExecuteNonQuery();
				}
				Log.Debug("done.");
				

			}
			catch {
				transaction.Rollback();
				throw;
			}

			transaction.Commit();















		}
	}
}
