using System;
using System.Collections.Generic;
using SharedCode.DatabaseSchemas;
using Npgsql;
using Serilog;

namespace ARI.IVR.OnCall
{
	public static partial class OnCallPostMessageHandler
	{
		static void RunCompany(NpgsqlConnection billingDB, Guid billingCompanyId, string databaseName) {

			using NpgsqlConnection dpDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(databaseName));
			dpDB.Open();

			var resVM = Voicemails.ForOnCallAttemptsFinished(dpDB, false);
			if (resVM.Count == 0) {
				Log.Information("[{BillingCompanyId}][{Database}] No VMs to check.", billingCompanyId, databaseName);
				return;
			}

			foreach (KeyValuePair<Guid, Voicemails> kvp in resVM) {
				RunMessage(billingDB, dpDB, kvp.Value, billingCompanyId, databaseName);
			}
		}
	}
}
