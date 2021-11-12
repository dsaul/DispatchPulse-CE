using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.Extensions
{
	public static class NpgsqlConnection_CreateDatabase
	{
		public static string? CreateDatabase(this NpgsqlConnection noDatabaseConnection, string dbName, string prefix = "zclient_dp_", string suffixBeforeNumber = "_") {

			if (string.IsNullOrWhiteSpace(dbName))
				throw new Exception("string.IsNullOrWhiteSpace(dbName) 1");

			dbName = SharedCode.RegexUtils.Konstants.NotLettersNumbersRegex.Replace(dbName, "").ToLower(Konstants.KDefaultCulture);
			if (string.IsNullOrWhiteSpace(dbName))
				throw new Exception("string.IsNullOrWhiteSpace(dbName) 2");


			// Iterate up numbers until we find a database name that is not in use, don't go past 100 though.
			string? databaseName = null;
			for (var i = 0; i < 100; i++) {
				databaseName = $"{prefix}{dbName}{suffixBeforeNumber}{i}";

				string cmdText = $"SELECT 1 FROM pg_database WHERE datname='{databaseName}'";
				using NpgsqlCommand cmd = new NpgsqlCommand(cmdText, noDatabaseConnection);
				bool dbExists = cmd.ExecuteScalar() != null;

				if (!dbExists)
					break;

				databaseName = null;
			}

			// Create the named database.
			{
				string cmd = $"CREATE DATABASE {databaseName} WITH OWNER = root;";


				using NpgsqlCommand createDBCommand = new NpgsqlCommand(cmd, noDatabaseConnection);
				createDBCommand.ExecuteNonQuery();
			}

			return databaseName;
		}
	}
}
