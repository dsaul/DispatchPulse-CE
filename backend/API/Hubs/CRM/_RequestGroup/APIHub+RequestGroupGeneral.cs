using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCode;
using SharedCode.DatabaseSchemas;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		public class RequestGroupGeneralParams : IdempotencyRequest
		{
		}
		public class RequestGroupGeneralResponse : PermissionsIdempotencyResponse
		{
			public Dictionary<Guid, AssignmentStatus> AssignmentStatus { get; set; } = new Dictionary<Guid, AssignmentStatus>();
			public Dictionary<Guid, AgentsEmploymentStatus> AgentsEmploymentStatus { get; set; } = new Dictionary<Guid, AgentsEmploymentStatus>();
			public Dictionary<Guid, EstimatingManHours> EstimatingManHours { get; set; } = new Dictionary<Guid, EstimatingManHours>();
			public Dictionary<Guid, LabourSubtypeHolidays> LabourSubtypeHolidays { get; set; } = new Dictionary<Guid, LabourSubtypeHolidays>();
			public Dictionary<Guid, LabourSubtypeException> LabourSubtypeException { get; set; } = new Dictionary<Guid, LabourSubtypeException>();
			public Dictionary<Guid, LabourSubtypeNonBillable> LabourSubtypeNonBillable { get; set; } = new Dictionary<Guid, LabourSubtypeNonBillable>();
			public Dictionary<Guid, LabourTypes> LabourTypes { get; set; } = new Dictionary<Guid, LabourTypes>();
			public Dictionary<Guid, ProjectStatus> ProjectStatus { get; set; } = new Dictionary<Guid, ProjectStatus>();
			public Dictionary<Guid, SettingsDefault> SettingsDefault { get; set; } = new Dictionary<Guid, SettingsDefault>();
			public Dictionary<Guid, SettingsProvisioning> SettingsProvisioning { get; set; } = new Dictionary<Guid, SettingsProvisioning>();
			public Dictionary<Guid, SettingsUser> SettingsUser { get; set; } = new Dictionary<Guid, SettingsUser>();
		}

		public async Task RequestGroupGeneral(RequestGroupGeneralParams p)
		{


			RequestGroupGeneralResponse response = new RequestGroupGeneralResponse
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

				if (null == dpDBConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "Unable to connect to dispatch pulse database.";
					break;
				}

				response.AssignmentStatus = AssignmentStatus.All(dpDBConnection);
				response.AgentsEmploymentStatus = AgentsEmploymentStatus.All(dpDBConnection);
				response.EstimatingManHours = EstimatingManHours.All(dpDBConnection);
				response.LabourSubtypeHolidays = LabourSubtypeHolidays.All(dpDBConnection);
				response.AssignmentStatus = AssignmentStatus.All(dpDBConnection);
				response.LabourSubtypeException = LabourSubtypeException.All(dpDBConnection);
				response.LabourSubtypeNonBillable = LabourSubtypeNonBillable.All(dpDBConnection);
				response.LabourTypes = LabourTypes.All(dpDBConnection);
				response.ProjectStatus = ProjectStatus.All(dpDBConnection);
				response.SettingsDefault = SettingsDefault.All(dpDBConnection);
				response.AssignmentStatus = AssignmentStatus.All(dpDBConnection);
				response.SettingsProvisioning = SettingsProvisioning.All(dpDBConnection);
				response.SettingsUser = SettingsUser.All(dpDBConnection);

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

			await Clients.Caller.SendAsync("RequestGroupGeneralCB", response).ConfigureAwait(false);
		}
	}
}
