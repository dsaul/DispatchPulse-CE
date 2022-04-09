﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestLabourParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();
			public bool? LimitToSessionAgent { get; set; }
			public string? LimitToProjectId { get; set; }
			public string? LimitToAgentId { get; set; }
			public string? LimitToAssignmentId { get; set; }
			public bool? LimitToActiveAndToday { get; set; }
			public bool? ShowChildrenOfProjectIdAsWell { get; set; }
		}

		public class RequestLabourResponse : PermissionsIdempotencyResponse
		{

			public Dictionary<Guid, Labour> Labour { get; set; } = new Dictionary<Guid, Labour>();
		}

		public async Task RequestLabour(RequestLabourParams p)
		{

			RequestLabourResponse response = new RequestLabourResponse
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

				bool permAny = permissions.Contains(EnvDatabases.kPermCRMRequestLabourAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermCRMRequestLabourCompany);
				bool permSelf = true;




				if (permAny || permCompany || permSelf)
				{
					if (p.LimitToIds == null || p.LimitToIds.Count == 0)
					{
						response.Labour = Labour.All(dpDBConnection);
					}
					else
					{
						response.Labour = Labour.ForIds(dpDBConnection, p.LimitToIds);
					}

				}
				else
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				// If it is self only, force p.LimitToSessionAgent to be true.
				if (permSelf && !permCompany && !permAny)
				{
					p.LimitToSessionAgent = true;
				}

				// Apply filters
				List<Guid> keys = response.Labour.Keys.ToList();


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
					Labour labour = response.Labour[key];


					//if (labour.Id.Equals(Guid.Parse("d7a49539-ca43-4b18-a310-05ea42fb1708")))
					//{

					//}

					// p.LimitToSessionAgent
					if (p.LimitToSessionAgent != null && p.LimitToSessionAgent == true)
					{
						bool remove = false;
						do
						{
							Guid? sessionAgentId = billingContact.DPAgentId;
							if (null == sessionAgentId)
							{
								remove = true;
								break;
							}

							Guid? agent = labour.AgentId;
							if (agent == null)
							{
								remove = true;
								break;
							}

							if (agent != sessionAgentId.Value)
							{
								remove = true;
								break;
							}


						} while (false);

						if (remove)
						{
							response.Labour.Remove(key);
							continue;
						}
					}


					// p.LimitToProjectId
					if (p.LimitToProjectId != null)
					{
						bool remove = false;
						do
						{
							Guid? mainProject = labour.ProjectId;
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
							response.Labour.Remove(key);
							continue;
						}
					}

					// p.LimitToAgentId
					if (p.LimitToAgentId != null)
					{
						bool remove = false;
						do
						{
							Guid? agentId = labour.AgentId;
							if (null == agentId)
							{
								remove = true;
								break;
							}



							if (agentId.Value != Guid.Parse(p.LimitToAgentId))
							{
								remove = true;
								break;
							}



						} while (false);

						if (remove)
						{
							response.Labour.Remove(key);
							continue;
						}
					}

					// p.LimitToAssignmentId
					if (p.LimitToAssignmentId != null)
					{
						bool remove = false;
						do
						{
							Guid? assignmentId = labour.AssignmentId;
							if (null == assignmentId)
							{
								remove = true;
								break;
							}

							if (assignmentId.Value != Guid.Parse(p.LimitToAssignmentId))
							{
								remove = true;

								break;
							}


						} while (false);

						if (remove)
						{
							response.Labour.Remove(key);
							continue;
						}
					}

					// p.LimitToActiveAndToday
					if (p.LimitToActiveAndToday != null && p.LimitToActiveAndToday == true)
					{
						bool remove = false;
						do
						{
							if (string.IsNullOrWhiteSpace(labour.StartISO8601))
							{
								remove = true;
								break;
							}


							DateTime dbStart = DateTime.Parse(labour.StartISO8601, Konstants.KDefaultCulture);
							DateTime localStart = dbStart.ToLocalTime();

							DateTime localNow = DateTime.Now;
							DateTime localStartOfDay = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0, DateTimeKind.Local);
							DateTime localEndOfDay = new DateTime(localNow.Year, localNow.Month, localNow.Day, 23, 59, 59, DateTimeKind.Local);


							if (labour.IsActive == false && (localStart < localStartOfDay || localStart > localEndOfDay))
							{
								remove = true;
								break;
							}



						} while (false);

						if (remove)
						{
							response.Labour.Remove(key);
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

			await Clients.Caller.SendAsync("RequestLabourCB", response).ConfigureAwait(false);


		}
	}
}