﻿using System;
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
		public class DeleteEstimatingManHoursParams : IdempotencyRequest
		{
			public List<Guid> EstimatingManHoursDelete { get; set; } = new List<Guid>();
		}
		public class DeleteEstimatingManHoursResponse : PermissionsIdempotencyResponse
		{
			public List<Guid> EstimatingManHoursDelete { get; set; } = new List<Guid>();
		}

		public async Task DeleteEstimatingManHours(DeleteEstimatingManHoursParams p)
		{
			DeleteEstimatingManHoursResponse response = new DeleteEstimatingManHoursResponse()
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
				if (p.EstimatingManHoursDelete == null)
				{
					response.IsError = true;
					response.ErrorMessage = "p.EstimatingManHoursDelete == null";
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

				if (p.EstimatingManHoursDelete.Count == 0)
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

				if (!permissions.Contains(EnvDatabases.kPermCRMDeleteEstimatingManHoursAny) &&
					!permissions.Contains(EnvDatabases.kPermCRMDeleteEstimatingManHoursCompany)
					)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}


				// delete

				List<Guid> affected = EstimatingManHours.Delete(dpDBConnection, p.EstimatingManHoursDelete);
				if (affected.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "No rows deleted?";
					break;
				}

				response.EstimatingManHoursDelete = affected;
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
				await Clients.Caller.SendAsync("DeleteEstimatingManHoursCB", response).ConfigureAwait(false);
			}
			else
			{
				await Clients.Group(BillingContacts.CompanyGroupNameForBillingContact(billingContact)).SendAsync("DeleteEstimatingManHoursCB", response).ConfigureAwait(false);
			}























		}
	}
}