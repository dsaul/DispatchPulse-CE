using System;
using Square;
using Square.Models;
using Square.Exceptions;
using Square.Apis;
using Npgsql;
using Databases.Records.Billing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace SquareTest
{
	


	class Program
	{
		

		static SquareClient SquareClient { get; set; } = new SquareClient.Builder()
				//.Environment(Square.Environment.Sandbox)
				//.AccessToken(SharedCode.Square.Konstants.SQUARE_SANDBOX_ACCESS_TOKEN)
				.Environment(Square.Environment.Production)
				.AccessToken(SharedCode.Square.Konstants.SQUARE_PRODUCTION_ACCESS_TOKEN)
				.Build();



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
				//.WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), SERILOG_LOG_FILE)
				.CreateLogger();

			//await EnsureAllCompaniesHaveSquareAccounts();

			await ChargePreAuthorizedCreditCards();
		}


		public static async Task EnsureAllCompaniesHaveSquareAccounts() {

			using NpgsqlConnection billingDB = new NpgsqlConnection(Databases.Konstants.KBillingDatabaseConnectionString);
			billingDB.Open();

			var api = SquareClient.CustomersApi;

			List<Task> tasks = new List<Task>();

			var allCompanies = BillingCompanies.All(billingDB);
			foreach (KeyValuePair<Guid, BillingCompanies> kvp in allCompanies) {
				tasks.Add(BillingCompanies.EnsureCompanyHasSquareAccount(SquareClient, kvp.Value));
			}

			await Task.WhenAll(tasks);

		}

		public static async Task ChargePreAuthorizedCreditCards() {




		}










	}
}
