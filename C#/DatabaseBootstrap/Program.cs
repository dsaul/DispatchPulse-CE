using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using SharedCode.Extensions;

namespace DatabaseBootstrap
{
	public class Program
	{
		public static void Main(string[] args) {
			Console.WriteLine("Dispatch Pulse Database Bootstrap");



			using NpgsqlConnection? noDatabaseConnection = new NpgsqlConnection(Databases.Konstants.NPGSQL_CONNECTION_STRING);

			try {
				noDatabaseConnection.Open();
			}
			catch (Exception ex) {
				Log.Error(ex, "Unable to connect to database.");
			}

			// Make sure that the job_runner database exists.
			if (false == noDatabaseConnection.DatabaseExists("job_runner")) {
				Log.Warning("job_runner database doesn't exist, creating.");
				noDatabaseConnection.CreateDatabase(
					dbName: "job_runner",
					prefix: "",
					suffixBeforeNumber:"", 
					noNumberIteration: true
				);
			}




			//{
			//	//actualDBName = noDatabaseConnection.CreateDatabase(DatabaseNameUniqueSearchFragment);
			//}
			//


			//NpgsqlConnection db = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(actualDBName));
			//db.Open();
			//{
			//	db.EnsureUUIDExtension();
			//	db.EnsureTimestampISO8601();
			//	Verification.RunAllVerifications(db, insertDefaultContents: true);
			//}
			//db.Close();






			noDatabaseConnection.Close();

			// We don't want docker to keep relaunching this program,
			// so if it got to this point, sleep repeatedly forever.
			while (true) {
				Thread.Sleep(1000);
			}
		}
	}
}