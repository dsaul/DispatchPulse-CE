using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using Serilog;

namespace SharedCode
{
	public static class Konstants
	{
		public static CultureInfo KDefaultCulture { get { return CultureInfo.CreateSpecificCulture("en-CA"); } }

		public static string? APP_BASE_URI_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("APP_BASE_URI_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("APP_BASE_URI_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? APP_BASE_URI
		{
			get {
				string? path = APP_BASE_URI_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}


		public static string? ACCOUNTS_RECEIVABLE_EMAIL_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("ACCOUNTS_RECEIVABLE_EMAIL_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("ACCOUNTS_RECEIVABLE_EMAIL_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? ACCOUNTS_RECEIVABLE_EMAIL
		{
			get {
				string? path = ACCOUNTS_RECEIVABLE_EMAIL_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}
	}
}
