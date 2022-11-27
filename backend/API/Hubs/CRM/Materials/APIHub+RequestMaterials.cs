using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;
using System.Linq;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{

		public class RequestMaterialsParams : IdempotencyRequest
		{
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
			public string? LimitToProjectId { get; set; }
			public bool? ShowChildrenOfProjectIdAsWell { get; set; }
		}
		public class RequestMaterialsResponse : PermissionsIdempotencyResponse
		{

			public Dictionary<Guid, Materials> Materials { get; } = new Dictionary<Guid, Materials>();
		}

		public async Task RequestMaterials(RequestMaterialsParams p)
		{

			RequestMaterialsResponse response = new RequestMaterialsResponse
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

				bool permAny = permissions.Contains(EnvDatabases.kPermCRMRequestMaterialsAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermCRMRequestMaterialsCompany);
				bool permSelf = permissions.Contains(EnvDatabases.kPermCRMRequestMaterialsSelf);


				if (permAny || permCompany)
				{
					if (p.LimitToIds == null || p.LimitToIds.Count == 0)
					{
						response.Materials.AddRange(Materials.All(dpDBConnection));
					}
					else
					{
						response.Materials.AddRange(Materials.ForIds(dpDBConnection, p.LimitToIds));
					}
				}
				else if (permSelf)
				{

#warning TODO: Implement


				}
				else
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// Apply filters
				List<Guid> keys = response.Materials.Keys.ToList();


				HashSet<Guid> limitToProjectIds = new HashSet<Guid>();


				if (p.LimitToProjectId != null)
				{
					Guid limitToId = Guid.Parse(p.LimitToProjectId);

					// Get children projects as well.
					limitToProjectIds = new HashSet<Guid> { limitToId };

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
							if (limitToProjectIds.Contains(child))
								continue;

							limitToProjectIds.Add(child);
							continueSearch.Add(child);
						}

						foreach (Guid o in continueSearch)
						{
							Recursive(o);
						}
					}

					if (p.ShowChildrenOfProjectIdAsWell != null & p.ShowChildrenOfProjectIdAsWell == true)
					{
						Projects.GetParentProjectMap(dpDBConnection, out parentMapParentKey, out parentMapChildKey);
						Recursive(limitToId);
					}

				}

				foreach (Guid key in keys)
				{
					Materials material = response.Materials[key];

					// p.LimitToProjectId
					if (p.LimitToProjectId != null)
					{
						bool remove = false;
						do
						{
							Guid? mainProject = material.ProjectId;
							if (mainProject == null)
							{
								remove = true;
								break;
							}

							if (!limitToProjectIds.Contains(mainProject.Value))
							{
								remove = true;
								break;
							}

						} while (false);

						if (remove)
						{
							response.Materials.Remove(key);
							continue;
						}
					}





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

			await Clients.Caller.SendAsync("RequestMaterialsCB", response).ConfigureAwait(false);

		}
	}
}