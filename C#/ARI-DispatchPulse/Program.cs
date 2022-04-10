using System;
using AsterNET.FastAGI;
using AsterNET.FastAGI.MappingStrategies;
using System.Collections.Generic;
using Npgsql;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using Serilog;
using SharedCode;

namespace ARI
{
	public static class Program {

		

		public static string? SERILOG_LOG_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("SERILOG_LOG_FILE");
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


			Log.Information("Ari-DispatchPulse (c) 2021 Dan Saul");


			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI)) {
				Log.Error("SIGNAL_R_HUB_URI_FILE not set!");
				return;
			}

			

			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.ARI_AND_API_SHARED_SECRET)) {
				Log.Error("ARI_AND_API_SHARED_SECRET_FILE not set!");
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

			SignalRConnection = new HubConnectionBuilder()
				.WithUrl(SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI)
				.Build();

			SignalRConnection.Closed += async (error) => {
				await Task.Delay(new Random().Next(0, 5) * 1000);
				await SignalRConnection.StartAsync();
			};

			await SignalRConnection.StartAsync();
			Log.Information("SignalR API Connection API URI {ApiURI}", SharedCode.Hubs.Konstants.SIGNAL_R_HUB_URI);

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
                    ScriptClass = "ARI.IVR.CompanyAccess.EntryPoint",
                    ScriptName = "CompanyAccess"
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
