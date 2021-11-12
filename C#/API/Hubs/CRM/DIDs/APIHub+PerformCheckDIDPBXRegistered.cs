using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Databases.Records.Billing;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformCheckDIDPBXRegisteredParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public string? Did { get; set; }
			public Guid? BillingCompanyId { get; set; }
		}

		public class PerformCheckDIDPBXRegisteredResponse : IdempotencyResponse
		{
			public bool? IsRegistered { get; set; } = null;
			public bool? IsRegisteredToDifferentCompany { get; set; } = null;
			public bool? IsRegisteredToUnknownCompany { get; set; } = null;
		}

		public async Task PerformCheckDIDPBXRegistered(PerformCheckDIDPBXRegisteredParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformCheckDIDPBXRegisteredResponse response = new PerformCheckDIDPBXRegisteredResponse()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

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

				if (p.Did == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No phone number provided.";
					break;
				}

				if (p.BillingCompanyId == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No billing company provided.";
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

				bool permAny = permissions.Contains(Databases.Konstants.kPermCRMRequestDIDsAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermCRMRequestDIDsCompany);

				if (permAny)
				{

				}
				else if (permCompany)
				{
					if (p.BillingCompanyId.Value != billingContact.CompanyId)
					{
						response.IsError = true;
						response.ErrorMessage = "No permissions. #2";
						response.IsPermissionsError = true;
						break;
					}
				}
				else
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				

				// Do action.




				var resRegPN = RegisteredPhoneNumbers.ForPhoneNumber(billingConnection, p.Did);
				if (resRegPN.Count == 0)
				{
					response.IsRegistered = false;
					break;
				}

				RegisteredPhoneNumbers pn = resRegPN.FirstOrDefault().Value;
				if (null == pn.BillingCompanyId)
				{
					response.IsRegistered = true;
					response.IsRegisteredToUnknownCompany = true;
					response.IsRegisteredToDifferentCompany = false;
					break;
				}

				if (p.BillingCompanyId.Value != pn.BillingCompanyId)
				{
					response.IsRegistered = true;
					response.IsRegisteredToUnknownCompany = false;
					response.IsRegisteredToDifferentCompany = true;
					break;
				}

				if (p.BillingCompanyId.Value == pn.BillingCompanyId)
				{
					response.IsRegistered = true;
					response.IsRegisteredToUnknownCompany = false;
					response.IsRegisteredToDifferentCompany = false;
					break;
				}


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




			if (null == billingContact)
			{
				await Clients.Caller.SendAsync("PerformCheckDIDPBXRegisteredCB", response).ConfigureAwait(false);

			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformCheckDIDPBXRegisteredCB", response).ConfigureAwait(false);
			}
		}
	}
}
