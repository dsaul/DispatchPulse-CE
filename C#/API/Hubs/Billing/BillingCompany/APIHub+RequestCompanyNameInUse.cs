using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestCompanyNameInUseParams : IdempotencyRequest
		{
			public string? Abbreviation { get; set; }
		}
		public class RequestCompanyNameInUseResponse : IdempotencyResponse
		{
			public bool? InUse { get; set; }
		}

		public async Task RequestCompanyNameInUse(RequestCompanyNameInUseParams p)
		{
			RequestCompanyNameInUseResponse response = new RequestCompanyNameInUseResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				if (string.IsNullOrWhiteSpace(p.Abbreviation))
				{
					response.IsError = true;
					response.ErrorMessage = "No abbreviation provided.";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;


				string billingConnectionString = Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME);
				if (null == billingConnectionString)
				{
					response.IsError = true;
					response.ErrorMessage = "Couldn't get connection information for the billing system.";
					break;
				}

				using NpgsqlConnection billingConnection = new NpgsqlConnection(billingConnectionString);
				billingConnection.Open();





				Dictionary<Guid, BillingCompanies> results = BillingCompanies.ForAbbreviation(billingConnection, p.Abbreviation);

				response.InUse = results.Count != 0;

			}
			while (false);


			await Clients.Caller.SendAsync("RequestCompanyNameInUseCB", response).ConfigureAwait(false);


		}
	}
}
