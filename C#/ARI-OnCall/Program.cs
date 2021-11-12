using System;
using AsterNET.Manager;
using AsterNET.Manager.Action;
using AsterNET.Manager.Response;
using AsterNET.FastAGI;
using AsterNET.Manager.Event;
using AsterNET.FastAGI.MappingStrategies;
using System.Collections.Generic;
using Npgsql;
using Npgsql.Json.NET;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using API.Hubs;
using System.IO;
using System.Threading;
using Twilio;
using Serilog;
using FluentEmail.Core;
using FluentEmail.Smtp;
using FluentEmail.Razor;
using System.Net.Mail;
using System.Net;
using SharedCode.S3;

namespace ARI.IVR.OnCall
{
	class Program
	{
		

		public static string? PBX_LOCAL_RECORD_FILE_DIRECTORY
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PBX_LOCAL_RECORD_FILE_DIRECTORY");
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
			
			if (string.IsNullOrWhiteSpace(SharedCode.OnCallResponder.Konstants.ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS)) {
				Log.Error("ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS_FILE empty or missing.");
				return;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.OnCallResponder.Konstants.ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI)) {
				Log.Error("ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI_FILE empty or missing.");
				return;
			}


			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)) {
				Log.Error("ARI_AND_API_SHARED_SECRET_FILE not set!");
				return;
			}


			if (string.IsNullOrWhiteSpace(PBX_LOCAL_RECORD_FILE_DIRECTORY)) {
				Log.Error("PBX_LOCAL_RECORD_FILE_DIRECTORY not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)) {
				Log.Error("ARI_TO_PBX_SSH_IDRSA_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_FQDN)) {
				Log.Error("PBX_FQDN not set!");
				return;
			}

			if (null == SharedCode.ARI.Konstants.PBX_SSH_PORT) {
				Log.Error("PBX_SSH_PORT not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_SSH_USER)) {
				Log.Error("PBX_SSH_USER not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY)) {
				Log.Error("PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_OUTGOING_SPOOL_COMPLETED_DIRECTORY)) {
				Log.Error("PBX_LOCAL_OUTGOING_SPOOL_COMPLETED_DIRECTORY not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.Twilio.Konstants.TWILIO_AUTH_TOKEN)) {
				Log.Error("TWILIO_AUTH_TOKEN not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.Twilio.Konstants.TWILIO_ACCOUNT_SID)) {
				Log.Error("TWILIO_ACCOUNT_SID not set!");
				return;
			}

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

			if (string.IsNullOrWhiteSpace(SharedCode.S3.Konstants.S3_PBX_ACCESS_KEY)) {
				Log.Error("S3_PBX_ACCESS_KEY_FILE not set!");
				return;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.S3.Konstants.S3_PBX_SECRET_KEY)) {
				Log.Error("S3_PBX_SECRET_KEY_FILE not set!");
				return;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.S3.Konstants.S3_PBX_SERVICE_URI)) {
				Log.Error("S3_PBX_SERVICE_URI_FILE not set!");
				return;
			}

			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI)) {
				Log.Error("SIGNAL_R_HUB_URI_FILE not set!");
				return;
			}


			Email.DefaultSender = new SmtpSender(() => new SmtpClient(SharedCode.EMail.Konstants.SMTP_HOST_FQDN, SharedCode.EMail.Konstants.SMTP_HOST_PORT.Value) {

				DeliveryMethod = SmtpDeliveryMethod.Network,
				Credentials = new NetworkCredential(SharedCode.EMail.Konstants.SMTP_USERNAME, SharedCode.EMail.Konstants.SMTP_PASSWORD)
			});
			Email.DefaultRenderer = new RazorRenderer();
			Log.Information("SMTP Client Initiated {SMTPUsername}:********@{SMTPHostFQDN}:{SMTPHostPort}",
				SharedCode.EMail.Konstants.SMTP_USERNAME, SharedCode.EMail.Konstants.SMTP_HOST_FQDN, SharedCode.EMail.Konstants.SMTP_HOST_PORT.Value);

			TwilioClient.Init(SharedCode.Twilio.Konstants.TWILIO_ACCOUNT_SID, SharedCode.Twilio.Konstants.TWILIO_AUTH_TOKEN);
			Log.Information("Twilio Client [{TwilioAccountSid}]", SharedCode.Twilio.Konstants.TWILIO_ACCOUNT_SID);

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




			AsterNET.Logger.Instance().Visible(true, AsterNET.Logger.MessageLevel.Debug);
			AsterNET.Logger.Instance().Visible(true, AsterNET.Logger.MessageLevel.Error);
			AsterNET.Logger.Instance().Visible(true, AsterNET.Logger.MessageLevel.Info);
			AsterNET.Logger.Instance().Visible(true, AsterNET.Logger.MessageLevel.Warning);


			NpgsqlConnection.GlobalTypeMapper.UseJsonNet();

			
			AsteriskFastAGI agi = new AsteriskFastAGI();
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
