using System;

using System.IO;
using Twilio.Types;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Serilog;

namespace API
{
	public static class Konstants {

		



		public static string? ASPNETCORE_ENVIRONMENT
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("ASPNETCORE_ENVIRONMENT IsNullOrWhiteSpace");
					return null;
				}



				return str;
			}
		}

		

		public const string kAppBaseURINotSetErrorMessage = "(set APP_BASE_URI_FILE in deployment)";



		public static string? TWILIO_ACCOUNT_SID_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("TWILIO_ACCOUNT_SID_FILE IsNullOrWhiteSpace");
					return null;
				}
				return str;
			}
		}

		public static string? TWILIO_ACCOUNT_SID
		{
			get
			{
				string? e = TWILIO_ACCOUNT_SID_FILE;
				if (e == null)
					return default;

				return File.ReadAllText(e);
			}
		}

		public static string? TWILIO_AUTH_TOKEN_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("TWILIO_AUTH_TOKEN_FILE IsNullOrWhiteSpace");
					return null;
				}
				return str;
			}
		}

		public static string? TWILIO_AUTH_TOKEN
		{
			get
			{
				string? e = TWILIO_AUTH_TOKEN_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}


		public static string? DISPATCH_PULSE_SMS_FROM_E164_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("DISPATCH_PULSE_SMS_FROM_E164_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("DISPATCH_PULSE_SMS_FROM_E164_FILE IsNullOrWhiteSpace");
					return null;
				}
				return str;
			}
		}

		public static string? DISPATCH_PULSE_SMS_FROM_E164
		{
			get
			{
				string? e = DISPATCH_PULSE_SMS_FROM_E164_FILE;
				if (e == null)
					return default;

				return File.ReadAllText(e);
			}
		}


		









		public static Guid KEmployeeGroupId { get; } = Guid.Parse("458369a0-774c-11ea-9784-02420a0000d8");
		public static Guid KCompanyOwnerGroupId { get; } = Guid.Parse("4574c8d2-774c-11ea-9784-02420a0000d8");
		public static string KStripeAPIKey { get; } = "sk_live_V1j2vJGan0Fd7gxDoI66Wsd000qMr37txn";
		//public static string KStripeAPIKey { get; } = "sk_test_zSJUKMxICBehjiKfhXWn6WbU00U78aQkP7";

		

		
		public static CultureInfo KDefaultCulture { get { return CultureInfo.CreateSpecificCulture("en-CA"); } }



		public static PhoneNumber KDefaultTextPhoneNumber
		{
			get {
				return new Twilio.Types.PhoneNumber(DISPATCH_PULSE_SMS_FROM_E164);
			}
		}

		

		

		

		

	}
}
