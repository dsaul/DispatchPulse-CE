using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Serilog;

namespace SharedCode.Hubs
{
	public static class Konstants
	{
		public static string? SIGNAL_R_HUB_URI_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("SIGNAL_R_HUB_URI_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SIGNAL_R_HUB_URI_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? SIGNAL_R_HUB_URI
		{
			get {
				string? path = SIGNAL_R_HUB_URI_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;

				return File.ReadAllText(path);
			}
		}

		public static string? ARI_AND_API_SHARED_SECRET_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("ARI_AND_API_SHARED_SECRET_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("ARI_AND_API_SHARED_SECRET_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? ARI_AND_API_SHARED_SECRET
		{
			get {
				if (string.IsNullOrWhiteSpace(ARI_AND_API_SHARED_SECRET_FILE))
					return null;
				return File.ReadAllText(ARI_AND_API_SHARED_SECRET_FILE);
			}
		}

		public static string? SQUARE_PAYMENTS_AND_API_SHARED_SECRET_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("SQUARE_PAYMENTS_AND_API_SHARED_SECRET_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SQUARE_PAYMENTS_AND_API_SHARED_SECRET_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? SQUARE_PAYMENTS_AND_API_SHARED_SECRET
		{
			get {
				if (string.IsNullOrWhiteSpace(SQUARE_PAYMENTS_AND_API_SHARED_SECRET_FILE))
					return null;
				return File.ReadAllText(SQUARE_PAYMENTS_AND_API_SHARED_SECRET_FILE);
			}
		}

	}
}
