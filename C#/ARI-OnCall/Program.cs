using System;
using AsterNET.Manager;
using AsterNET.Manager.Action;
using AsterNET.Manager.Response;
using AsterNET.FastAGI;
using AsterNET.Manager.Event;
using AsterNET.FastAGI.MappingStrategies;
using System.Collections.Generic;
using Npgsql;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Threading;
using Twilio;
using Serilog;
using FluentEmail.Core;
using FluentEmail.Smtp;
using FluentEmail.Razor;
using System.Net.Mail;
using System.Net;
using SharedCode;

namespace ARI.IVR.OnCall
{
	class Program
	{
		

		public static string? RECORDINGS_DIRECTORY
		{
			get {
				string? str = Environment.GetEnvironmentVariable("RECORDINGS_DIRECTORY");
				if (!string.IsNullOrWhiteSpace(str)) {
					return str;
				}
				return null;
			}
		}

		


		public static HubConnection? SignalRConnection { get; set; } = default;


		static async Task Main()
		{
			
			Log.Logger = new LoggerConfiguration()
				.Enrich.WithMachineName()
				.Enrich.FromLogContext()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.WithMachineName()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.CreateLogger();


			Log.Information("Ari-OnCall (c) 2021 Dan Saul");
			
			if (string.IsNullOrWhiteSpace(EnvOnCallResponder.ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS)) {
				Log.Error("ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS_FILE empty or missing.");
				return;
			}
			if (string.IsNullOrWhiteSpace(EnvOnCallResponder.ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI)) {
				Log.Error("ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI_FILE empty or missing.");
				return;
			}


			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)) {
				Log.Error("ARI_AND_API_SHARED_SECRET_FILE not set!");
				return;
			}


			if (string.IsNullOrWhiteSpace(RECORDINGS_DIRECTORY)) {
				Log.Error("RECORDINGS_DIRECTORY not set!");
				return;
			}

			

			if (string.IsNullOrWhiteSpace(EnvTwilio.TWILIO_AUTH_TOKEN)) {
				Log.Error("TWILIO_AUTH_TOKEN_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(EnvTwilio.TWILIO_ACCOUNT_SID)) {
				Log.Error("TWILIO_ACCOUNT_SID_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_HOST_FQDN)) {
				Log.Error("SMTP_HOST_FQDN_FILE not set!");
				return;
			}

			if (null == EnvEmail.SMTP_HOST_PORT) {
				Log.Error("SMTP_HOST_PORT_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_USERNAME)) {
				Log.Error("SMTP_USERNAME_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(EnvEmail.SMTP_PASSWORD)) {
				Log.Error("SMTP_PASSWORD_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_ACCESS_KEY)) {
				Log.Error("S3_PBX_ACCESS_KEY_FILE not set!");
				return;
			}
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_SECRET_KEY)) {
				Log.Error("S3_PBX_SECRET_KEY_FILE not set!");
				return;
			}
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_SERVICE_URI)) {
				Log.Error("S3_PBX_SERVICE_URI_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI)) {
				Log.Error("SIGNAL_R_HUB_URI_FILE not set!");
				return;
			}


			Email.DefaultSender = new SmtpSender(() => new SmtpClient(EnvEmail.SMTP_HOST_FQDN, EnvEmail.SMTP_HOST_PORT.Value) {

				DeliveryMethod = SmtpDeliveryMethod.Network,
				Credentials = new NetworkCredential(EnvEmail.SMTP_USERNAME, EnvEmail.SMTP_PASSWORD)
			});
			Email.DefaultRenderer = new RazorRenderer();
			Log.Information("SMTP Client Initiated {SMTPUsername}:********@{SMTPHostFQDN}:{SMTPHostPort}",
				EnvEmail.SMTP_USERNAME, EnvEmail.SMTP_HOST_FQDN, EnvEmail.SMTP_HOST_PORT.Value);

			TwilioClient.Init(EnvTwilio.TWILIO_ACCOUNT_SID, EnvTwilio.TWILIO_AUTH_TOKEN);
			Log.Information("Twilio Client [{TwilioAccountSid}]", EnvTwilio.TWILIO_ACCOUNT_SID);

			SignalRConnection = new HubConnectionBuilder()
				.WithUrl(SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI)
				.Build();	

			SignalRConnection.Closed += async (error) => {
				await Task.Delay(new Random().Next(0, 5) * 1000);
				await SignalRConnection.StartAsync();
			};

			await SignalRConnection.StartAsync();
			Log.Information("SignalR API Connection API URI {ApiURI}", SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI);

			_ = Task.Run(() => {
				while (true) {
					OnCallPostMessageHandler.Run();
					Thread.Sleep(1000*10);
				}
			});
			Log.Information("OnCallPostMessageHandler Start.");




			NpgsqlConnection.GlobalTypeMapper.UseJsonNet();

			
			AsteriskFastAGI agi = new ();
			// Remove the lines below to enable the default (resource based) MappingStrategy
			// You can use an XML file with XmlMappingStrategy, or simply pass in a list of
			// ScriptMapping. 
			// If you wish to save it to a file, use ScriptMapping.SaveMappings and pass in a path.
			// This can then be used to load the mappings without having to change the source code!

			agi.MappingStrategy = new GeneralMappingStrategy(new List<ScriptMapping>()
			{
				new ScriptMapping() {
					ScriptClass = "ARI.IVR.OnCall.EntryPoint",
					ScriptName = "OnCall"
				},
				new ScriptMapping() {
					ScriptClass = "ARI.IVR.OnCallRespondee.EntryPoint",
					ScriptName = "RespondeeMenu"
				}
			}); 

			agi.SC511_CAUSES_EXCEPTION = true;
			agi.SCHANGUP_CAUSES_EXCEPTION = true;


			Log.Information(@"Ready for calls.");

			agi.Start();

			Log.CloseAndFlush();
		}
	}
}
