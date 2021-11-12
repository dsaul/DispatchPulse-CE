using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OnCallResponderMessageAccess
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
				.WriteTo.Console()
				//.WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), SERILOG_LOG_FILE)
				.CreateLogger();


			Log.Information("On-Call Responder Message Access (c) 2021 Dan Saul");

			if (string.IsNullOrWhiteSpace(SharedCode.EMail.Konstants.SMTP_HOST_FQDN)) {
				Log.Error("SMTP_HOST_FQDN_FILE not set!");
				return;
			}

			if (null == SharedCode.EMail.Konstants.SMTP_HOST_PORT) {
				Log.Error("SMTP_HOST_PORT_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.EMail.Konstants.SMTP_USERNAME)) {
				Log.Error("SMTP_USERNAME_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.EMail.Konstants.SMTP_PASSWORD)) {
				Log.Error("SMTP_PASSWORD_FILE not set!");
				return;
			}

			Email.DefaultSender = new SmtpSender(() => new SmtpClient(SharedCode.EMail.Konstants.SMTP_HOST_FQDN, SharedCode.EMail.Konstants.SMTP_HOST_PORT.Value) {

				DeliveryMethod = SmtpDeliveryMethod.Network,
				Credentials = new NetworkCredential(SharedCode.EMail.Konstants.SMTP_USERNAME, SharedCode.EMail.Konstants.SMTP_PASSWORD)
			});
			Email.DefaultRenderer = new RazorRenderer();

			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
