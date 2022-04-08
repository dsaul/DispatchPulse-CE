﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SharedCode;
using Serilog;

namespace Databases.Records.Billing
{
	public record BillingSubscriptionsProvisioningStatus(
		Guid? Uuid,
		string? Status,
		string? Json
		)
	{
		public static Dictionary<Guid, BillingSubscriptionsProvisioningStatus> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingSubscriptionsProvisioningStatus> ret = new Dictionary<Guid, BillingSubscriptionsProvisioningStatus>();

			string sql = @"SELECT * from ""billing-subscriptions-provisioning-status"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptionsProvisioningStatus obj = BillingSubscriptionsProvisioningStatus.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingSubscriptionsProvisioningStatus> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingSubscriptionsProvisioningStatus> ret = new Dictionary<Guid, BillingSubscriptionsProvisioningStatus>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-subscriptions-provisioning-status\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptionsProvisioningStatus obj = BillingSubscriptionsProvisioningStatus.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingSubscriptionsProvisioningStatus> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingSubscriptionsProvisioningStatus> ret = new Dictionary<Guid, BillingSubscriptionsProvisioningStatus>();

			string sql = @"SELECT * from ""billing-subscriptions-provisioning-status""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptionsProvisioningStatus obj = BillingSubscriptionsProvisioningStatus.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}



		public static List<Guid> Delete(NpgsqlConnection connection, List<Guid> idsToDelete) {

			List<Guid> toSendToOthers = new List<Guid>();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"billing-subscriptions-provisioning-status\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsToDelete[i]);
			}



			int rowsAffected = cmd.ExecuteNonQuery();
			if (rowsAffected == 0) {
				return toSendToOthers;
			}

			toSendToOthers.AddRange(idsToDelete);
			return toSendToOthers;



		}




		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingSubscriptionsProvisioningStatus> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingSubscriptionsProvisioningStatus> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingSubscriptionsProvisioningStatus>();

			foreach (KeyValuePair<Guid, BillingSubscriptionsProvisioningStatus> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-subscriptions-provisioning-status""
						(
							""uuid"",
							""status"",
							""json""
						)
					VALUES
						(
							@uuid,
							@status,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""status"" = excluded.""status"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(kvp.Value.Status) ? (object)DBNull.Value : kvp.Value.Status);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}



		public static BillingSubscriptionsProvisioningStatus FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			string? status = default;
			string? json = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("status")) {
				status = reader.GetString("status");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new BillingSubscriptionsProvisioningStatus(
				Uuid: uuid,
				Status: status,
				Json: json
				);
		}

		[JsonIgnore]
		public JObject? JsonObject
		{
			get {
				if (string.IsNullOrWhiteSpace(Json))
					return null;
				return JsonConvert.DeserializeObject(Json, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
			}
		}

		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("billing-subscriptions-provisioning-status")) {
				Log.Debug($"----- Table \"billing-subscriptions-provisioning-status\" exists.");
			} else {
				Log.Information($"----- Table \"billing-subscriptions-provisioning-status\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-subscriptions-provisioning-status"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""status"" character varying(255) NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_subscriptions_provisioning_status_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				// None
			}





		}















	}
}
