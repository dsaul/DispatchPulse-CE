using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PerformBillingPermissionsGroupsMembershipsAddParams : IdempotencyRequest
		{
			public Guid? BillingContactId { get; set; }
			public List<Guid> PermissionsGroupIds { get; set; } = new ();
		}

		public class PerformBillingPermissionsGroupsMembershipsAddResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> BillingPermissionsGroupsMemberships { get; } = new ();

		}

		public async Task PerformBillingPermissionsGroupsMembershipsAdd(PerformBillingPermissionsGroupsMembershipsAddParams p)
		{
			if (null == p)
				throw new ArgumentNullException(nameof(p));

			PerformBillingPermissionsGroupsMembershipsAddResponse response = new ()
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestBillingPermissionsGroupsMembershipsResponse othersMsg = new ()
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
				othersMsg.RoundTripRequestId = p.RoundTripRequestId;

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

				if (!permissions.Contains(EnvDatabases.kPermBillingPermissionsGroupsMembershipsModifyAny) &&
					!permissions.Contains(EnvDatabases.kPermBillingPermissionsGroupsMembershipsModifyCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				if (null == p.BillingContactId)
				{
					response.IsError = true;
					response.ErrorMessage = "null == p.BillingContactId";
					break;
				}


				// Do action


				

				Dictionary<Guid, BillingPermissionsGroupsMemberships> existingMemberships = 
					BillingPermissionsGroupsMemberships.ForBillingContactId(billingConnection, p.BillingContactId.Value);

				foreach (Guid groupToAdd in p.PermissionsGroupIds)
				{
					// Check for existing membership.
					bool skip = false;
					foreach (KeyValuePair<Guid, BillingPermissionsGroupsMemberships> entry in existingMemberships)
					{
						if (entry.Value.GroupId == groupToAdd)
						{
							skip = true;
							break;
						}
					}
					if (skip)
						continue;


					// Add membership.
					List<Guid> callerResponse;
					Dictionary<Guid, BillingPermissionsGroupsMemberships> toSendToOthers;

					Guid guid = Guid.NewGuid();
					BillingPermissionsGroupsMemberships.Upsert(billingConnection, new Dictionary<Guid, BillingPermissionsGroupsMemberships>
					{
						{ 
							guid, 
							new BillingPermissionsGroupsMemberships(
								Id: guid,
								GroupId: groupToAdd,
								ContactId: p.BillingContactId.Value,
								Json: "{}"
								)
						}
					}, out callerResponse, out toSendToOthers);


					response.BillingPermissionsGroupsMemberships.AddRange(callerResponse);
					othersMsg.BillingPermissionsGroupsMemberships.AddRange(toSendToOthers.Values);

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

			await Clients.Caller.SendAsync("PerformBillingPermissionsGroupsMembershipsAddCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestBillingPermissionsGroupsMembershipsForCurrentSessionCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestBillingPermissionsGroupsMembershipsForCurrentSessionCB", othersMsg).ConfigureAwait(false);
			}


			if (null != p && response.IsError == false && null != p.SessionId)
			{
				await RequestBillingPermissionsBoolForCurrentSession(new RequestBillingPermissionsBoolParams()
				{
					SessionId = p.SessionId.Value
				}).ConfigureAwait(false);
			}
			
		}

	}
}
