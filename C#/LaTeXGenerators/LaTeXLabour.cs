using SharedCode.DatabaseSchemas;
using NodaTime;
using NodaTime.Text;
using Npgsql;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTeXGenerators
{
	public static class LaTeXLabour
	{
		private static readonly object _lockObject = new object();

		public static async Task<string> Generate(
			NpgsqlConnection billingDB,
			NpgsqlConnection dpDB,
			bool includePreamble,
			bool includePostamble,
			IEnumerable<Labour> labour
			) {
			StringBuilder tex = new StringBuilder();

			if (includePreamble) {
				tex.Append(@"\documentclass[12pt, letterpaper, twoside]{article}");
				tex.Append('\n');
				tex.Append(@"\usepackage{longtable}");
				tex.Append('\n');
				tex.Append(@"\usepackage[margin=0.5in]{geometry}");
				tex.Append('\n');
				tex.Append(@"\usepackage{booktabs}");
				tex.Append('\n');
				tex.Append(@"\usepackage{rotating}");
				tex.Append('\n');
				tex.Append(@"\usepackage{dashrule}");
				tex.Append('\n');
				tex.Append(@"\usepackage{amssymb}");
				tex.Append('\n');
				tex.Append(@"\usepackage{pdfrender}");
				tex.Append('\n');
				tex.Append(@"\usepackage[skip=0pt]{caption}");
				tex.Append('\n');
				tex.Append(@"\usepackage[table]{xcolor}");
				tex.Append('\n');
				tex.Append(@"\usepackage{arydshln}");
				tex.Append('\n');
				tex.Append(@"\usepackage[utf8]{inputenc}");
				tex.Append('\n');
				tex.Append(@"\usepackage{pdflscape}");
				tex.Append('\n');
				tex.Append(@"\usepackage[hidelinks]{hyperref}");
				tex.Append('\n');
				
				tex.Append(@"\newcommand *{\boldcheckmark}{%
					\textpdfrender{
						TextRenderingMode=FillStroke,
						LineWidth=.5pt, % half of the line width is outside the normal glyph
					}{\checkmark}%
				}");
				tex.Append('\n');

				tex.Append(@"\begin{document}");
				tex.Append('\n');
			}
	
			tex.Append("\\begin{landscape}\n");
			tex.Append(@"\newcolumntype{P}[2]{%
			>{\begin{turn}{#1}\begin{minipage}{#2}\small\raggedright\hspace{0pt}}l%
					<{\end{minipage}\end{turn}}%
		}
		
		\begin{center}
			\begin{longtable}{|p{2cm}:p{3cm}:p{10cm}:p{1.5cm}:p{0.4cm}:p{0.4cm}:p{0.4cm}:p{0.4cm}:p{0.4cm}:p{0.4cm}:p{0.4cm}|}
				\caption*{Labour} \\
				
				\hline
				\multicolumn{1}{|c|}{\textbf{Date}} &
				\multicolumn{1}{c|}{\textbf{Agent}} &
				\multicolumn{1}{c|}{\textbf{Description}} &
				\multicolumn{1}{c|}{\textbf{Hours}} &
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Billable} \\ \smallskip}} &
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Travel} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{On Site} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Remote} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Extra} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Billed} \\ \smallskip}} &
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Paid Out} \\ \smallskip}}\\
				\hline 
				\endfirsthead
				
				\multicolumn{11}{c}{{Labour -- continued from previous page}} \\
				\hline
				\multicolumn{1}{|c|}{\textbf{Date}} &
				\multicolumn{1}{c|}{\textbf{Agent}} &
				\multicolumn{1}{c|}{\textbf{Description}} &
				\multicolumn{1}{c|}{\textbf{Hours}} &
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Billable} \\ \smallskip}} &
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Travel} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{On Site} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Remote} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Extra} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Billed} \\ \smallskip}} &
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Paid Out} \\ \smallskip}}\\
				\hline 
				\endhead
				
				\hline
				\multicolumn{11}{r}{{Continued on next page}} \\
				%\hline
				\endfoot
				
				\hline
				\endlastfoot
				");


			var labourSorted = from l in labour
							   orderby l.StartISO8601 descending
							   select l;



			Labour[] labourArray = labourSorted.ToArray();

			decimal hoursTotal = 0;

			

			// Fetch objects
			HashSet<Guid> projectIdToFetch = new HashSet<Guid>(labourArray.Length);
			HashSet<Guid> agentIdToFetch = new HashSet<Guid>();
			HashSet<Guid> assignmentIdToFetch = new HashSet<Guid>();
			HashSet<Guid> exceptionTypeIdToFetch = new HashSet<Guid>();
			HashSet<Guid> holidayTypeIdToFetch = new HashSet<Guid>();
			HashSet<Guid> nonBillableTypeIdToFetch = new HashSet<Guid>();
			HashSet<Guid> labourTypesToFetch = new HashSet<Guid>();

			foreach (Labour entry in labourArray) {

				if (null != entry.ProjectId) {
					projectIdToFetch.Add(entry.ProjectId.Value);
				}

				if (null != entry.AgentId) {
					agentIdToFetch.Add(entry.AgentId.Value);
				}

				if (null != entry.AssignmentId) {
					assignmentIdToFetch.Add(entry.AssignmentId.Value);
				}

				if (null != entry.ExceptionTypeId) {
					exceptionTypeIdToFetch.Add(entry.ExceptionTypeId.Value);
				}

				if (null != entry.HolidayTypeId) {
					holidayTypeIdToFetch.Add(entry.HolidayTypeId.Value);
				}

				if (null != entry.NonBillableTypeId) {
					nonBillableTypeIdToFetch.Add(entry.NonBillableTypeId.Value);
				}

				if (null != entry.TypeId) {
					labourTypesToFetch.Add(entry.TypeId.Value);
				}

			}

			Dictionary<Guid, Projects> projectsCache = Projects.ForIds(dpDB, projectIdToFetch);
			Dictionary<Guid, Agents> agentsCache = Agents.ForIds(dpDB, agentIdToFetch);
			Dictionary<Guid, Assignments> assignmentsCache = Assignments.ForIds(dpDB, assignmentIdToFetch);
			Dictionary<Guid, LabourSubtypeException> exceptionCache = LabourSubtypeException.ForIds(dpDB, exceptionTypeIdToFetch);
			Dictionary<Guid, LabourSubtypeHolidays> holidayCache = LabourSubtypeHolidays.ForIds(dpDB, holidayTypeIdToFetch);
			Dictionary<Guid, LabourSubtypeNonBillable> nonBillableCache = LabourSubtypeNonBillable.ForIds(dpDB, nonBillableTypeIdToFetch);
			Dictionary<Guid, LabourTypes> labourTypesCache = LabourTypes.ForIds(dpDB, labourTypesToFetch);


			string ProcessEntry(Labour entry) {



				//Console.Write(".");

				Agents? agent = null;
				Projects? project = null;
				Assignments? assignment = null;
				LabourSubtypeException? exception = null;
				LabourSubtypeHolidays? holiday = null;
				LabourSubtypeNonBillable? nonBillable = null;
				LabourTypes? type = null;

				string date = "";
				string agentName = "";
				string description = "";
				string hours = "";
				string billable = "";
				string travel = "";
				string onSite = "";
				string remote = "";
				string extra = "";
				string billed = "";
				string paidOut = "";

				if (null != entry) {
					if (!string.IsNullOrWhiteSpace(entry.StartISO8601)) {
						ParseResult<Instant> pr = InstantPattern.ExtendedIso.Parse(entry.StartISO8601);

						if (pr.Success) {
							Instant instant = pr.Value;
#warning TODO: CORRECT THIS REPORT
							var timeZone = DateTimeZoneProviders.Tzdb[BillingCompanies.kJsonValueIANATimezoneDefault];
							ZonedDateTime dtWpg = instant.InZone(timeZone);
							date = dtWpg.ToString(@"yyyy-MM-dd", new CultureInfo("en-CA")).LaTeXEscape();
						}

					}

					if (null != entry.AgentId) {
						agent = agentsCache.GetValueOrDefault(entry.AgentId.Value);
					}
					if (null != entry.ProjectId) {
						project = projectsCache.GetValueOrDefault(entry.ProjectId.Value);
					}
					if (null != entry.ExceptionTypeId) {
						exception = exceptionCache.GetValueOrDefault(entry.ExceptionTypeId.Value);
					}
					if (null != entry.HolidayTypeId) {
						holiday = holidayCache.GetValueOrDefault(entry.HolidayTypeId.Value);
					}
					if (null != entry.NonBillableTypeId) {
						nonBillable = nonBillableCache.GetValueOrDefault(entry.NonBillableTypeId.Value);
					}
					if (null != entry.AssignmentId) {
						assignment = assignmentsCache.GetValueOrDefault(entry.AssignmentId.Value);
					}
					if (null != entry.TypeId) {
						type = labourTypesCache.GetValueOrDefault(entry.TypeId.Value);
					}


					//
					// Generate fields
					//


					// Agent
					if (null != agent) {
						if (string.IsNullOrWhiteSpace(agent.Name)) {
							agentName = "";
						} else {

							string href = $"{Konstants.APP_BASE_URI}section/agents/{agent.Id}".LaTeXEscape();
							string text = agent.Name.LaTeXEscape();

							agentName = $"\\href{{{href}}}{{{text}}}";
						}
					}


					if (null != project) {
						List<string> lines = new List<string>();


						foreach (Address addr in project.Addresses) {

							if (string.IsNullOrWhiteSpace(addr.Value)) {
								continue;
							}

							string escaped = addr.Value.LaTeXEscape();
							string[] split = escaped.Split('\n');
							lines.AddRange(split);
						}

						if (!string.IsNullOrWhiteSpace(project.Name)) {

							string escaped = project.Name.LaTeXEscape();
							string[] split = escaped.Split('\n');
							lines.AddRange(split);
						}

						string href = $"{Konstants.APP_BASE_URI}section/projects/{project.Id}".LaTeXEscape();
						string text = string.Join(' ', lines);

						description = $"\\href{{{href}}}{{{text}}}";

					} else if (null != exception) { // exception
						List<string> lines = new List<string>();

						string name = "Unnamed";
						if (!string.IsNullOrWhiteSpace(exception.Name)) {
							name = exception.Name.LaTeXEscape();
						}

						string escaped = $"Exception: {name}";
						string[] split = escaped.Split('\n');
						lines.AddRange(split);

						description = string.Join(' ', lines);

					} else if (null != holiday) { // holiday
						List<string> lines = new List<string>();

						string name = "Unnamed";
						if (!string.IsNullOrWhiteSpace(holiday.Name)) {
							name = holiday.Name.LaTeXEscape();
						}

						string escaped = $"Holiday: {name}";
						string[] split = escaped.Split('\n');
						lines.AddRange(split);

						description = string.Join(' ', lines);

					} else if (null != nonBillable) {
						List<string> lines = new List<string>();

						string name = "Unnamed";
						if (!string.IsNullOrWhiteSpace(nonBillable.Name)) {
							name = nonBillable.Name.LaTeXEscape();
						}

						string escaped = $"Non Billable: {name}";
						string[] split = escaped.Split('\n');
						lines.AddRange(split);

						description = string.Join(' ', lines);

					}

					if (entry.TimeMode == Labour.TimeModeE.DateAndHours) {

						decimal hComponent = Math.Floor(entry.Hours == null ? 0 : entry.Hours.Value);
						decimal mComponent = Math.Round(60 * ((entry.Hours == null ? 0 : entry.Hours.Value) % 1));

						hours = $"{hComponent}h {mComponent.ToString().PadLeft(2, '0')}m".LaTeXEscape();

						if (null != entry.Hours) {
							lock (_lockObject) {
								hoursTotal += entry.Hours.Value;
							}
							
						}


					} else if (
						entry.TimeMode == Labour.TimeModeE.StartStopTimestamp &&
						!string.IsNullOrWhiteSpace(entry.StartISO8601) &&
						!string.IsNullOrWhiteSpace(entry.EndISO8601)) {

						DateTime startDB = DateTime.Parse(entry.StartISO8601);
						DateTime startLocal = startDB.ToLocalTime();
						DateTime endDB = DateTime.Parse(entry.EndISO8601);
						DateTime endLocal = endDB.ToLocalTime();

						TimeSpan diff = endLocal.Subtract(startLocal);

						decimal hComponent = diff.Hours;
						decimal mComponent = diff.Minutes;

						hours = $"{hComponent}h {mComponent.ToString().PadLeft(2, '0')}m".LaTeXEscape();

						lock (_lockObject) {
							hoursTotal += (decimal)diff.TotalHours;
						}
						


					}

					// is billable

					if (null != type && type.IsBillable) {
						billable = @"\checkmark";
					}

					// travel

					if (entry.LocationType == Labour.LocationTypeE.Travel) {
						travel = @"\checkmark";
					}

					// on-site

					if (entry.LocationType == Labour.LocationTypeE.OnSite) {
						onSite = @"\checkmark";
					}

					// remote

					if (entry.LocationType == Labour.LocationTypeE.Remote) {
						remote = @"\checkmark";
					}

					// extra

					if (entry.GetIsExtra(dpDB, projectsCache)) {
						extra = @"\checkmark";
					}

					// billed

					if (entry.IsBilled) {
						billed = @"\checkmark";
					}

					// paidOut

					if (entry.IsPaidOut) {
						paidOut = @"\checkmark";
					}



				}

				// If we don't have something in these spots we'll get a rendering error.
				if (string.IsNullOrWhiteSpace(date)) {
					date = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(agentName)) {
					agentName = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(description)) {
					description = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(hours)) {
					hours = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(billable)) {
					billable = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(travel)) {
					travel = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(onSite)) {
					onSite = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(remote)) {
					remote = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(extra)) {
					extra = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(billed)) {
					billed = "~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(paidOut)) {
					paidOut = "~"; // nbsp
				}

				return $" {date} & {agentName} & {description} & {hours}& {billable}& {travel}& {onSite}& {remote}& {extra}& {billed}& {paidOut}\\\\\n \\hline\n";
			}


			List<Task<string>> tasks = new List<Task<string>>();

			foreach (Labour l in labourArray) {
				tasks.Add(Task<string>.Run(() => {
					return ProcessEntry(l);
				}));
			}

			await Task.WhenAll(tasks);


			foreach (Task<string> task in tasks) {
				tex.Append(task.Result);
			}

			tex.Append("\\end{longtable}\n");
			tex.Append("\\end{center}\n");
			tex.Append("\\end{landscape}\n");
			tex.Append('\n');
			tex.Append("\\subsubsection*{Labour Summary}\n");


			decimal diffHComponent = Math.Floor(hoursTotal);
			decimal diffMComponent = Math.Round(60 * (hoursTotal % 1));

			string diffHours = $"{diffHComponent}h {diffMComponent.ToString().PadLeft(2, '0')}m".LaTeXEscape();

			tex.Append($"\\textbf{{Total Hours:}} {diffHours}\n");
	
			if (includePostamble) {
				tex.Append("\\end{document}\n");
			}


			return tex.ToString();
		}
	}
}
