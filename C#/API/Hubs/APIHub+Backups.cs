using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using SharedCode.DatabaseSchemas;
using SharedCode;

namespace API.Hubs
{
	public partial class APIHub : Hub
	{
		#region Create Server Side Backup

		public class CreateBackupTaskParams : IdempotencyRequest
		{
			public string? Id { get; set; }
			public Guid? SessionId { get; set; }
			public string? TzIANA { get; set; }
		}

		public class CreateBackupTaskResponse : PermissionsIdempotencyResponse
		{
			public class DB
			{
				public Dictionary<Guid, Agents> Agents { get; } = new Dictionary<Guid, Agents>();
				public Dictionary<Guid, AgentsEmploymentStatus> AgentsEmploymentStatus { get; } = new Dictionary<Guid, AgentsEmploymentStatus>();
				public Dictionary<Guid, Assignments> Assignments { get; } = new Dictionary<Guid, Assignments>();
				public Dictionary<Guid, AssignmentStatus> AssignmentStatus { get; } = new Dictionary<Guid, AssignmentStatus>();
				public Dictionary<Guid, Companies> Companies { get; } = new Dictionary<Guid, Companies>();
				public Dictionary<Guid, Contacts> Contacts { get; } = new Dictionary<Guid, Contacts>();
				public Dictionary<Guid, EstimatingManHours> EstimatingManHours { get; } = new Dictionary<Guid, EstimatingManHours>();
				public Dictionary<Guid, Labour> Labour { get; } = new Dictionary<Guid, Labour>();
				public Dictionary<Guid, LabourSubtypeException> LabourSubtypeException { get; } = new Dictionary<Guid, LabourSubtypeException>();
				public Dictionary<Guid, LabourSubtypeHolidays> LabourSubtypeHolidays { get; } = new Dictionary<Guid, LabourSubtypeHolidays>();
				public Dictionary<Guid, LabourSubtypeNonBillable> LabourSubtypeNonBillable { get; } = new Dictionary<Guid, LabourSubtypeNonBillable>();
				public Dictionary<Guid, LabourTypes> LabourTypes { get; } = new Dictionary<Guid, LabourTypes>();
				public Dictionary<Guid, Materials> Materials { get; } = new Dictionary<Guid, Materials>();
				public Dictionary<Guid, Products> Products { get; } = new Dictionary<Guid, Products>();
				public Dictionary<Guid, ProjectNotes> ProjectNotes { get; } = new Dictionary<Guid, ProjectNotes>();
				public Dictionary<Guid, Projects> Projects { get; } = new Dictionary<Guid, Projects>();
				public Dictionary<Guid, ProjectStatus> ProjectStatus { get; } = new Dictionary<Guid, ProjectStatus>();
				public Dictionary<Guid, SettingsDefault> SettingsDefault { get; } = new Dictionary<Guid, SettingsDefault>();
				public Dictionary<Guid, SettingsProvisioning> SettingsProvisioning { get; } = new Dictionary<Guid, SettingsProvisioning>();
				public Dictionary<Guid, SettingsUser> SettingsUser { get; } = new Dictionary<Guid, SettingsUser>();
				public Dictionary<Guid, Skills> Skills { get; } = new Dictionary<Guid, Skills>();
			}



			public int BackupVersion { get; set; }
			public bool ThisPCBackup { get; set; } = false;
			public DB Database { get; set; } = new DB();

		}

		public async Task CreateBackupTask(CreateBackupTaskParams p)
		{
			CreateBackupTaskResponse response = new CreateBackupTaskResponse
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

				bool permServer = permissions.Contains(EnvDatabases.kPermCRMBackupsRunServer);

				if (!permServer)
				{
					response.IsError = true;
					response.ErrorMessage = "No permissions.";
					response.IsPermissionsError = true;
					break;
				}

				response.Database.Agents.AddRange(Agents.All(dpDBConnection));
				response.Database.AgentsEmploymentStatus.AddRange(AgentsEmploymentStatus.All(dpDBConnection));
				response.Database.AssignmentStatus.AddRange(AssignmentStatus.All(dpDBConnection));
				response.Database.Assignments.AddRange(Assignments.All(dpDBConnection));
				response.Database.Companies.AddRange(Companies.All(dpDBConnection));
				response.Database.Contacts.AddRange(Contacts.All(dpDBConnection));
				response.Database.EstimatingManHours.AddRange(EstimatingManHours.All(dpDBConnection));
				response.Database.Labour.AddRange(Labour.All(dpDBConnection));
				response.Database.LabourSubtypeException.AddRange(LabourSubtypeException.All(dpDBConnection));
				response.Database.LabourSubtypeHolidays.AddRange(LabourSubtypeHolidays.All(dpDBConnection));
				response.Database.LabourSubtypeNonBillable.AddRange(LabourSubtypeNonBillable.All(dpDBConnection));
				response.Database.LabourTypes.AddRange(LabourTypes.All(dpDBConnection));
				response.Database.Materials.AddRange(Materials.All(dpDBConnection));
				response.Database.Products.AddRange(Products.All(dpDBConnection));
				response.Database.ProjectNotes.AddRange(ProjectNotes.All(dpDBConnection));
				response.Database.ProjectStatus.AddRange(ProjectStatus.All(dpDBConnection));
				response.Database.Projects.AddRange(Projects.All(dpDBConnection));
				response.Database.SettingsDefault.AddRange(SettingsDefault.All(dpDBConnection));
				response.Database.SettingsProvisioning.AddRange(SettingsProvisioning.All(dpDBConnection));
				response.Database.SettingsUser.AddRange(SettingsUser.All(dpDBConnection));
				response.Database.Skills.AddRange(Skills.All(dpDBConnection));


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

			await Clients.Caller.SendAsync("CreateBackupTaskCB", response).ConfigureAwait(false);




		}





		#endregion

	}
}
