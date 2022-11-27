using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SquarePayments
{
	public static class Konstants
	{
		

		public static string? PADFORM_URI_DOCX_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PADFORM_URI_DOCX_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PADFORM_URI_DOCX_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? PADFORM_URI_DOCX
		{
			get {
				string? path = PADFORM_URI_DOCX_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}

		public static string? PADFORM_URI_PDF_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PADFORM_URI_PDF_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PADFORM_URI_PDF_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? PADFORM_URI_PDF
		{
			get {
				string? path = PADFORM_URI_PDF_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}
	}
}
