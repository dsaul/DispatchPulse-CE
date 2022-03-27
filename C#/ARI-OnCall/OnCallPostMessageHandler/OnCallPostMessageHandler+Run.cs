using System;
using System.Collections.Generic;
using Databases.Records.Billing;
using Npgsql;
using Serilog;

namespace ARI.IVR.OnCall
{
	public static partial class OnCallPostMessageHandler
	{
		public static void Run() {

			using NpgsqlConnection billingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
			billingDB.Open();

			Log.Information("Checking PBX for any completed calls.");
			RunCompletedCallCheck(billingDB);

			Log.Information("Begin check for new call out.");

			// Get packages that provision on call auto attendants.
			var resBP = BillingPackages.ForProvisionOnCallAutoAttendants(billingDB, true);
			if (resBP.Count == 0) {
				Log.Information("There are no packages that provision on call auto attendants?");
				return;
			}

			var resSub = BillingSubscriptions.ForPackageIdsAndHasDatabase(billingDB, resBP.Keys);
			if (resSub.Count == 0) {
				Log.Information("There are no subscriptions that reference on call auto attendant packages.");
				return;
			}


			HashSet<Guid> companiesProcessed = new HashSet<Guid>();
			foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in resSub) {
				if (string.IsNullOrWhiteSpace(kvp.Value.ProvisionedDatabaseName))
					continue;
				if (null == kvp.Value.CompanyId)
					continue;
				if (companiesProcessed.Contains(kvp.Value.CompanyId.Value))
					continue;

				RunCompany(billingDB, kvp.Value.CompanyId.Value, kvp.Value.ProvisionedDatabaseName);
				companiesProcessed.Add(kvp.Value.CompanyId.Value);
			}



		}
	}
}
