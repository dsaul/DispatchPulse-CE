using System;
using System.Collections.Generic;
using System.Text;
using AsterNET.FastAGI;
using Npgsql;
using Newtonsoft.Json.Linq;
using SharedCode;
using Databases.Records.CRM;
using Amazon.Polly;
using System.Globalization;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void AgentOverview(
			AGIRequest request,
			AGIChannel channel,
			RequestData data) {
			PlayTTS("Here is an overview for today.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

			if (null == data.Subscription || string.IsNullOrWhiteSpace(data.Subscription.ProvisionedDatabaseName)) {
				PlayTTS("There was an error while reading the database, please try again later. Code 98a", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			if (null == data.Agent) {
				PlayTTS("There was an error while reading the database, please try again later. Code 232", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			if (null == data.DPDB) {
				data.ConnectToDPDBName(data.Subscription.ProvisionedDatabaseName);
			}
			if (null == data.DPDB) {
				PlayTTS("There was an error while reading the database, please try again later. Code 233", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}


			// Get all assignment statuses that are open.
			data.OpenAssignmentStatusIds = new StringBuilder();
			data.OpenAssignmentStatuses.RemoveAll((obj) => true);
			{

				var resOpenStatusIds = AssignmentStatus.ForIsOpen(data.DPDB, true);
				data.OpenAssignmentStatuses.AddRange(resOpenStatusIds.Values);


				for (int i = 0; i < data.OpenAssignmentStatuses.Count; i++) {
					if (i != 0)
						data.OpenAssignmentStatusIds.Append(',');

					AssignmentStatus status = data.OpenAssignmentStatuses[i];
					data.OpenAssignmentStatusIds.Append('\'');
					data.OpenAssignmentStatusIds.Append(status.Id);
					data.OpenAssignmentStatusIds.Append('\'');
				}
			}


			// Get all open assignments for the current agent.
			data.AllOpenAssignments.RemoveAll((obj) => true);
			{
				string sql = "SELECT * from \"assignments\" WHERE (json ->> 'statusId') IN "+
					$"({data.OpenAssignmentStatusIds}) AND " +
					$"concat_ws('','',json->> 'agentIds',json->> 'agentId') ILIKE '%{data.Agent.Id}%'";
				using NpgsqlCommand cmd = new NpgsqlCommand(sql, data.DPDB); // OK as phone digit inputs only, also inputs are guids.
				using NpgsqlDataReader reader = cmd.ExecuteReader();

				if (reader.HasRows) {
					while (reader.Read()) {
						Assignments assignment = Assignments.FromDataReader(reader);
						data.AllOpenAssignments.Add(assignment);
					}
				}

			}

			data.AssignmentsTodayAndInThePast.RemoveAll((obj) => true);
			data.ScheduledAssignments.RemoveAll((obj) => true);
			data.UnscheduledAssignments.RemoveAll((obj) => true);

			foreach (Assignments assignment in data.AllOpenAssignments) {

				assignment.GetSchedule(
					data.DPDB,
					out bool? usingProjectSchedule,
					out bool? hasStartISO8601,
					out string? startTimeMode,
					out string? startISO8601,
					out bool? hasEndISO8601,
					out string? endTimeMode,
					out string? endISO8601
					);




				JObject? json = assignment.JsonObject;
				if (null == json)
					continue;

				if (null != hasStartISO8601 && !hasStartISO8601.Value) {
					data.AssignmentsTodayAndInThePast.Add(assignment);
					data.UnscheduledAssignments.Add(assignment);
					continue;
				}

				string nowISO8601 = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
				if (null == startISO8601) {
					data.AssignmentsTodayAndInThePast.Add(assignment);
					data.UnscheduledAssignments.Add(assignment);
					continue;
				}

				int compare = ISO8601Compare.Compare(nowISO8601, startISO8601);
				if (compare < 0) { // now is earlier than startISO8601
					continue;
				}

				data.AssignmentsTodayAndInThePast.Add(assignment);
				data.ScheduledAssignments.Add(assignment);
			}

			StringBuilder stringBuilder = new StringBuilder();

			if (data.ScheduledAssignments.Count == 0 && data.UnscheduledAssignments.Count == 0) {
				stringBuilder.Append("You have no assignments scheduled for today.");
			} else {
				stringBuilder.Append($"You have {data.ScheduledAssignments.Count} scheduled assignment{(data.ScheduledAssignments.Count == 1 ? "" : "s")}, ");
				stringBuilder.Append($"and {data.UnscheduledAssignments.Count} unscheduled assignment{(data.UnscheduledAssignments.Count == 1 ? "" : "s")} today. ");
			}

			PlayTTS(stringBuilder.ToString(), escapeAllKeys, Engine.Neural, VoiceId.Brian);

			AgentMenu(request, channel, data);
		}
	}
}
