using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SharedCode.DatabaseSchemas;
using Newtonsoft.Json.Linq;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformPBXRegisterDIDParams : IdempotencyRequest
		{
			public string? Did { get; set; }
			public string? DidPassword { get; set; }
			public Guid? BillingCompanyId { get; set; }
		}

		public class PerformPBXRegisterDIDResponse : PermissionsIdempotencyResponse
		{
			public bool? Complete { get; set; } = null;
		}

		public async Task PerformPBXRegisterDID(PerformPBXRegisterDIDParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformPBXRegisterDIDResponse response = new PerformPBXRegisterDIDResponse()
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

				bool permAny = permissions.Contains(EnvDatabases.kPermCRMPushDIDsAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermCRMPushDIDsCompany);

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

				if (p.DidPassword != BadPhoneHash.CreateBadPhoneHash(p.Did))
				{
					response.IsError = true;
					response.ErrorMessage = "Sorry, the passcode for that phone number doesn't match.";
					break;
				}




				// Find this number.
				var resRegPN = RegisteredPhoneNumbers.ForPhoneNumber(billingConnection, p.Did);
				if (resRegPN.Count != 0)
				{
					response.IsError = true;
					response.ErrorMessage = "This number is already registered.";
					break;
				}



				// Add the entry for this number.

				Guid pnId = Guid.NewGuid();
				RegisteredPhoneNumbers pn = new RegisteredPhoneNumbers(pnId, new JObject
				{
					[RegisteredPhoneNumbers.kJsonKeyPhoneNumber] = p.Did,
					[RegisteredPhoneNumbers.kJsonKeyBillingCompanyId] = p.BillingCompanyId
				}.ToString());

				RegisteredPhoneNumbers.Upsert(billingConnection, new Dictionary<Guid, RegisteredPhoneNumbers> {
					{ pnId, pn }
				}, out _, out _);

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
