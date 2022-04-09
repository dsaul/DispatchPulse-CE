using FluentEmail.Core;
using FluentEmail.Smtp;
using FluentEmail.Razor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog.Events;
using SharedCode;

namespace SquarePayments
{
	public class Program
	{
		public static HubConnection? SignalRConnection { get; set; } = default;

		public static async Task Main(string[] args)
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

			
			if (string.IsNullOrWhiteSpace(SharedCode.Konstants.ACCOUNTS_RECEIVABLE_EMAIL)) {
				Log.Error("ACCOUNTS_RECEIVABLE_EMAIL_FILE not set!");
				return;
			}
			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_HOST_FQDN)) {
				Log.Error("SMTP_HOST_FQDN_FILE not set!");
				return;
			}
			Log.Debug("SMTP_HOST_FQDN:{SMTP_HOST_FQDN}", EnvEmail.SMTP_HOST_FQDN);

			if (null == EnvEmail.SMTP_HOST_PORT) {
				Log.Error("SMTP_HOST_PORT_FILE not set!");
				return;
			}
			Log.Debug("SMTP_HOST_PORT:{SMTP_HOST_PORT}", EnvEmail.SMTP_HOST_PORT);

			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_USERNAME)) {
				Log.Error("SMTP_USERNAME_FILE not set!");
				return;
			}
			Log.Debug("SMTP_USERNAME:{SMTP_USERNAME}", EnvEmail.SMTP_USERNAME);

			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_PASSWORD)) {
				Log.Error("SMTP_PASSWORD_FILE not set!");
				return;
			}
			Log.Debug("SMTP_PASSWORD_LENGTH:{SMTP_PASSWORD_LENGTH}", EnvEmail.SMTP_PASSWORD.Length);

			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY)) {
				Log.Error("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE not set!");
				return;
			}
			//Log.Debug("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE:{S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE}", EnvAmazonS3.S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY);

			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY)) {
				Log.Error("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE not set!");
				return;
			}
			//Log.Debug("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY:{S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY}", EnvAmazonS3.S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY);

			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.SQUARE_PAYMENTS_AND_API_SHARED_SECRET)) {
				Log.Error("SQUARE_PAYMENTS_AND_API_SHARED_SECRET_FILE not set!");
				return;
			}
			//Log.Debug("SQUARE_PAYMENTS_AND_API_SHARED_SECRET:{SQUARE_PAYMENTS_AND_API_SHARED_SECRET}", SharedCode.Hubs.Konstants.SQUARE_PAYMENTS_AND_API_SHARED_SECRET);

			if (string.IsNullOrWhiteSpace(EnvSquare.SQUARE_PRODUCTION_ACCESS_TOKEN)) {
				Log.Error("SQUARE_PRODUCTION_ACCESS_TOKEN not set!");
				return;
			}
			//Log.Debug("SQUARE_PRODUCTION_ACCESS_TOKEN:{SQUARE_PRODUCTION_ACCESS_TOKEN}", SharedCode.Square.Konstants.SQUARE_PRODUCTION_ACCESS_TOKEN);

			if (string.IsNullOrWhiteSpace(EnvSquare.SQUARE_SANDBOX_ACCESS_TOKEN)) {
				Log.Error("SQUARE_SANDBOX_ACCESS_TOKEN not set!");
				return;
			}
			//Log.Debug("SQUARE_SANDBOX_ACCESS_TOKEN:{SQUARE_SANDBOX_ACCESS_TOKEN}", SharedCode.Square.Konstants.SQUARE_SANDBOX_ACCESS_TOKEN);

			Email.DefaultSender = new SmtpSender(() => new SmtpClient(EnvEmail.SMTP_HOST_FQDN, EnvEmail.SMTP_HOST_PORT.Value) {

				DeliveryMethod = SmtpDeliveryMethod.Network,
				Credentials = new NetworkCredential(EnvEmail.SMTP_USERNAME, EnvEmail.SMTP_PASSWORD)
			});
			Email.DefaultRenderer = new RazorRenderer();
			Log.Information("SMTP Client Initiated {SMTPUsername}:********@{SMTPHostFQDN}:{SMTPHostPort}",
				EnvEmail.SMTP_USERNAME, EnvEmail.SMTP_HOST_FQDN, EnvEmail.SMTP_HOST_PORT.Value);




			SignalRConnection = new HubConnectionBuilder()
				.WithUrl(SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI)
				.Build();

			SignalRConnection.Closed += async (error) => {
				await Task.Delay(new Random().Next(0, 5) * 1000);
				await SignalRConnection.StartAsync();
			};

			await SignalRConnection.StartAsync();
			Log.Information("SignalR API Connection API URI {ApiURI}", SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI);







			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
