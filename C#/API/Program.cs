using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using SharedCode;
using Microsoft.Extensions.DependencyInjection;
using SharedCode.S3;
using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using System.Net.Mail;
using System.Net;
using Serilog;
using Serilog.Events;
using SharedCode;

namespace API
{
	public class Program
	{
		


		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.Enrich.WithMachineName()
				.Enrich.FromLogContext()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.WithMachineName()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
				.WriteTo.Console()
				//.WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), SERILOG_LOG_FILE)
				.CreateLogger();

			// Make sure environment variables are set.
			if (string.IsNullOrWhiteSpace(Konstants.DISPATCH_PULSE_SMS_FROM_E164))
				throw new Exception("DISPATCH_PULSE_SMS_FROM_E164 not set.");
			if (string.IsNullOrWhiteSpace(Konstants.ASPNETCORE_ENVIRONMENT))
				throw new Exception("ASPNETCORE_ENVIRONMENT not set.");
			if (string.IsNullOrWhiteSpace(EnvDatabases.NPGSQL_CONNECTION_STRING))
				throw new Exception("NPGSQL_CONNECTION_STRING not set.");
			if (string.IsNullOrWhiteSpace(EnvDatabases.PGPASSFILE))
				throw new Exception("PGPASSFILE not set.");
			if (string.IsNullOrWhiteSpace(Konstants.TWILIO_ACCOUNT_SID_FILE))
				throw new Exception("TWILIO_ACCOUNT_SID_FILE not set.");
			if (string.IsNullOrWhiteSpace(Konstants.TWILIO_AUTH_TOKEN_FILE))
				throw new Exception("TWILIO_AUTH_TOKEN_PATH not set.");
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_ACCESS_KEY_FILE))
				throw new Exception("S3_PBX_ACCESS_KEY_FILE not set.");
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_SECRET_KEY_FILE))
				throw new Exception("S3_PBX_SECRET_KEY_FILE not set.");
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_ACCESS_KEY))
				throw new Exception("S3_PBX_ACCESS_KEY empty.");
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_SECRET_KEY))
				throw new Exception("S3_PBX_SECRET_KEY empty.");
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_SERVICE_URI))
				throw new Exception("S3_PBX_SERVICE_URI_FILE not set.");
			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.SQUARE_PAYMENTS_AND_API_SHARED_SECRET))
				throw new Exception("SQUARE_PAYMENTS_AND_API_SHARED_SECRET_FILE not set.");
			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_HOST_FQDN))
				throw new Exception("SMTP_HOST_FQDN_FILE not set.");
			if (null == EnvEmail.SMTP_HOST_PORT)
				throw new Exception("SMTP_HOST_PORT_FILE not set.");
			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_USERNAME))
				throw new Exception("SMTP_USERNAME_FILE not set.");
			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_PASSWORD))
				throw new Exception("SMTP_PASSWORD_FILE not set.");

			Email.DefaultSender = new SmtpSender(() => new SmtpClient(EnvEmail.SMTP_HOST_FQDN, EnvEmail.SMTP_HOST_PORT.Value)
			{

				DeliveryMethod = SmtpDeliveryMethod.Network,
				Credentials = new NetworkCredential(EnvEmail.SMTP_USERNAME, EnvEmail.SMTP_PASSWORD)
			});

			Email.DefaultRenderer = new RazorRenderer();

			// SMS
			TwilioClient.Init(Konstants.TWILIO_ACCOUNT_SID, Konstants.TWILIO_AUTH_TOKEN);

			//TextMessageFunnel.SendAPIAdminText("Startup","API has just started.");

			// Web API
			IWebHostBuilder whb = WebHost.CreateDefaultBuilder(args);
			
			whb.UseStartup<Startup>();
			whb.ConfigureLogging((hostingContext, logging) =>
			{
				logging.ClearProviders();
				logging.AddConsole();
				logging.AddDebug();
				logging.AddEventSourceLogger();
			});


			IWebHost wh = whb.Build();

			var logger = wh.Services.GetRequiredService<ILogger<Program>>();
			logger.LogInformation("**********STARTUP**********");

			wh.Run();
		}
	}
}
