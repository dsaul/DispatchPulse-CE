using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class ISO8601Compare
	{
		public static int Compare(string iso1, string iso2)
		{
			DateTime dt1 = DateTime.Parse(iso1, Konstants.KDefaultCulture);
			DateTime dt2 = DateTime.Parse(iso2, Konstants.KDefaultCulture);

			return dt1.CompareTo(dt2);
		}

	}
}
