using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net;

namespace Databases.Records.Billing
{
	public record UtilityIpToCountry(
		Guid? Uuid,
		int? IpFrom,
		int? IpTo,
		string? CountryCode,
		string? CountryName
		)
	{
		public static UtilityIpToCountry FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			int? ipFrom = default;
			int? ipTo = default;
			string? countryCode = default;
			string? countryName = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("ip-from")) {
				ipFrom = reader.GetInt32("ip-from");
			}
			if (!reader.IsDBNull("ip-to")) {
				ipTo = reader.GetInt32("ip-to");
			}
			if (!reader.IsDBNull("country-code")) {
				countryCode = reader.GetString("country-code");
			}
			if (!reader.IsDBNull("country-name")) {
				countryName = reader.GetString("country-name");
			}

			return new UtilityIpToCountry(
				Uuid: uuid,
				IpFrom: ipFrom,
				IpTo: ipTo,
				CountryCode: countryCode,
				CountryName: countryName
				);

		}

		public static Dictionary<Guid, UtilityIpToCountry> ForIPAddress(NpgsqlConnection connection, IPAddress ipObj) {

			Dictionary<Guid, UtilityIpToCountry> ret = new Dictionary<Guid, UtilityIpToCountry>();


			// Network Byte Order
			//int ipInt = (int)System.BitConverter.ToUInt32(ipObj.GetAddressBytes(), 0);

			// Host Byte Order
			int ipInt = IPAddress.NetworkToHostOrder(
					(int)System.BitConverter.ToUInt32(ipObj.GetAddressBytes(), 0));


			string sql = @"SELECT * from ""utility-ip-to-country"" WHERE @ipInt >= ""ip-from"" AND @ipInt <= ""ip-to"";";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@ipInt", ipInt);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					UtilityIpToCountry obj = UtilityIpToCountry.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}


	}
}
