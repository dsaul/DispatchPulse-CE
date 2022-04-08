using System;
using AsterNET.FastAGI;
using System.Linq;
using Afk.ZoneInfo;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void AssignmentDetail(
			AGIRequest request, 
			AGIChannel channel,
			RequestData data, 
			Assignments assignment
			) {
			if (null == data.Subscription || string.IsNullOrWhiteSpace(data.Subscription.ProvisionedDatabaseName)) {
				PlayTTS("There was an error while reading the database, please try again later. Code 731", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			if (null == data.DPDB) {
				data.ConnectToDPDBName(data.Subscription.ProvisionedDatabaseName);
			}
			if (null == data.DPDB) {
				PlayTTS("There was an error while reading the database, please try again later. Code 233", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

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

			int repeatCount = 0;
			while (true) {


				if (repeatCount == 3) {
					throw new PerformHangupException();
				}

				char key = '\0';

				if (key == '\0') {
					key = PlayAssignmentProjectName(request, channel, data, assignment);
				}

				if (key == '\0' && !string.IsNullOrWhiteSpace(startISO8601) && null != hasStartISO8601 && hasStartISO8601.Value) {

					DateTime startISO = DateTime.Parse(startISO8601, null, System.Globalization.DateTimeStyles.RoundtripKind);

					if (null == data.Company) {
						ThrowError(request, "2n32", "null == data.Company");
					}
					if (string.IsNullOrWhiteSpace(data.Company.IANATimezone)) {
						ThrowError(request, "2n33", "string.IsNullOrWhiteSpace(data.Company.IANATimezone)");
					}

					TzTimeZone zoneWpg = TzTimeInfo.GetZone(data.Company.IANATimezone);
					DateTime startISOLocal = zoneWpg.ToLocalTime(startISO);

					switch (startTimeMode) {
						case "none":
							break;
						case "morning-first-thing":
							key = PlayTTS($"The scheduled start time is {startISOLocal:MMMM d} first thing in the morning. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
							break;
						case "morning-second-thing":
							key = PlayTTS($"The scheduled start time is {startISOLocal:MMMM d} second thing in the morning. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
							break;
						case "afternoon-first-thing":
							key = PlayTTS($"The scheduled start time is {startISOLocal:MMMM d} first thing in the afternoon. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
							break;
						case "afternoon-second-thing":
							key = PlayTTS($"The scheduled start time is {startISOLocal:MMMM d} second thing in the afternoon. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
							break;
						case "time":
							if (startISOLocal.Minute == 0) {
								key = PlayTTS($"The scheduled start time is {startISOLocal:MMMM d} at {startISOLocal:H tt}", escapeAllKeys, Engine.Neural, VoiceId.Brian);
							} else {
								key = PlayTTS($"The scheduled start time is {startISOLocal:MMMM d} at {startISOLocal:H mm tt}", escapeAllKeys, Engine.Neural, VoiceId.Brian);
							}

							break;
					}

				}

				// Filter to this assignment
				var filtered = (from k in data.AgentActiveLabour
								where k.Value.AssignmentId != null && k.Value.AssignmentId.Value == assignment.Id
								select k).ToList();


				if (filtered.Count > 0) {


					Labour labour = filtered.FirstOrDefault().Value;


					switch (labour.LocationType) {
						case Labour.LocationTypeE.Travel:
							
							if (key == '\0') {
								key = PlayTTS(
									"Press 1 for the work requested. " +
									"Press 2 to hear a list of the addresses. " +
									"Press 3 to list any phone numbers listed on the account. " +
									"Press 4 if you have arrived on site and want to switch to tracking time on-site. " +
									"Press 6 to save and end this work timer. " +
									"Press 7 to cancel and delete this work timer. " +
									"Press 9 to save this work and close the assignment. " +
									"Press 0 to list the other companies working at this site. " +
									"Press pound leave this menu, or press star to hear this menu again. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
								if (key == '\0') {
									key = WaitForDigit(5000);
								}
							}
							

							switch (key) {
								case '1':
									PlayAssignmentWorkRequested(request, channel, data, assignment);
									continue;
								case '2':
									PlayAssignmentProjectAddressList(request, channel, data, assignment);
									continue;
								case '3':
									PlayAssignmentProjectContactsPhoneNumberList(request, channel, data, assignment);
									continue;
								case '4':
									CompleteTravelAndBeginWorkOnSite(request, channel, data, labour);
									continue;
								case '6':
									SaveAndEndThisWorkTimer(request, channel, data, labour);
									continue;
								case '7':
									CancelAndDeleteThisWorkTimer(request, channel, data, labour);
									continue;
								case '9':
									SaveThisWorkTimerAndCompleteTheAssignment(request, channel, data, labour);
									continue;
								case '0':
									PlayAssignmentOtherCompanies(request, channel, data, assignment);
									continue;
								case '#':
									return;
								case '*':
									continue;
							}



							break;
						case Labour.LocationTypeE.Remote:
						case Labour.LocationTypeE.OnSite:
							if (key == '\0') {
								key = PlayTTS(
									"Press 1 for the work requested. " +
									"Press 2 to hear a list of the addresses. " +
									"Press 3 to list any phone numbers listed on the account. " +
									"Press 6 to save and end this work timer. " +
									"Press 7 to cancel and delete this work timer. " +
									"Press 9 to save this work and close the assignment." +
									"Press 0 to list the other companies working at this site.. " +
									"Press pound leave this menu, or press star to hear this menu again. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
								if (key == '\0') {
									key = WaitForDigit(5000);
								}
							}
							

							switch (key) {
								case '1':
									PlayAssignmentWorkRequested(request, channel, data, assignment);
									continue;
								case '2':
									PlayAssignmentProjectAddressList(request, channel, data, assignment);
									continue;
								case '3':
									PlayAssignmentProjectContactsPhoneNumberList(request, channel, data, assignment);
									continue;
								case '6':
									SaveAndEndThisWorkTimer(request, channel, data, labour);
									continue;
								case '7':
									CancelAndDeleteThisWorkTimer(request, channel, data, labour);
									continue;
								case '9':
									SaveThisWorkTimerAndCompleteTheAssignment(request, channel, data, labour);
									continue;
								case '0':
									PlayAssignmentOtherCompanies(request, channel, data, assignment);
									continue;
								case '#':
									return;
								case '*':
									continue;
							}


							break;
					}

					






				} else if (filtered.Count == 0) {
					if (key == '\0') {
						key = PlayTTS(
							"Press 1 for the work requested. " +
							"Press 2 to hear a list of the addresses. " +
							"Press 3 to list any phone numbers listed on the account. " +
							"Press 4 to mark yourself as travelling to this assignment. " +
							"Press 5 to mark yourself as working remotely on this assignment. " +
							"Press 6 to mark yourself as working on-site at this assignment. " +
							"Press 0 to list the other companies working at this site. " +
							"Press pound leave this menu, or press star to hear this menu again. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
						if (key == '\0') {
							key = WaitForDigit(5000);
						}
					}
						

					switch (key) {
						case '1':
							PlayAssignmentWorkRequested(request, channel, data, assignment);
							continue;
						case '2':
							PlayAssignmentProjectAddressList(request, channel, data, assignment);
							continue;
						case '3':
							PlayAssignmentProjectContactsPhoneNumberList(request, channel, data, assignment);
							continue;
						case '4':
							MarkAssignmentAsTravelling(request, channel, data, assignment);
							continue;
						case '5':
							MarkAssignmentAsWorkingRemotely(request, channel, data, assignment);
							continue;
						case '6':
							MarkAssignmentAsWorkingOnSite(request, channel, data, assignment);
							continue;
						case '0':
							PlayAssignmentOtherCompanies(request, channel, data, assignment);
							continue;
						case '#':
							return;
						case '*':
							continue;
					}
				}

				


				PlayTTS("I didn't get a response, please try again.", "", Engine.Neural, VoiceId.Brian);
				repeatCount++;
			}




		}
	}
}
