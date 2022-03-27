using System;
using System.Collections.Generic;
using System.Text;
using Databases.Records.CRM;
using Databases.Records.Billing;
using Npgsql;

namespace ARI.IVR.CompanyAccess
{
	public class RequestData : IDisposable
	{
		public string? CompanyPhoneId { get; set; } = null;
		public bool? CompanyIdConfirmed { get; set; } = null;
		public string? AgentPhoneId { get; set; } = null;
		public bool? AgentIdConfirmed { get; set; } = null;
		public string? EnteredPasscode { get; set; } = null;

		public BillingCompanies? Company { get; set; } = null;
		public BillingSubscriptions? Subscription { get; set; } = null;
		public Agents? Agent { get; set; } = null;
		public string? AgentName { get; set; } = null;
		public StringBuilder? OpenAssignmentStatusIds { get; set; } = null;
		public List<AssignmentStatus> OpenAssignmentStatuses { get; } = new List<AssignmentStatus>();
		public List<Assignments> AllOpenAssignments { get; } = new List<Assignments>();
		public List<Assignments> AssignmentsTodayAndInThePast { get; } = new List<Assignments>();
		public List<Assignments> ScheduledAssignments { get; } = new List<Assignments>();
		public List<Assignments> UnscheduledAssignments { get; } = new List<Assignments>();
		public Dictionary<Guid, Labour> AgentActiveLabour { get; } = new Dictionary<Guid, Labour>();

		public NpgsqlConnection? DPDB { get; set; } = null;
		public NpgsqlConnection? BillingDB { get; set; } = null;

		public void ConnectToDPDBName(string dbName) {
			DPDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(dbName));
			DPDB.Open();
		}

		public void ConnectToBillingDB() {
			BillingDB = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
			BillingDB.Open();
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

		~RequestData() {
			Dispose();
		}
	}
}
