using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace API.Utility
{
	public static class GetIP
	{
		public static string GetRequestIP(HttpContext httpContext, bool tryUseXForwardHeader = true)
		{
			if (httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			string? ip = null;

			// todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

			// X-Forwarded-For (csv list):  Using the First entry in the list seems to work
			// for 99% of cases however it has been suggested that a better (although tedious)
			// approach might be to read each IP from right to left and use the first public IP.
			// http://stackoverflow.com/a/43554000/538763
			//
			if (tryUseXForwardHeader)
			{

				ip = SplitCsv(GetHeaderValueAs<string>(httpContext, "X-Forwarded-For")).FirstOrDefault();
			}

			// RemoteIpAddress is always null in DNX RC1 Update1 (bug).
			if (string.IsNullOrWhiteSpace(ip) && httpContext.Connection?.RemoteIpAddress != null)
				ip = httpContext.Connection.RemoteIpAddress.ToString();

			if (string.IsNullOrWhiteSpace(ip))
				ip = GetHeaderValueAs<string>(httpContext, "REMOTE_ADDR");

			// _httpContextAccessor.HttpContext?.Request?.Host this is the local host.

			if (string.IsNullOrWhiteSpace(ip))
				throw new Exception("Unable to determine caller's IP.");

			return ip;
		}

		public static T? GetHeaderValueAs<T>(HttpContext httpContext, string headerName)
		{
			if (httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			if (httpContext.Request == null)
				return default;

			if (httpContext.Request.Headers == null)
				return default;

			StringValues values;

			if (httpContext.Request.Headers.TryGetValue(headerName, out values))
			{
				string rawValues = values.ToString();   // writes out as Csv when there are multiple.

				if (!string.IsNullOrWhiteSpace(rawValues))
					return (T)Convert.ChangeType(values.ToString(), typeof(T), Culture.DevelopmentCulture);
			}
			return default;
		}

		public static List<string> SplitCsv(this string? csvList)
		{
			if (string.IsNullOrWhiteSpace(csvList))
				return new List<string>();

			return csvList
				.TrimEnd(',')
				.Split(',')
				.AsEnumerable<string>()
				.Select(s => s.Trim())
				.ToList();
		}
	}
}
