using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SharedCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharedCode.DatabaseSchemas;
using SharedCode.DatabaseSchemas;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestAssignmentsParams : IdempotencyRequest
		{
			public Guid? SessionId { get; set; }
			public List<Guid> LimitToIds { get; set; } = new List<Guid>();


			public bool? LimitToSessionAgent { get; set; }
			public bool? LimitToOpen { get; set; }
			public bool? LimitToClosed { get; set; }
			public bool? LimitToTodayOrEarlier { get; set; }
			public bool? LimitToTasksWithNoStartTime { get; set; }
			public string? LimitToProjectId { get; set; }
			public string? LimitToAgentId { get; set; }
			public bool? LimitToPastDue { get; set; }
			public bool? LimitToUnassigned { get; set; }
			public bool? LimitToDueWithNoLabour { get; set; }
			public bool? LimitToBillableReview { get; set; }

			public bool? FilterAssignmentsWithNoStartTime { get; set; }
			public bool? ShowChildrenOfProjectIdAsWell { get; set; }
		}
		public class RequestAssignmentsResponse : PermissionsIdempotencyResponse
		{
			public Dictionary<Guid, Assignments> Assignments { get; set; } = new Dictionary<Guid, Assignments>();
		}

		public async Task RequestAssignments(RequestAssignmentsParams p)
		{

			RequestAssignmentsResponse response = new RequestAssignmentsResponse
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

				bool permAny = permissions.Contains(EnvDatabases.kPermCRMRequestAssignmentsAny);
				bool permCompany = permissions.Contains(EnvDatabases.kPermCRMRequestAssignmentsCompany);
				bool permSelf = true; // kPermCRMRequestAssignmentsSelf

				if (permAny || permCompany || permSelf)
				{
					if (p.LimitToIds == null || p.LimitToIds.Count == 0)
					{
						response.Assignments = Assignments.All(dpDBConnection);
					}
					else
					{
						response.Assignments = Assignments.ForIds(dpDBConnection, p.LimitToIds);
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
				List<Guid> keys = response.Assignments.Keys.ToList();





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
					// Only continue if we remove this key from the results.
					
					Assignments assignment = response.Assignments[key];

					// LimitToSessionAgent
					if (p.LimitToSessionAgent != null && p.LimitToSessionAgent == true)
					{
						
						bool remove = false;
						do {
							
							Guid? sessionAgentId = billingContact.DPAgentId;
							if (null == sessionAgentId)
							{
								remove = true;
								break;
							}

							HashSet<Guid> agents = assignment.AgentIds;
							if (agents.Count == 0)
							{
								remove = true;
								break;
							}

							if (!agents.Contains(sessionAgentId.Value)) {
								remove = true;
								break;
							}




						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}





					// LimitToOpen
					if (p.LimitToOpen != null && p.LimitToOpen == true)
					{
						bool remove = false;
						do
						{
							Guid? statusId = assignment.StatusId;
							if (null == statusId)
							{
								remove = true;
								break;
							}


							Dictionary<Guid, AssignmentStatus> statuses = AssignmentStatus.ForIds(
								dpDBConnection, new List<Guid>() { statusId.Value }
							);

							if (statuses.Count == 0)
							{
								remove = true;
								break;
							}

							bool? isOpen = statuses.First().Value.IsOpen;
							if (null == isOpen)
							{
								remove = true;
								break;
							}

							if (false == isOpen.Value)
							{
								remove = true;
								break;
							}




						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}



					// LimitToClosed
					if (p.LimitToClosed != null && p.LimitToClosed == true)
					{
						bool remove = false;
						do
						{

							Guid? statusId = assignment.StatusId;
							if (null == statusId)
							{
								remove = true;
								break;
							}

							Dictionary<Guid, AssignmentStatus> statuses = AssignmentStatus.ForIds(
								dpDBConnection, new List<Guid>() { statusId.Value }
							);

							if (statuses.Count == 0)
							{
								remove = true;
								break;
							}

							bool? isOpen = statuses.First().Value.IsOpen;
							if (null == isOpen)
							{
								remove = true;
								break;
							}

							if (true == isOpen.Value)
							{
								remove = true;
								break;
							}

						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}



					// LimitToTodayOrEarlier
					if (p.LimitToTodayOrEarlier != null && p.LimitToTodayOrEarlier == true)
					{
						bool remove = false;
						do
						{

							// We need to check the project to see if we are forced to use the 
							// project's schedule rather then the assignments.

							if (null == assignment)
							{
								remove = true;
								break;
							}

							JObject? assignmentRoot = assignment.JsonObject;
							if (null == assignmentRoot)
							{
								remove = true;
								break;
							}

							bool? hasStartISO8601 = null;
							string? startISO8601 = null;

							assignment.GetSchedule(
								dpDBConnection,
								out _,
								out hasStartISO8601,
								out _,
								out startISO8601,
								out _,
								out _,
								out _);


							// Unscheduled could be today.
							if (null == hasStartISO8601 || false == hasStartISO8601.Value || string.IsNullOrWhiteSpace(startISO8601))
							{
								break;
							}

							// Schedled must be before the end of today. We don't want to do timezone math, so send the next day as well.
							DateTime dtAssignment = DateTime.Parse(startISO8601, Konstants.KDefaultCulture);

							DateTime utcNow = DateTime.UtcNow;
							DateTime utcTomorrow = utcNow.AddDays(1);

							if (dtAssignment > utcTomorrow)
							{
								remove = true;
								break;
							}

						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}

					// LimitToTasksWithNoStartTime
					if (p.LimitToTasksWithNoStartTime != null && p.LimitToTasksWithNoStartTime == true)
					{
						bool remove = false;
						do
						{
							// We need to check the project to see if we are forced to use the 
							// project's schedule rather then the assignments.

							if (null == assignment)
							{
								remove = true;
								break;
							}


							JObject? assignmentRoot = assignment.JsonObject;
							if (null == assignmentRoot)
							{
								remove = true;
								break;
							}

							bool? hasStartISO8601 = null;

							assignment.GetSchedule(
								dpDBConnection,
								out _,
								out hasStartISO8601,
								out _,
								out _,
								out _,
								out _,
								out _);

							if (null != hasStartISO8601 && hasStartISO8601.Value)
							{
								remove = true;
								break;
							}


						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}

					// LimitToProjectId
					if (p.LimitToProjectId != null && false == string.IsNullOrWhiteSpace(p.LimitToProjectId))
					{
						bool remove = false;
						do
						{
							if (null == assignment)
							{
								remove = true;
								break;
							}

							Guid? mainProject = assignment.ProjectId;
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
							response.Assignments.Remove(key);
							continue;
						}
					}

					// LimitToAgentId
					if (p.LimitToAgentId != null && false == string.IsNullOrWhiteSpace(p.LimitToAgentId))
					{
						bool remove = false;
						do
						{
							if (null == assignment)
							{
								remove = true;
								break;
							}

							JObject? assignmentRoot = assignment.JsonObject;
							if (null == assignmentRoot)
							{
								remove = true;
								break;
							}


							Guid limitToParsed;
							if (!Guid.TryParse(p.LimitToAgentId, out limitToParsed))
							{
								remove = true;
								break;
							}



							HashSet<Guid> agentIds = assignment.AgentIds;
							if (agentIds.Count == 0)
							{
								remove = true;
								break;
							}

							if (!agentIds.Contains(limitToParsed))
							{
								remove = true;
								break;
							}


						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}

					// LimitToPastDue
					if (p.LimitToPastDue != null && p.LimitToPastDue == true)
					{
						bool remove = false;
						do
						{
							if (null == assignment)
							{
								remove = true;
								break;
							}

							JObject? assignmentRoot = assignment.JsonObject;
							if (null == assignmentRoot)
							{
								remove = true;
								break;
							}

							bool? hasEndISO8601 = null;
							string? endTimeMode = null;
							string? endISO8601 = null;

							assignment.GetSchedule(
								dpDBConnection,
								out _,
								out _,
								out _,
								out _,
								out hasEndISO8601,
								out endTimeMode,
								out endISO8601
								);

							if (hasEndISO8601 == false ||
								string.IsNullOrWhiteSpace(endISO8601))
							{
								remove = true;
								break;
							}

							DateTime dtAssignmentEnd = DateTime.Parse(endISO8601, Konstants.KDefaultCulture);

							// We expand the range we send back to account for weird timezone issues.
							DateTime dtUtc = DateTime.UtcNow;
							DateTime dtUtcPlus1Day = dtUtc.AddDays(1);

							if (dtAssignmentEnd > dtUtcPlus1Day)
							{
								remove = true;
								break;
							}


						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}

					// LimitToUnassigned
					if (p.LimitToUnassigned != null && p.LimitToUnassigned == true)
					{
						bool remove = false;
						do
						{
							if (null == assignment)
							{
								remove = true;
								break;
							}

							JObject? assignmentRoot = assignment.JsonObject;
							if (null == assignmentRoot)
							{
								remove = true;
								break;
							}

							HashSet<Guid> agentIds = assignment.AgentIds;

							
							if (agentIds.Count != 0)
							{
								remove = true;
								break;
							}


						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}

					// LimitToDueWithNoLabour
					if (p.LimitToDueWithNoLabour != null && p.LimitToDueWithNoLabour == true)
					{
						bool remove = false;
						do
						{
							if (null == assignment || null == assignment.Id)
							{
								remove = true;
								break;
							}

							bool? hasStartISO8601;
							string? startTimeMode;
							string? startISO8601;

							assignment.GetSchedule(
								dpDBConnection,
								out _,
								out hasStartISO8601,
								out startTimeMode,
								out startISO8601,
								out _,
								out _,
								out _
								);


							if (null == hasStartISO8601 || false == hasStartISO8601.Value || string.IsNullOrWhiteSpace(startISO8601))
							{
								remove = true;
								break;
							}


							DateTime dueTime = DateTime.Parse(startISO8601, Konstants.KDefaultCulture);
							DateTime nowUTC = DateTime.UtcNow;

							if (dueTime > nowUTC) // not due
							{
								remove = true;
								break;
							}

							Dictionary<Guid,Labour> labour = Labour.ForAssignmentIds(dpDBConnection, new List<Guid> { assignment.Id.Value });
							if (labour.Count > 0)
							{
								remove = true;
								break;
							}

						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}

					// LimitToBillableReview
					if (p.LimitToBillableReview != null && p.LimitToBillableReview == true)
					{
						bool remove = false;
						do
						{
							if (null == assignment)
							{
								remove = true;
								break;
							}

							Guid? statusId = assignment.StatusId;
							if (null == statusId)
							{
								remove = true;
								break;
							}

							Dictionary<Guid, AssignmentStatus> statuses = AssignmentStatus.ForIds(
								dpDBConnection, new List<Guid>() { statusId.Value }
							);

							if (statuses.Count == 0)
							{
								remove = true;
								break;
							}

							bool? billableReview = statuses.First().Value.IsBillableReview;
							if (null == billableReview)
							{
								remove = true;
								break;
							}

							if (false == billableReview.Value)
							{
								remove = true;
								break;
							}

						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
							continue;
						}
					}

					// FilterAssignmentsWithNoStartTime
					if (p.FilterAssignmentsWithNoStartTime != null && p.FilterAssignmentsWithNoStartTime == true)
					{
						bool remove = false;
						do
						{
							if (null == assignment)
							{
								remove = true;
								break;
							}

							bool? hasStartISO8601 = null;
							string? startISO8601 = null;

							assignment.GetSchedule(
								dpDBConnection,
								out _,
								out hasStartISO8601,
								out _,
								out startISO8601,
								out _,
								out _,
								out _);

							if (string.IsNullOrWhiteSpace(startISO8601) || hasStartISO8601 == null || hasStartISO8601 == false)
							{
								remove = true;
								break;
							}












						} while (false);

						if (remove)
						{
							response.Assignments.Remove(key);
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

			await Clients.Caller.SendAsync("RequestAssignmentsCB", response).ConfigureAwait(false);

		}
	}
}