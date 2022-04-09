using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestBillingPackagesParams : IdempotencyRequest
		{
			public Guid SessionId { get; set; }
		}
		public class RequestBillingPackagesResponse : PermissionsIdempotencyResponse
		{
			public List<BillingPackages> BillingPackages { get; } = new List<BillingPackages> { };
		}
		public async Task RequestBillingPackagesForCurrentSession(RequestBillingPackagesParams p)
		{
			RequestBillingPackagesResponse response = new RequestBillingPackagesResponse()
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

				if (permissions.Contains(EnvDatabases.kPermBillingPackagesReadAny))
				{


					// Return all packages.
					Dictionary<Guid, BillingPackages> results = BillingPackages.All(billingConnection);

					response.BillingPackages.AddRange(results.Values);


				}
				else if (permissions.Contains(EnvDatabases.kPermBillingPackagesReadCompany))
				{
					// Return just packages that this company has.
					// Go through the subscriptions for this company and get a list of the packages.

					if (null == billingContact.CompanyId)
					{
						response.IsError = true;
						response.ErrorMessage = "No company id.";
						break;
					}


					Dictionary<Guid, BillingSubscriptions> subs = BillingSubscriptions.ForCompanyId(billingConnection, billingContact.CompanyId.Value);
					List<Guid> packageIds = new List<Guid>();

					foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in subs)
					{
						if (null == kvp.Value.PackageId)
							continue;

						packageIds.Add(kvp.Value.PackageId.Value);
					}

					Dictionary<Guid, BillingPackages> packages = BillingPackages.ForIds(billingConnection, packageIds);


					response.BillingPackages.AddRange(packages.Values);

					
				}
				else
				{
					// no permissions
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
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


			if (null != billingContact)
			{
				await Clients.Group(BillingContacts.UserGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingPackagesForCurrentSessionCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Caller.SendAsync("RequestBillingPackagesForCurrentSessionCB", response).ConfigureAwait(false);
			}



		}
	}
}
