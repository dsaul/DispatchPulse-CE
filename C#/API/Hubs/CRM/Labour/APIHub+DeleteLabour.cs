using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class DeleteLabourParams : IdempotencyRequest
		{
			public List<Guid> LabourDelete { get; set; } = new List<Guid>();
		}
		public class DeleteLabourResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> LabourDelete { get; set; } = new List<Guid>();
		}

		public async Task DeleteLabour(DeleteLabourParams p)
		{


			DeleteLabourResponse response = new DeleteLabourResponse()
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
				if (p.LabourDelete == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.LabourDelete == null";
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

				if (p.LabourDelete.Count == 0)
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

				bool permAny = permissions.Contains(EnvDatabases.kPermCRMDeleteLabourAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermCRMDeleteLabourCompany);
				bool permSelf = permissions.Contains(EnvDatabases.kPermCRMDeleteLabourSelf);

				if (permAny || permCompany)
				{
					// delete

					List<Guid> affected = Labour.Delete(dpDBConnection, p.LabourDelete);
					if (affected.Count == 0)
					{
						response.IsError = true;
						response.ErrorMessage = "No rows deleted?";
						break;
					}

					response.LabourDelete = affected;
				}
				else if (permSelf)
				{
					do
					{
						bool abort = false;
						foreach (Guid id in p.LabourDelete)
						{

							Labour labour = Labour.ForId(dpDBConnection, id).First().Value;

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

						List<Guid> affected = Labour.Delete(dpDBConnection, p.LabourDelete);
						if (affected.Count == 0)
						{
							response.IsError = true;
							response.ErrorMessage = "No rows deleted?";
							break;
						}

						response.LabourDelete = affected;
					}
					while (false);
				}
				else
				{
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

			if (billingContact == null)
			{
				await Clients.Caller.SendAsync("DeleteLabourCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("DeleteLabourCB", response).ConfigureAwait(false);
			}









		}
	}
}