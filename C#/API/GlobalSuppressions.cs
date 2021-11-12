
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>", Scope = "type", Target = "~T:API.Program")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "member", Target = "~M:API.Utility.NpgsqlCommandExecuteFile.ExecuteFileFromResourceName(Npgsql.NpgsqlCommand,System.String,System.Reflection.Assembly)~System.Int32")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "member", Target = "~M:API.Utility.NpgsqlCommandExecuteFile.ExecuteFile(Npgsql.NpgsqlCommand,System.String,System.Text.Encoding)~System.Int32")]
[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "member", Target = "~M:API.Hubs.APIHub.RequestBillingPermissionsGroupsForCurrentSession(API.Hubs.APIHub.RequestBillingPermissionsBoolParams)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:API.Hubs.APIHub.PerformRegisterCreateDPDatabase(API.Hubs.APIHub.RegisterCreateDPDatabaseParams)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "member", Target = "~M:API.Hubs.APIHub.PerformRegisterCreateDPDatabase(API.Hubs.APIHub.RegisterCreateDPDatabaseParams)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>", Scope = "member", Target = "~M:API.Hubs.APIHub.CompanyGroupNameForBillingContact(Databases.Records.Billing.BillingContacts)~System.String")]
[assembly: SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>", Scope = "member", Target = "~M:API.Hubs.APIHub.UserGroupNameForBillingContact(Databases.Records.Billing.BillingContacts)~System.String")]
[assembly: SuppressMessage("Style", "IDE0031:Use null propagation", Justification = "<Pending>", Scope = "member", Target = "~M:API.Hubs.APIHub.PerformARIMarkAssignmentAsTravelling(API.Hubs.PerformARIMarkAssignmentAsTravellingParams)~System.Threading.Tasks.Task{API.Hubs.PerformARIMarkAssignmentAsTravellingResponse}")]
