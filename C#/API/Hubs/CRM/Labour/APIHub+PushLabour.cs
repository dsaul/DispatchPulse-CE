using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utility;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class PushLabourParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public Dictionary<Guid, Labour> Labour { get; set; } = new Dictionary<Guid, Labour>();
		}

		public class PushLabourResponse : IdempotencyResponse
		{
			public List<Guid> Labour { get; set; } = new List<Guid>();
		}

		public async Task PushLabour(PushLabourParams p)
		{

			PushLabourResponse response = new PushLabourResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			RequestLabourResponse othersMsg = new RequestLabourResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
			NpgsqlConnection? dpDBConnection = null;

			List<Guid> callerResponse = new List<Guid>();
			Dictionary<Guid, Labour> toSendToOthers = new Dictionary<Guid, Labour>();
			BillingContacts? billingContact = null;

			do
			{
				if (p == null)
				{
					response.IsError = true;
					response.ErrorMessage = "No parameters provided.";
					break;
				}
				if (p.Labour == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.Labour == null";
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


				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

				// Check permissions.
				HashSet<string> permissions = BillingPermissionsBool.GrantedForBillingContact(billingConnection, billingContact);

				bool permAny = permissions.Contains(Databases.Konstants.kPermCRMPushLabourAny);
				bool permCompany = permissions.Contains(Databases.Konstants.kPermCRMPushLabourCompany);
				bool permSelf = permissions.Contains(Databases.Konstants.kPermCRMPushLabourSelf);

				if (permAny || permCompany)
				{
					Labour.Upsert(
						dpDBConnection,
						p.Labour,
						out callerResponse,
						out toSendToOthers
					);


					response.Labour = callerResponse;
					othersMsg.Labour = toSendToOthers;
				}
				else if (permSelf)
				{
					// can only 
					do
					{
						bool abort = false;
						foreach (KeyValuePair<Guid, Labour> kvp in p.Labour)
						{
							Labour labour = kvp.Value;
							if (labour.AgentId != billingContact.DPAgentId)
							{
								abort = true;
								break;
							}
						}

						if (abort)
						{
							response.IsError = true;
							response.ErrorMessage = "You cannot modify those agents.";
							response.IsPermissionsError = true;
							break;
						}

						Labour.Upsert(
							dpDBConnection,
							p.Labour,
							out callerResponse,
							out toSendToOthers
						);


						response.Labour = callerResponse;
						othersMsg.Labour = toSendToOthers;
					}
					while (false);
				}
				else
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
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


			await Clients.Caller.SendAsync("PushLabourCB", response).ConfigureAwait(false);

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("RequestLabourCB", othersMsg).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("RequestLabourCB", othersMsg).ConfigureAwait(false);
			}










		}
	}
}