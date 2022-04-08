using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SharedCode.DatabaseSchemas;
using System.IO;
using SharedCode.DatabaseSchemas;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public async Task<PerformNotifyReloadBillingCompanyResponse> PerformNotifyReloadBillingCompany(PerformNotifyReloadBillingCompanyParams p)
		{
			if (string.IsNullOrWhiteSpace(SharedCode.Hubs.Konstants.SQUARE_PAYMENTS_AND_API_SHARED_SECRET))
				return new PerformNotifyReloadBillingCompanyResponse(IsError: true, ErrorMessage: "SQUARE_PAYMENTS_AND_API_SHARED_SECRET_FILE not set!", Completed: false);
			if (p.SharedSecret != SharedCode.Hubs.Konstants.SQUARE_PAYMENTS_AND_API_SHARED_SECRET)
				return new PerformNotifyReloadBillingCompanyResponse(IsError: true, ErrorMessage: "Shared Secret doesn't match.", Completed: false);
			if (null == p)
				throw new ArgumentNullException(nameof(p));
			if (null == p.SessionId)
				return new PerformNotifyReloadBillingCompanyResponse(IsError: true, ErrorMessage: "Didn't receive a session id.", Completed: false);


			bool isError = false;
			string? errorMessage = null;
			bool completed = false;

			do
			{
				using NpgsqlConnection? billingConnection = new NpgsqlConnection(Databases.Konstants.DatabaseConnectionStringForDB(Databases.Konstants.BILLING_DATABASE_NAME));
				billingConnection.Open();

				var resCompany = BillingCompanies.ForSessionId(billingConnection, p.SessionId.Value);
				if (0 == resCompany.Count)
				{
					isError = true;
					errorMessage = "We couldn't find your company in the database.";
					break;
				}

				BillingCompanies company = resCompany.FirstOrDefault().Value;
				if (null == company.Uuid)
				{
					isError = true;
					errorMessage = "We couldn't find your company in the database.";
					break;
				}

				RequestBillingCompanyResponse othersMsg = new RequestBillingCompanyResponse
				{
					IdempotencyToken = Guid.NewGuid().ToString(),
					BillingCompany = company
				};

				await Clients.Group(BillingCompanies.GroupNameForCompanyId(company.Uuid.Value)).SendAsync("RequestBillingCompanyForCurrentSessionCB", othersMsg).ConfigureAwait(false);
			}
			while (false);

			return new PerformNotifyReloadBillingCompanyResponse(IsError: isError, ErrorMessage: errorMessage, Completed: completed);
		}
	}
}
