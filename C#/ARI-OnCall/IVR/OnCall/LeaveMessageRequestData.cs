using System;
using SharedCode.DatabaseSchemas;
using SharedCode;
using Npgsql;
using Newtonsoft.Json.Linq;

namespace ARI.IVR.OnCall
{
	public class LeaveMessageRequestData : IDisposable {

		public RegisteredPhoneNumbers? RegisteredPhoneNumber { get; set; } = null;
		public string? CallerIdNonDigitsRemoved { get; set; } = null;
		public string? CallerIdNonDigitsRemovedWithSpaces { get; set; } = null;
		public NpgsqlConnection? DPDB { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;
		public BillingCompanies? BillingCompany { get; set; } = null;
		public BillingSubscriptions? Subscription { get; set; } = null;
		public string? DPDatabaseName { get; set; } = null;
		public DIDs? DPDID { get; set; } = null;
		public OnCallAutoAttendants? AutoAttendant { get; set; } = null;
		
		public Guid? OnCallMessageRecordingId { get; set; } = null;
		public string? OnCallMessageRecordingPathAsterisk { get; set; } = null;
		public string? OnCallMessageRecordingPathActual { get; set; } = null;
		


		public JObject Json { get; set; } = new JObject {
			[Voicemails.kJsonKeyType] = null,
			[Voicemails.kJsonKeyOnCallAutoAttendantId] = null,
			[Voicemails.kJsonKeyMessageLeftAtISO8601] = null,
			[Voicemails.kJsonKeyCallerIdNumber] = null,
			[Voicemails.kJsonKeyCallerIdName] = null,
			[Voicemails.kJsonKeyCallbackNumber] = null,
			[Voicemails.kJsonKeyTimeline] = new JArray(),
			[Voicemails.kJsonKeyOnCallAttemptsFinished] = false,
			[Voicemails.kJsonKeyIsMarkedHandled] = false,
			[Voicemails.kJsonKeyNoAgentResponseNotificationNumber] = null,
		};

		public void ConnectToDPDBName(string dbName) {
			if (DPDB == null) {
				DPDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(dbName));
				DPDB.Open();
			}
		}

		public void ConnectToBillingDB() {
			if (BillingDB == null) {
				BillingDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(EnvDatabases.BILLING_DATABASE_NAME));
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

		~LeaveMessageRequestData() {
			Dispose();
		}



		public string? Type
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyType];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			set {
				Json[Voicemails.kJsonKeyType] = value;
			}
		}

		public string? MessageLeftAtISO8601
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyMessageLeftAtISO8601];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			set {
				Json[Voicemails.kJsonKeyMessageLeftAtISO8601] = value;
			}
		}

		public string? CallerIdNumber
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyCallerIdNumber];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			set {
				Json[Voicemails.kJsonKeyCallerIdNumber] = value;
			}
		}

		public string? CallbackNumber
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyCallbackNumber];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			set {
				Json[Voicemails.kJsonKeyCallbackNumber] = value;
			}
		}

		public string? CallerIdName
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyCallerIdName];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			set {
				Json[Voicemails.kJsonKeyCallerIdName] = value;
			}
		}

		public Guid? OnCallAutoAttendantId
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyOnCallAutoAttendantId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
			set {
				Json[Voicemails.kJsonKeyOnCallAutoAttendantId] = value.ToString();
			}
		}


		public string? NoAgentResponseNotificationNumber
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyNoAgentResponseNotificationNumber];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			set {
				Json[Voicemails.kJsonKeyNoAgentResponseNotificationNumber] = value;
			}
		}

		public string? NoAgentResponseNotificationEMail
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyNoAgentResponseNotificationEMail];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			set {
				Json[Voicemails.kJsonKeyNoAgentResponseNotificationEMail] = value;
			}
		}

		public string? MarkedHandledNotificationEMail
		{
			get {
				JToken? tok = Json[Voicemails.kJsonKeyMarkedHandledNotificationEMail];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			set {
				Json[Voicemails.kJsonKeyMarkedHandledNotificationEMail] = value;
			}
		}



		




		public enum TimelineType {
			text
		};

		public void AddTimelineEntry(TimelineType type, string timestampISO8601, string description, string colour) {

			JObject entry = new JObject {
				[Voicemails.kJsonKeyTimelineKeyType] = type.ToString(),
				[Voicemails.kJsonKeyTimelineKeyTimestampISO8601] = timestampISO8601,
				[Voicemails.kJsonKeyTimelineKeyDescription] = description,
				[Voicemails.kJsonKeyTimelineKeyColour] = colour,
			};

			JArray? timeline = Json[Voicemails.kJsonKeyTimeline] as JArray;
			if (null == timeline) {
				timeline = new JArray();
				Json[Voicemails.kJsonKeyTimeline] = timeline;
			}
			if (null != timeline) {
				timeline.Add(entry);
			}


		}


































	}
}
