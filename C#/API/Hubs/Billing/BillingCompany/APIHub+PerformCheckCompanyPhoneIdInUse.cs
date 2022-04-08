using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class PerformCheckCompanyPhoneIdInUseParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public string? CompanyId { get; set; }
		}

		public class PerformCheckCompanyPhoneIdInUseResponse : IdempotencyResponse
		{
			public bool? InUse { get; set; } = false;

		}


		public async Task PerformCheckCompanyPhoneIdInUse(PerformCheckCompanyPhoneIdInUseParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformCheckCompanyPhoneIdInUseResponse response = new PerformCheckCompanyPhoneIdInUseResponse();


			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			BillingContacts? billingContact = null;


			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}

				response.RoundTripRequestId = p.RoundTripRequestId;

				BillingSessions? session = null;
				BillingCompanies? billingCompany = null;

				SessionUtils.GetSessionInformation(
					this,
					response,
					p.SessionId,
					out _,
					out billingConnection,
					out session,
					out billingContact,
					out billingCompany,
					out _,
					out _,
					out dpDBConnection
					);

				if (null != response.IsError && response.IsError.Value)
					break;

				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to billing database.";
					break;
				}

				if (null == billingContact)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to get billing contact.";
					break;
				}


				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				if (!permissions.Contains(Databases.Konstants.kPermBillingCompaniesModifyCompanyPhoneId)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				if (null == p.CompanyId)
				{
					response.IsError = true;
					response.ErrorMessage = "null == p.CompanyId";
					break;
				}



				// Do check.
				string sql = "SELECT * FROM \"billing-companies\" WHERE json->> 'phoneId' = @phoneId";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, billingConnection);
				cmd.Parameters.AddWithValue("@phoneId", p.CompanyId);

				using NpgsqlDataReader reader = cmd.ExecuteReader();

				response.InUse = reader.HasRows;

			}
			while (false);

			if (billingConnection != null)
			{
				billingConnection.Dispose();
				billingConnection = null;
			}

			if (dpDBConnection != null)
			{
				dpDBConnection.Dispose();
				dpDBConnection = null;
			}

			if (null != billingContact)
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformCheckCompanyPhoneIdInUseCB", response).ConfigureAwait(false);
			}
			
		}




	}
}
