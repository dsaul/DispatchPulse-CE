using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using SharedCode.Cal;
using System.Net;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.CalendarComponents;
using System.Linq;

namespace CalendarTest
{
	

	

	class Program
	{
		public static HttpClient HttpClient = new HttpClient();

		public static async Task Main() {

			await TestEventsRightNow();



		}

		public static async Task TestEventsRightNow() {
			// Test Calendar
			// https://calendar.google.com/calendar/ical/c_fcvl0nn9t24frup9nqravce4os%40group.calendar.google.com/private-eabc1be99974776e01e9c0552f6c508d/basic.ics

			Uri primaryUri = new Uri("https://calendar.google.com/calendar/ical/c_fcvl0nn9t24frup9nqravce4os%40group.calendar.google.com/private-eabc1be99974776e01e9c0552f6c508d/basic.ics");
			Uri backupUri = new Uri("https://calendar.google.com/calendar/ical/c_3vh27l90k0fu1tqhb64f1m3pf4%40group.calendar.google.com/private-85a767b5cb1da5032b298356f34f8d17/basic.ics");

			var onCallPrimaryTask = ICalFetch.GetOnCallScheduleFromICal(primaryUri);
			var onCallBackupTask = ICalFetch.GetOnCallScheduleFromICal(backupUri);

			Task.WaitAll(new Task[] {
				onCallPrimaryTask,
				onCallBackupTask
			});

			(HashSet<CalendarOnCallPhoneNumber> numbersPrimary, HashSet<string> emailsPrimary) = onCallPrimaryTask.Result;
			(HashSet<CalendarOnCallPhoneNumber> numbersBackup, HashSet<string> emailsBackup) = onCallBackupTask.Result;

			foreach (CalendarOnCallPhoneNumber num in numbersPrimary) {
				Console.WriteLine($"Primary {num.Description} {num.Number}");
			}
			foreach (string email in emailsPrimary) {
				Console.WriteLine($"Primary {email}");
			}
			foreach (CalendarOnCallPhoneNumber num in numbersBackup) {
				Console.WriteLine($"Backup {num.Description} {num.Number}");
			}
			foreach (string email in emailsBackup) {
				Console.WriteLine($"Backup {email}");
			}
		}

		public static async void OccurancesThisMonth() {
			using WebClient wc = new WebClient();

			Uri primaryUri = new Uri("https://calendar.google.com/calendar/ical/c_fcvl0nn9t24frup9nqravce4os%40group.calendar.google.com/private-eabc1be99974776e01e9c0552f6c508d/basic.ics");
			Uri backupUri = new Uri("https://calendar.google.com/calendar/ical/c_3vh27l90k0fu1tqhb64f1m3pf4%40group.calendar.google.com/private-85a767b5cb1da5032b298356f34f8d17/basic.ics");

			string calStr = await wc.DownloadStringTaskAsync(primaryUri);

			Calendar cal = Calendar.Load(calStr);

			HashSet<Occurrence> occ = CalendarUtils.OccurancesRoughlyAroundThisMonth(cal);
		}


	}
}
