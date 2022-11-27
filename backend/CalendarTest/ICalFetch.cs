using System;
using System.Net;
using Ical.Net;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.CalendarComponents;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode.Cal;

namespace CalendarTest
{
	
	public static class ICalFetch
	{
		public static async Task<(HashSet<CalendarOnCallPhoneNumber>, HashSet<string>)> GetOnCallScheduleFromICal(Uri iCalURI) {

			using WebClient wc = new WebClient();

			string contents = await wc.DownloadStringTaskAsync(iCalURI);

			var calendar = Calendar.Load(contents);

			return (
				CalendarUtils.CalendarOnCallPhoneNumbersRightNow(calendar),
				CalendarUtils.CalendarOnCallEMailsRightNow(calendar)
				);

		}
	}
}
