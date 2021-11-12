﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.Extensions
{
	public static class NpgsqlConnection_TableExists
	{
		public static bool TableExists(this NpgsqlConnection db, string tableName) {
			string sql = @"
				SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name  = @tableName
			";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
			cmd.Parameters.AddWithValue("@tableName", tableName);
			return cmd.ExecuteScalar() != null;
		}
	}
}
