using SharedCode;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using Serilog;

namespace JobRunnerDatabaseVerification
{
	public static class DPVerify
	{
		
	
		public static async Task Verify(string databaseName) {

			//if (databaseName != "zclient_dp") {
			//	return;
			//}


			Log.Debug($"---- DPVerify.Verify {databaseName}");

			using NpgsqlConnection db = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(databaseName));
			db.Open();

			Log.Debug("---- Ensuring UUID extension exists.");
			db.EnsureUUIDExtension();


			Log.Debug("---- Ensuring timestamp_iso8601 exists");
			db.EnsureTimestampISO8601();

			Log.Debug("---- Verify Tables:");
			VerifyTables(db);
		}

		

		static void VerifyTables(NpgsqlConnection dpDB) {

			Verification.VerifyDPClientDatabase(dpDB, insertDefaultContents: false);


			Log.Debug("----- Done.");
		}
































	}
}
