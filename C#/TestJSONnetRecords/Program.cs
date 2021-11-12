using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;

namespace TestJSONnetRecords
{
	record Product(string Name, DateTime Expiry, string[] Sizes)
	{
		[JsonIgnore]
		public static string Test
		{
			get {
				return "asd";
			}
		}


	}

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

			Product product = new(
				Name: "Apple",
				Expiry: new DateTime(2008, 12, 28),
				Sizes: new string[] { "Small" }

				);

			string json = JsonConvert.SerializeObject(product);
			// {
			//   "Name": "Apple",
			//   "Expiry": "2008-12-28T00:00:00",
			//   "Sizes": [
			//     "Small"
			//   ]
			// }

			Log.Debug(json);
		}
	}
}
