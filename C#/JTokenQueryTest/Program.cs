using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;

namespace JTokenQueryTest
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

			//Log.Debug("Hello World!");

			string json = @"{
  ""id"": ""65ece583-34ac-4279-8d6b-3e2fe33ccd9e"",
  ""name"": ""Dan Saul"",
  ""title"": """",
  ""employmentStatusId"": ""7457f2d2-e052-11e9-ac0d-02420a000018"",
  ""hourlyWage"": 0.0,
  ""notificationSMSNumber"": null,
  ""lastModifiedISO8601"": ""2018-06-11T08:18:49.0000000Z"",
  ""lastModifiedBillingId"": null,
  ""phoneId"": ""1"",
  ""phonePasscode"": ""1""
}";

			JObject o = JObject.Parse(json);
			JToken acme = o.SelectToken("phoneId");

			Log.Debug(acme.ToString());
		}
	}
}
