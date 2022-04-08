using Databases.Records;
using SharedCode.DatabaseSchemas;
using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using System.Linq;
using System.Globalization;
using SharedCode.DatabaseSchemas;
using SharedCode;
using SharedCode;

namespace LaTeXGenerators
{
	public static class LaTeXAssignments
	{
		public static readonly CultureInfo Culture = new CultureInfo("en-CA");

		public static string Generate(
			NpgsqlConnection billingDB,
			NpgsqlConnection dpDB,
			bool includePreamble,
			bool includePostamble,
			IEnumerable<Assignments> assignments
			) {
			StringBuilder tex = new StringBuilder();

			if (includePreamble) {

				tex.Append(@"\documentclass[10pt, letterpaper, twoside]{article}
\usepackage[utf8]{inputenc}
\usepackage[margin=0.5in]{geometry}
\usepackage{tabularx}
\usepackage{makecell}
\usepackage{adjustbox}
\usepackage[hidelinks]{hyperref}
\usepackage{tocloft}
\usepackage{multicol}
\usepackage{xcolor}
\usepackage{blindtext}
\setlength\parindent{0pt}

\renewcommand{\cftsecleader}{\cftdotfill{\cftdotsep}}
\newcommand\mydotfill{\cftdotfill{\cftdotsep}}

\newcolumntype{Y}{>{\centering\arraybackslash}X}

\begin{document}
");

			}

			bool first = true;

			foreach (Assignments assignment in assignments) {
				if (!first) {
					tex.Append(@"\clearpage");
					tex.Append('\n');
				}

				string id = "~";
				Guid? projectId = null;
				Projects? project = null;
				string projectName = "Unnamed";
				Guid? projectStatusId = null;
				ProjectStatus? projectStatus = null;
				string projectStatusFriendly = "Unspecified";
				List<Address> projectAddresses = new List<Address>();
				List<LabeledCompanyId> projectCompanies = new List<LabeledCompanyId>();
				List<LabeledContactId> projectContacts = new List<LabeledContactId>();
				string projectStart = "Unspecified";
				string projectEnd = "Unspecified";
				HashSet<Guid> assignmentAgentIds = new HashSet<Guid>();
				string assignmentAgentFriendlyName = "Unassigned";
				Guid? assignmentStatusId = null;
				AssignmentStatus? assignmentStatus = null;
				string assignmentStatusFriendly = "Unspecified";
				string assignmentStart = "Unspecified";
				string assignmentEnd = "Unspecified";
				string workRequested = "~";
				string workPerformed = "~";
				string internalComments = "~";
				string lastActivity = "";

				if (null != assignment) {

					if (null != assignment.Id) {
						id = assignment.Id.Value.ToString().LaTeXEscape();
					}

					if (null != assignment.ProjectId) {
						projectId = assignment.ProjectId;
					}

					if (null != assignment.AgentIds && assignment.AgentIds.Count > 0) {
						assignmentAgentIds = assignment.AgentIds;


						Dictionary<Guid,Agents> results = Agents.ForIds(dpDB, new List<Guid>(assignmentAgentIds));

						if (results.Count == 0) {
							assignmentAgentFriendlyName = "No Agents";
						} else {

							StringBuilder sb = new StringBuilder();

							var keys = results.Keys.ToList();
							for (int i=0; i<keys.Count; i++) {
								Agents agent = results[keys[i]];

								sb.Append(agent.Name);
								if (i != keys.Count-1) {
									sb.Append(", ");
								}

							}

							assignmentAgentFriendlyName = sb.ToString();


						}

					}

					if (null != assignment.StatusId) {
						assignmentStatusId = assignment.StatusId.Value;

						Dictionary<Guid,AssignmentStatus> results = AssignmentStatus.ForId(dpDB, assignmentStatusId.Value);
						

						if (results.Count > 0) {

							assignmentStatus = results.FirstOrDefault().Value;


							assignmentStatusFriendly =
								assignmentStatus == null || string.IsNullOrWhiteSpace(assignmentStatus.Name) ?
								"Unnamed status." : assignmentStatus.Name;

						}


					}




					assignment.GetSchedule(
						dpDB,
						out bool? usingProjectSchedule,
						out bool? hasStartISO8601,
						out string? startTimeMode,
						out string? startISO8601,
						out bool? hasEndISO8601,
						out string? endTimeMode,
						out string? endISO8601
						);



					if (null != hasStartISO8601 && true == hasStartISO8601.Value && null != startISO8601 && !string.IsNullOrWhiteSpace(startISO8601)) {

						DateTime dateISO = DateTime.Parse(startISO8601);
						DateTime dateLocal = dateISO.ToLocalTime();

						string dateFormat = dateLocal.ToString("MMM d, yyyy", Culture);
						string dateTimeFormat = dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture);

						// DATE_MED Oct 14, 1983
						// DATETIME_MED Oct 14, 1983, 9:30 AM
						// https://www.c-sharpcorner.com/blogs/date-and-time-format-in-c-sharp-programming1

						assignmentStart = startTimeMode switch {
							"morning-first-thing" => $"Start \\enspace\\mydotfill\\enspace {dateFormat.LaTeXEscape()}, first thing in the morning\\\n",
							"morning-second-thing" => $"Start \\enspace\\mydotfill\\enspace {dateFormat.LaTeXEscape()}, second thing in the morning\\\n",
							"afternoon-first-thing" => $"Start \\enspace\\mydotfill\\enspace {dateFormat.LaTeXEscape()}, first thing in the afternoon\\\n",
							"afternoon-second-thing" => $"Start \\enspace\\mydotfill\\enspace {dateFormat.LaTeXEscape()}, second thing in the afternoon\\\n",
							"time" => $"Start \\enspace\\mydotfill\\enspace {dateTimeFormat.LaTeXEscape()}\\\n",
							_ => $"Start \\enspace\\mydotfill\\enspace {dateFormat.LaTeXEscape()}\\\n",
						};
					}


					if (null != hasEndISO8601 && true == hasEndISO8601.Value && !string.IsNullOrWhiteSpace(endISO8601)) {

						DateTime dateISO = DateTime.Parse(endISO8601);
						DateTime dateLocal = dateISO.ToLocalTime();

						string dateFormat = dateLocal.ToString("MMM d, yyyy", Culture);
						string dateTimeFormat = dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture);

						// DATE_MED Oct 14, 1983
						// DATETIME_MED Oct 14, 1983, 9:30 AM
						// https://www.c-sharpcorner.com/blogs/date-and-time-format-in-c-sharp-programming1

						assignmentEnd = endTimeMode switch {
							"time" => $"End\\enspace\\mydotfill\\enspace {dateTimeFormat.LaTeXEscape()}\\\n",
							_ => $"End\\enspace\\mydotfill\\enspace {dateFormat.LaTeXEscape()}\\\n",
						};
					}

					if (null != assignment.WorkRequested && !string.IsNullOrWhiteSpace(assignment.WorkRequested)) {
						workRequested = assignment.WorkRequested.LaTeXEscape();
					}

					// Work performed
					do {
						StringBuilder sb = new StringBuilder();

						var results = ProjectNotes.ForAssignmentId(dpDB, assignment.Id);
						
						foreach (KeyValuePair<Guid, ProjectNotes> kvp in results) {

							// Don't put internal entries in here.
							if (null != kvp.Value.InternalOnly && true == kvp.Value.InternalOnly.Value) {
								continue;
							}

							sb.Append(kvp.Value.GetLaTeXString(billingDB));
							sb.Append("\n\n");
						}


						workPerformed = sb.ToString().LaTeXEscape();

					}
					while (false);

					// Internal Comments
					do {
						StringBuilder sb = new StringBuilder();

						var results = ProjectNotes.ForAssignmentId(dpDB, assignment.Id);

						foreach (KeyValuePair<Guid, ProjectNotes> kvp in results) {

							// Don't put public entries in here.
							if (null != kvp.Value.InternalOnly && false == kvp.Value.InternalOnly.Value) {
								continue;
							}

							sb.Append(kvp.Value.GetLaTeXString(billingDB));
							sb.Append("\n\n");
						}


						internalComments = sb.ToString().LaTeXEscape();
					}
					while (false);



				}

				if (null != projectId) {

					var results = Projects.ForId(dpDB, projectId.Value);
					if (results.Count > 0) {
						project = results.FirstOrDefault().Value;
					}
				}

				if (null != project) {
					if (false == string.IsNullOrWhiteSpace(project.Name)) {
						projectName = project.Name.LaTeXEscape();
					}

					if (null != project.StatusId) {

						projectStatusId = project.StatusId.Value;

						var res = ProjectStatus.ForId(dpDB, projectStatusId.Value);
						if (null != res && res.Count > 0) {
							projectStatus = res.FirstOrDefault().Value;
							projectStatusFriendly = null != projectStatus && null != projectStatus.Name ? projectStatus.Name : "Unnamed Status";
						}
					}


					
					// last activity
					if (null != project.LastModifiedBillingId || false == string.IsNullOrWhiteSpace(project.LastModifiedIso8601)) {

						// date
						if (false == string.IsNullOrWhiteSpace(project.LastModifiedIso8601)) {



							DateTime dateDB = DateTime.Parse(project.LastModifiedIso8601);
							DateTime dateLocal = dateDB.ToLocalTime();
							string dateStr = dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture);

							lastActivity += $" {dateStr}".LaTeXEscape();
						}


						// who
						if (null != project.LastModifiedBillingId) {


							var res = BillingContacts.ForId(billingDB, project.LastModifiedBillingId.Value);
							if (null != res && res.Count > 0) { 
								
								BillingContacts modBillContact = res.FirstOrDefault().Value;

								if (string.IsNullOrWhiteSpace(modBillContact.FullName)) {
									lastActivity += $" by ${modBillContact.FullName} ".LaTeXEscape();
								} else {
									lastActivity += " by Unknown ".LaTeXEscape();
								}

							} else {
								lastActivity += " by Unknown ".LaTeXEscape();
							}

						} else {
							lastActivity += " by Unknown ".LaTeXEscape();
						}

					} else {

						lastActivity += "Unknown".LaTeXEscape();

					}


					projectAddresses = project.Addresses.ToList();
					projectCompanies = project.Companies.ToList();
					projectContacts = project.Contacts.ToList();


					if (project.HasStartISO8601 && !string.IsNullOrWhiteSpace(project.StartISO8601)) {

						DateTime dateDB = DateTime.Parse(project.StartISO8601);
						DateTime dateLocal = dateDB.ToLocalTime();

						projectStart = project.StartTimeMode switch {
							Projects.StartTimeModeE.MorningFirstThing => $"Start \\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}, first thing in the morning\\\n",
							Projects.StartTimeModeE.MorningSecondThing => $"Start \\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}, second thing in the morning\\\n",
							Projects.StartTimeModeE.AfternoonFirstThing => $"Start \\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}, first thing in the afternoon\\\n",
							Projects.StartTimeModeE.AfternoonSecondThing => $"Start \\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}, second thing in the afternoon\\\n",
							Projects.StartTimeModeE.Time => $"Start \\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture).LaTeXEscape()}\\\n",
							_ => $"Start \\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}\\\n",
						};
					}
			
			
					if (project.HasEndISO8601 && !string.IsNullOrWhiteSpace(project.EndISO8601)) {

						DateTime dateDB = DateTime.Parse(project.EndISO8601);
						DateTime dateLocal = dateDB.ToLocalTime();

						projectEnd = project.EndTimeMode switch {
							Projects.EndTimeModeE.Time => $"End\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture).LaTeXEscape()}\\\n",
							_ => $"End\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}\\\n",
						};
					}
			
			
				}



				tex.Append(@"\begin{flushleft}
		\begin{tabularx}{\textwidth}{lYr}
			\begin{tabular}[t]{l}
				 %\textbf{COMPANY_NAME}\medskip~\\
				 
			\end{tabular}& 
			
			\begin{tabular}[t]{l}
				%\small Phone\enspace\mydotfill\enspace \href{tel:COMPANY_CONTACT_NUMBER}{COMPANY_CONTACT_NUMBER} \\
				%\small EMail\enspace\mydotfill\enspace \href{mailto:COMPANY_CONTACT_EMAIL}{COMPANY_CONTACT_EMAIL} \\
				%Mail\enspace\mydotfill\enspace\shortstack[r]{
				%	COMPANY_MAILING_ADDR_L1 \\ COMPANY_MAILING_ADDR_L2
				%}
			\end{tabular}
			
			& 
			\begin{tabular}[t]{r}
				\textbf{Assignment}\medskip~\\");
				tex.Append('\n');
				tex.Append($"ID\\enspace\\mydotfill\\enspace \\href{{{Konstants.APP_BASE_URI}section/assignments/{id}?tab=General}}{{{id}}} \\\\\n");
				tex.Append(@"Printed\enspace\mydotfill\enspace\today \\
				~ \\
			\end{tabular}
			 \\
		\end{tabularx}
	\end{flushleft}
	\vspace{-1.4\baselineskip}
	\textbf{Project}\hrulefill
	\begin{multicols}{2}
		\begin{minipage}{\columnwidth}");
				tex.Append('\n');
				tex.Append($"Name\\enspace\\mydotfill\\enspace \\href{{{Konstants.APP_BASE_URI}section/projects/{projectId}?tab=General}}{{{projectName}}}  \\\\\n");
				tex.Append(@"Status\enspace\mydotfill\enspace ");
				tex.Append(projectStatusFriendly);
				tex.Append(@" \\
		Last Modified\enspace\mydotfill\enspace ");
				tex.Append(lastActivity);
				tex.Append(@" \\
		\end{minipage}
		\begin{minipage}{\columnwidth}
		\textbf{Addresses:} \\");

				if (projectAddresses.Count == 0) {
					tex.Append("'No addresses on project.\\\\\n");
				} else {
					foreach (Address addr in projectAddresses) {
						string label = addr.Label.LaTeXEscape();
						string value = addr.Value.LaTeXEscape();
				
						tex.Append($"{label} \\enspace\\mydotfill\\enspace {value} \\\\\n");
					}
				}
		
		
		
		
				tex.Append(@"\end{minipage}
			\begin{minipage}{\columnwidth}
			\textbf{Companies:} \\");
				tex.Append('\n');
		
				if (projectCompanies.Count == 0) {
					tex.Append("No companies on project.\\\\\n");
				} else {

					foreach (LabeledCompanyId entry in projectCompanies) {

						string companyLabel = entry.Label.LaTeXEscape();
						Guid? companyId = entry.Value;
						string companyName = "Not Specified";

						if (null != companyId) {

							var results = Companies.ForId(dpDB, companyId.Value);
							if (null != results && results.Count > 0) {
								Companies company = results.FirstOrDefault().Value;

								companyName = null == company || string.IsNullOrWhiteSpace(company.Name) ? "Unnamed Company" : company.Name.LaTeXEscape();
							}
						}

						tex.Append($"{companyLabel} \\enspace\\mydotfill\\enspace \\href{{{Konstants.APP_BASE_URI}section/companies/{companyId}?tab=General}}{{{companyName}}} \\\\\n");
					}

					
				}
		
		
		
				tex.Append(@"\end{minipage}
				\begin{minipage}{\columnwidth}
				\textbf{Contacts:} \\");
				tex.Append('\n');
		
				if (projectContacts.Count == 0) {
					tex.Append("No contacts on project.\\\\\n");
				} else {

					foreach (LabeledContactId contact in projectContacts) {
						string contactLabel = contact.Label.LaTeXEscape();
						Guid? contactId = contact.Value;
						string contactName = "Not Specified";

						if (null != contactId) {

							var results = Contacts.ForId(dpDB, contactId.Value);
							if (null != results && results.Count > 0) {
								Contacts contactObj = results.FirstOrDefault().Value;

								contactName = null == contactObj || string.IsNullOrWhiteSpace(contactObj.Name) ? "Unnamed Contact" : contactObj.Name.LaTeXEscape();
							}
						}

						tex.Append($"{contactLabel} \\enspace\\mydotfill\\enspace \\href{{{Konstants.APP_BASE_URI}section/contacts/{contactId}?tab=General}}{{{contactName}}} \\\\\n");
					}

				}

				tex.Append(@"\end{minipage}
		\begin{minipage}{\columnwidth}
		\textbf{Project Schedule:} \\");
				tex.Append('\n');


				tex.Append(projectStart);
				tex.Append('\n');


				tex.Append(projectEnd);
				tex.Append('\n');





				tex.Append(@"\end{minipage}
	\end{multicols}
	\vspace{-.7\baselineskip}
	\textbf{Assignment}\hrulefill
	\begin{multicols}{2}");
				tex.Append('\n');


				tex.Append($"Agent\\enspace\\mydotfill\\enspace {assignmentAgentFriendlyName} \\\\\n");

				tex.Append($"Status\\enspace\\mydotfill\\enspace {assignmentStatusFriendly} \\\\\n");

				tex.Append(@"\begin{minipage}{\columnwidth}
			\textbf{Assignment Schedule:} \\");
				tex.Append('\n');


				tex.Append($"{assignmentStart}\n");

				tex.Append($"{assignmentEnd}\n");




				tex.Append(@"\end{minipage}
	\end{multicols}
	\vspace{-.7\baselineskip}
	\textbf{Work Requested}\hrulefill\medskip~\\");
				tex.Append('\n');

				tex.Append($"{workRequested} \\medskip~\\\\\n");
				tex.Append($"\\textbf{{Work Performed}}\\hrulefill\\medskip~\\\\\n");
				tex.Append($"{workPerformed} \\medskip~\\\\\n");
				tex.Append($"\\textbf{{Internal Comments}}\\hrulefill\\medskip~\\\\");
				tex.Append($"{internalComments} \\medskip\n");
				tex.Append(@"\textbf{Notes}\hrulefill\medskip~\\
	\textcolor{black!20}{\underline{\hspace{\textwidth}}}
	\textcolor{black!20}{\underline{\hspace{\textwidth}}}
	\textcolor{black!20}{\underline{\hspace{\textwidth}}}
	\textcolor{black!20}{\underline{\hspace{\textwidth}}}
	\textcolor{black!20}{\underline{\hspace{\textwidth}}}
	\textcolor{black!20}{\underline{\hspace{\textwidth}}}
	\textcolor{black!20}{\underline{\hspace{\textwidth}}}
	\textcolor{black!20}{\underline{\hspace{\textwidth}}}
	
	
	\vfill
	\hrulefill \\");
					tex.Append('\n');
	
				first = false;
			}
	
	
	

	
			if (includePostamble) {
				tex.Append("\\end{document}\n");
			}
	
	
			return tex.ToString();
		}
	}
}
