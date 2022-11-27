using Renci.SshNet;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace SSHTest
{
	class Program
	{
		static void Main(string[] args)
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

			Log.Debug("Hello World!");

			// Tell the PBX To download the file.

			var pk = new PrivateKeyFile(@"");
			var keyFiles = new[] { pk };

			var methods = new List<AuthenticationMethod>();
			methods.Add(new PrivateKeyAuthenticationMethod("root", keyFiles));

			var conn = new ConnectionInfo(SharedCode.ARI.Konstants.PBX_FQDN, 22, "root", methods.ToArray());

			var sshClient = new SshClient(conn);
			sshClient.Connect();
			SshCommand sc= sshClient.CreateCommand(@"bash -c ""s3cmd get s3://tts-cache/neural/Brian/88f664e2-1622-4ce5-b72b-8d9287e6d843/88f664e2-1622-4ce5-b72b-8d9287e6d843.pcm /srv/tts-cache/neural/Brian/88f664e2-1622-4ce5-b72b-8d9287e6d843/88f664e2-1622-4ce5-b72b-8d9287e6d843.pcm""");
			sc.Execute();
			string answer = sc.Result;
			Log.Debug($"SSH ANSWER {answer}");
		}
	}
}
