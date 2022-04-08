using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using API.Utility;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class PerformRegisterAdditionalUsersAdditionalUser
		{
			public string? Id { get; set; }
			public string? FullName { get; set; }
			public string? Email { get; set; }
			public string? PhoneNumber { get; set; }
			public string? PasswordHash { get; set; }
		}

		public class PerformRegisterAdditionalUsersParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<PerformRegisterAdditionalUsersAdditionalUser> OtherAccountsToAdd { get; set; } = new List<PerformRegisterAdditionalUsersAdditionalUser>();

		}

		public class PerformRegisterAdditionalUsersResponse : IdempotencyResponse
		{
			public bool? Created { get; set; } = false;

		}

		public async Task PerformRegisterAdditionalUsers(PerformRegisterAdditionalUsersParams p)
		{
			PerformRegisterAdditionalUsersResponse response = new PerformRegisterAdditionalUsersResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			BillingContacts? billingContact = null;
			NpgsqlConnection? dpDBConnection = null;

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
					out dpDBConnection,
					true
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

				if (p.OtherAccountsToAdd == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Register Additional Users: No other users.";
					break;
				}

				bool breakSomeMore = false;
				foreach (PerformRegisterAdditionalUsersAdditionalUser addnl in p.OtherAccountsToAdd)
				{
					if (string.IsNullOrWhiteSpace(addnl.Email))
					{
						response.IsError = true;
						response.ErrorMessage = "Register Additional Users: One of the users emails is invalid.";

						breakSomeMore = true;
						break;
					}

					if (string.IsNullOrWhiteSpace(addnl.FullName))
					{
						response.IsError = true;
						response.ErrorMessage = "Register Additional Users: One of the users names is invalid.";

						breakSomeMore = true;
						break;
					}

					if (string.IsNullOrWhiteSpace(addnl.PhoneNumber))
					{
						response.IsError = true;
						response.ErrorMessage = "Register Additional Users: One of the users phone numbers is invalid.";

						breakSomeMore = true;
						break;
					}

					if (string.IsNullOrWhiteSpace(addnl.PasswordHash))
					{
						response.IsError = true;
						response.ErrorMessage = "Register Additional Users: One of the users passwords is invalid.";

						breakSomeMore = true;
						break;
					}


				}
				if (breakSomeMore)
					break;


				// actually add the accounts after verification
				foreach (PerformRegisterAdditionalUsersAdditionalUser addnl in p.OtherAccountsToAdd)
				{
					if (null == billingCompany || null == billingCompany.Uuid)
						continue;


					Guid contactId = Guid.NewGuid();

					string fullName = "No Name";
					if (!string.IsNullOrWhiteSpace(addnl.FullName))
						fullName = addnl.FullName;

					string email = "email@example.com";
					if (!string.IsNullOrWhiteSpace(addnl.Email))
						email = addnl.Email;

					string phone = "555-555-1234";
					if (!string.IsNullOrWhiteSpace(addnl.PhoneNumber))
						phone = addnl.PhoneNumber;


					BillingContacts contact = new BillingContacts(
					Uuid: contactId,
					FullName: fullName,
					Email: email,
					Phone: phone,
					PasswordHash: addnl.PasswordHash,
					EmailListMarketing: false,
					EmailListTutorials: false,
					MarketingCampaign: "",
					CompanyId: billingCompany.Uuid.Value,
					ApplicationData: "{}",
					Json: "{}"
					);
					BillingContacts.Upsert(billingConnection, new Dictionary<Guid, BillingContacts> {
						{ contactId, contact }
					}, out _, out _);


					Guid membershipId = Guid.NewGuid();
					BillingPermissionsGroupsMemberships newGroupMembership = new BillingPermissionsGroupsMemberships(
						Id: membershipId,
						GroupId: Konstants.KEmployeeGroupId, // Employee
						ContactId: contact.Uuid,
						Json: "{}"
						);
					BillingPermissionsGroupsMemberships.Upsert(billingConnection, new Dictionary<Guid, BillingPermissionsGroupsMemberships>
					{
						{ membershipId, newGroupMembership }
					}, out _, out _);

				}


				response.Created = true;







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


			await Clients.Caller.SendAsync("PerformRegisterAdditionalUsersCB", response).ConfigureAwait(false);
		}

	}
}
