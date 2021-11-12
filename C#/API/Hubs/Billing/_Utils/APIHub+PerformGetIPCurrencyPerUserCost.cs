using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Databases.Records.CRM;
using Databases.Records.Billing;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformGetIPAndCurrencyParams : IdempotencyRequest
		{
			
		}

		public class PerformGetIPAndCurrencyResponse : IdempotencyResponse
		{
			public string? IP { get; set; }
			public string? Currency  { get; set; }
			public decimal? PerUserCost { get; set; }
		}


		public async Task PerformGetIPCurrencyPerUserCost(PerformGetIPAndCurrencyParams p)
		{
			PerformGetIPAndCurrencyResponse response = new PerformGetIPAndCurrencyResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;


				string billingConnectionString = Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.KBillingDatabaseName);
				if (string.IsNullOrWhiteSpace(billingConnectionString))
				{
					response.IsError = true;
					response.ErrorMessage = "Couldn't get connection information for the billing system.";
					break;
				}

				billingConnection = new NpgsqlConnection(billingConnectionString);
				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Couldn't open a connection to the billing system. #1";
					break;
				}


				billingConnection.Open();
				if (billingConnection.State != System.Data.ConnectionState.Open)
				{
					response.IsError = true;
					response.ErrorMessage = "Couldn't open a connection to the billing system. #2";
					break;
				}

				decimal perUserCostUSD = 50;
				decimal perUserCostCAD = 70;


				response.IP = GetIP.GetRequestIP(Context.GetHttpContext());


				if (!IPAddress.TryParse(response.IP, out IPAddress? ipObj))
				{
					response.IsError = true;
					response.ErrorMessage = "Can't read the IP address.";
					response.IP = null;
					response.Currency = "USD";
					response.PerUserCost = perUserCostUSD;
					break;
				}


				Dictionary<Guid, UtilityIpToCountry> ipEntries = UtilityIpToCountry.ForIPAddress(billingConnection, ipObj);
				UtilityIpToCountry obj = ipEntries.FirstOrDefault().Value;







				if (null == obj)
				{
					response.IsError = true;
					response.ErrorMessage = "No database entry for this IP.";
					response.Currency = "USD";
					response.PerUserCost = perUserCostUSD;
					break;
				}
				else if (obj.CountryCode == "CA")
				{
					response.Currency = "CAD";
					response.PerUserCost = perUserCostCAD;
					break;
				}
				else
				{
					response.Currency = "USD";
					response.PerUserCost = perUserCostUSD;
					break;
				}







			}
			while (false);


			if (billingConnection != null)
			{
				billingConnection.Dispose();
				billingConnection = null;
			}









			

			

			do
			{
				
				


			}
			while (false);

			


			await Clients.Caller.SendAsync("PerformGetIPCurrencyPerUserCostCB", response).ConfigureAwait(false);
		}



	}
}
