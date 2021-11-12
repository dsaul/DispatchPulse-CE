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
		public class PerformPBXDeRegisterDIDParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public string? Did { get; set; }
			public Guid? BillingCompanyId { get; set; }
		}

		public class PerformPBXDeRegisterDIDResponse : IdempotencyResponse
		{
			public bool? Complete { get; set; } = null;
		}

		public async Task PerformPBXDeRegisterDID(PerformPBXDeRegisterDIDParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformPBXDeRegisterDIDResponse response = new PerformPBXDeRegisterDIDResponse()
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

				bool permAny = permissions.Contains(Databases.Konstants.kPermCRMDeleteDIDsAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermCRMDeleteDIDsCompany);

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


				// Find this number.
				var resRegPN = RegisteredPhoneNumbers.ForPhoneNumber(billingConnection, p.Did);
				if (resRegPN.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "This phone number is not registered.";
					break;
				}



				// Delete the entry for this number.
				RegisteredPhoneNumbers.Delete(billingConnection, resRegPN.Keys);

				response.Complete = true;

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
				await Clients.Caller.SendAsync("PerformPBXDeRegisterDIDCB", response).ConfigureAwait(false);

			}
			else
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("PerformPBXDeRegisterDIDCB", response).ConfigureAwait(false);
			}
		}
	}
}
