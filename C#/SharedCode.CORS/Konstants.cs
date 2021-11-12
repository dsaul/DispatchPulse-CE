﻿using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharedCode.CORS
{
    public static class Konstants
    {
		public static string? CORS_ORIGINS_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("CORS_ORIGINS_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("string.IsNullOrWhiteSpace(CORS_ORIGINS_FILE)");
					return null;
				}
				return str;
			}
		}

		public static IEnumerable<string>? CORS_ORIGINS
		{
			get {
				string? path = CORS_ORIGINS_FILE;
				if (string.IsNullOrWhiteSpace(path)) {
					Log.Error("string.IsNullOrWhiteSpace(CORS_ORIGINS_FILE)");
					yield break;
				}
				string? str = File.ReadAllText(path);
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("CORS_ORIGINS_FILE file empty");
					yield break;
				}

				JArray origins = JArray.Parse(str);
				foreach (JToken jToken in origins) {
					yield return jToken.ToString();
				}

			}
		}
	}
}
