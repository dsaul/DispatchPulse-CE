﻿using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.S3
{
	public static class Konstants
	{
		public static string? S3_CARD_ON_FILE_BUCKET_NAME_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("S3_CARD_ON_FILE_BUCKET_NAME_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("S3_CARD_ON_FILE_BUCKET_NAME_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_CARD_ON_FILE_BUCKET_NAME
		{
			get {
				string? path = S3_CARD_ON_FILE_BUCKET_NAME_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}





		public static string? S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY
		{
			get {
				string? path = S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}


		public static string? S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY
		{
			get {
				string? path = S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}


		public static string? S3_DISPATCH_PULSE_SERVICE_URI_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("S3_DISPATCH_PULSE_SERVICE_URI_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("S3_DISPATCH_PULSE_SERVICE_URI_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_DISPATCH_PULSE_SERVICE_URI
		{
			get {
				string? path = S3_DISPATCH_PULSE_SERVICE_URI_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}

		public static string? S3_PBX_ACCESS_KEY_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("S3_PBX_ACCESS_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("S3_PBX_ACCESS_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_PBX_ACCESS_KEY
		{
			get {
				string? e = S3_PBX_ACCESS_KEY_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}

		public static string? S3_PBX_SECRET_KEY_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("S3_PBX_SECRET_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("S3_PBX_SECRET_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_PBX_SECRET_KEY
		{
			get {
				string? e = S3_PBX_SECRET_KEY_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}

		public static string? S3_PBX_SERVICE_URI_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("S3_PBX_SERVICE_URI_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("S3_PBX_SERVICE_URI_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_PBX_SERVICE_URI
		{
			get {
				string? e = S3_PBX_SERVICE_URI_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}

	}
}
