using System;
using System.Collections.Generic;
using System.Linq;
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
		public class RequestProjectsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
			public bool? ShowChildrenOfProjectIdAsWell { get; set; }
		}
		public class RequestProjectsResponse : IdempotencyResponse
		{

			public Dictionary<Guid, Projects> Projects { get; set; } = new Dictionary<Guid, Projects>();
		}

		public async Task RequestProjects(RequestProjectsParams p)
		{
			RequestProjectsResponse response = new RequestProjectsResponse
			{
				IdempotencyToken = Guid.NewGuid().ToString(),
			};

			NpgsqlConnection? billingConnection = null;
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
				BillingContacts? billingContact = null;
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

				if (!permissions.Contains(Databases.Konstants.kPermCRMRequestProjectsAny) &&
					!permissions.Contains(Databases.Konstants.kPermCRMRequestProjectsCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}



				// Add the child ids if applicable.
				if (
					p.LimitToIds != null && p.LimitToIds.Count != 0 &&
					p.ShowChildrenOfProjectIdAsWell != null && p.ShowChildrenOfProjectIdAsWell.Value == true)
				{
					Guid[] orig = new Guid[p.LimitToIds.Count];
					p.LimitToIds.CopyTo(orig);

					Dictionary<Guid, List<Guid>> parentMapParentKey;
					Dictionary<Guid, List<Guid>> parentMapChildKey;

					void Recursive(Guid guid)
					{
						if (!parentMapParentKey.ContainsKey(guid))
							return;

						List<Guid> continueSearch = new List<Guid>();

						List<Guid> childList = parentMapParentKey[guid];
						foreach (Guid child in childList)
						{
							if (p.LimitToIds.Contains(child))
								continue;

							p.LimitToIds.Add(child);
							continueSearch.Add(child);
						}

						foreach (Guid o in continueSearch)
						{
							Recursive(o);
						}
					}

					Projects.GetParentProjectMap(dpDBConnection, out parentMapParentKey, out parentMapChildKey);
					foreach (Guid o in orig)
					{
						Recursive(o);
					}
					
				}




				
				if (p.LimitToIds == null || p.LimitToIds.Count == 0)
				{
					response.Projects = Projects.All(dpDBConnection);
				}
				else
				{
					response.Projects = Projects.ForIds(dpDBConnection, p.LimitToIds);
				}

			} while (false);

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

			await Clients.Caller.SendAsync("RequestProjectsCB", response).ConfigureAwait(false);
		}
	}
}