using Databases.Records.CRM;
using Npgsql;
using SharedCode.Databases.Records.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.Databases.Records
{
	public static class Verification
	{
		public static void RunAllBillingVerifications(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

		}


		public static void RunAllDispatchPulseVerifications(NpgsqlConnection dpDB, bool insertDefaultContents = false) {
			Agents.VerifyRepairTable(dpDB, insertDefaultContents);
			AgentsEmploymentStatus.VerifyRepairTable(dpDB, insertDefaultContents);
			AssignmentStatus.VerifyRepairTable(dpDB, insertDefaultContents);
			Assignments.VerifyRepairTable(dpDB, insertDefaultContents);
			Calendars.VerifyRepairTable(dpDB, insertDefaultContents);
			Companies.VerifyRepairTable(dpDB, insertDefaultContents);
			Contacts.VerifyRepairTable(dpDB, insertDefaultContents);
			DIDs.VerifyRepairTable(dpDB, insertDefaultContents);
			EstimatingManHours.VerifyRepairTable(dpDB, insertDefaultContents);
			Labour.VerifyRepairTable(dpDB, insertDefaultContents);
			LabourSubtypeException.VerifyRepairTable(dpDB, insertDefaultContents);
			LabourSubtypeHolidays.VerifyRepairTable(dpDB, insertDefaultContents);
			LabourSubtypeNonBillable.VerifyRepairTable(dpDB, insertDefaultContents);
			LabourTypes.VerifyRepairTable(dpDB, insertDefaultContents);
			Materials.VerifyRepairTable(dpDB, insertDefaultContents);
			Products.VerifyRepairTable(dpDB, insertDefaultContents);
			ProjectNotes.VerifyRepairTable(dpDB, insertDefaultContents);
			ProjectStatus.VerifyRepairTable(dpDB, insertDefaultContents);
			Projects.VerifyRepairTable(dpDB, insertDefaultContents);
			SettingsDefault.VerifyRepairTable(dpDB, insertDefaultContents);
			SettingsProvisioning.VerifyRepairTable(dpDB, insertDefaultContents);
			SettingsUser.VerifyRepairTable(dpDB, insertDefaultContents);
			Skills.VerifyRepairTable(dpDB, insertDefaultContents);
			OnCallAutoAttendants.VerifyRepairTable(dpDB, insertDefaultContents);
			Voicemails.VerifyRepairTable(dpDB, insertDefaultContents);
			Recordings.VerifyRepairTable(dpDB, insertDefaultContents);
		}
	}
}
