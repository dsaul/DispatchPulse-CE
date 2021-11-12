using Serilog;
using Serilog.Events;
using System;

namespace DateTimeDiffTest
{
	class Program
	{
		static void Main()
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

			Log.Debug("DateTime Diff Test");

			DateTime start = new DateTime(2020, 12, 27, 8, 0, 0);
			DateTime end = new DateTime(2020, 12, 27, 16, 30, 0);

			TimeSpan diff = end.Subtract(start);
			Log.Debug("{diff}", diff);
		}
	}
}
