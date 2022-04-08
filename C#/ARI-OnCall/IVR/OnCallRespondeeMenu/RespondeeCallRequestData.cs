using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARI.IVR.OnCallRespondee
{
	public class RespondeeCallRequestData : IDisposable
	{
		public Guid? BillingCompanyId { get; set; } = null;
		public Guid? VoicemailId { get; set; } = null;
		public string? DatabaseName { get; set; } = null;
		public string? CallWasTo { get; set; } = null;
		public BillingCompanies? BillingCompany { get; set; } = null;
		public Voicemails? Message { get; set; } = null;
		public NpgsqlConnection? DPDB { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;

		public void ConnectToDPDBName(string dbName) {
			if (DPDB == null) {
				DPDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(dbName));
				DPDB.Open();
			}
		}

		public void ConnectToBillingDB() {
			if (BillingDB == null) {
				BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
				BillingDB.Open();
			}

		}


		public void Dispose() {

			if (null != DPDB) {
				DPDB.Dispose();
				DPDB = null;
			}

			if (null != BillingDB) {
				BillingDB.Dispose();
				BillingDB = null;
			}

		}

		~RespondeeCallRequestData() {
			Dispose();
		}



		public void AddTimelineEntry(OnCall.LeaveMessageRequestData.TimelineType type, string timestampISO8601, string description, string colour) {
			if (null == Message) {
				return;
			}

			if (null == Message.JsonObject) {
				return;
			}

			if (null == DPDB) {
				return;
			}

			if (null == VoicemailId) {
				return;
			}

			JObject? json = Message.JsonObject.DeepClone() as JObject;
			if (null == json)
				return;

			JObject entry = new JObject {
				[Voicemails.kJsonKeyTimelineKeyType] = type.ToString(),
				[Voicemails.kJsonKeyTimelineKeyTimestampISO8601] = timestampISO8601,
				[Voicemails.kJsonKeyTimelineKeyDescription] = description,
				[Voicemails.kJsonKeyTimelineKeyColour] = colour,
			};

			JArray? timeline = json[Voicemails.kJsonKeyTimeline] as JArray;
			if (null != timeline) {
				timeline.Add(entry);
			}

			Message = Message with {
				Json = json.ToString(Formatting.Indented)
			};

			Voicemails.Upsert(DPDB, new Dictionary<Guid, Voicemails> {
					{ VoicemailId.Value, Message }
				}, out _, out _);

		}














	}
}
