using SharedCode.DatabaseSchemas;
using Npgsql;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Globalization;

namespace LaTeXGenerators
{
	public static class LaTeXProjects
	{
		public static readonly CultureInfo Culture = new CultureInfo("en-CA");

		public static async Task<string> Generate(
			NpgsqlConnection billingDB,
			NpgsqlConnection dpDB,
			bool includePreamble,
			bool includePostamble,
			IEnumerable<Projects> projects,
			bool includeCompanies,
			bool includeContacts,
			bool includeSchedule,
			bool includeNotes,
			bool includeLabour,
			bool includeMaterials
			) {
			StringBuilder tex = new StringBuilder();

			if (includePreamble) {
				tex.Append(@"\documentclass[12pt, letterpaper, twoside]{article}
\usepackage{longtable}
\usepackage[margin=0.5in]{geometry}
\usepackage{booktabs}
\usepackage{rotating}
\usepackage{dashrule}
\usepackage{wasysym}
\usepackage{multirow}
\usepackage[utf8]{inputenc}
\usepackage{amssymb}
\usepackage{blindtext}
\usepackage{parskip}
\usepackage[skip=0pt]{caption}
\usepackage{pdfrender}
\usepackage{longtable}
\usepackage[table]{xcolor}
\usepackage{tocloft}
\usepackage{arydshln}
\usepackage{pdflscape}
\usepackage[hidelinks]{hyperref}
\renewcommand{\cftsecleader}{\cftdotfill{\cftdotsep}}
\newcommand\mydotfill{\cftdotfill{\cftdotsep}}
\newcommand{\subhead}[1]{{\Large #1 \vspace{1ex}}}

\newcommand*{\boldcheckmark}{%
	\textpdfrender{
		TextRenderingMode=FillStroke,
		LineWidth=.5pt, % half of the line width is outside the normal glyph
	}{\checkmark}%
}

\begin{document}");
			}

			var projectsSorted = from p in projects
							   orderby p.LastModifiedIso8601 descending
							   select p;

			Projects[] projectsArray = projectsSorted.ToArray();


			// Fetch objects
			HashSet<Guid> projectIdToFetch = new HashSet<Guid>(projectsArray.Length);
			HashSet<Guid> statusIdToFetch = new HashSet<Guid>();
			HashSet<Guid> contactsIdToFetch = new HashSet<Guid>();
			HashSet<Guid> companiesIdToFetch = new HashSet<Guid>();

			foreach (Projects entry in projectsArray) {

				if (null != entry.Id) {
					projectIdToFetch.Add(entry.Id.Value);
				}
				if (null != entry.ParentId) {
					projectIdToFetch.Add(entry.ParentId.Value);
				}
				if (null != entry.StatusId) {
					statusIdToFetch.Add(entry.StatusId.Value);
				}
				if (null != entry.Contacts) {
					foreach (LabeledContactId obj in entry.Contacts) {
						if (obj.Value != null) {
							contactsIdToFetch.Add(obj.Value.Value);
						}
					}
				}
				if (null != entry.Companies) {
					foreach (LabeledCompanyId obj in entry.Companies) {
						if (obj.Value != null) {
							companiesIdToFetch.Add(obj.Value.Value);
						}
					}
				}
			}

			Dictionary<Guid, Projects> projectsCache = Projects.ForIds(dpDB, projectIdToFetch);
			Dictionary<Guid, ProjectStatus> statusCache = ProjectStatus.ForIds(dpDB, statusIdToFetch);
			Dictionary<Guid, Contacts> contactsCache = Contacts.ForIds(dpDB, contactsIdToFetch);
			Dictionary<Guid, Companies> companiesCache = Companies.ForIds(dpDB, companiesIdToFetch);


			string ProcessEntry(Projects project) {

				StringBuilder sb = new StringBuilder();

				if (null == project.Id) {
					return sb.ToString();
				}

				string name = "";
				string parentName = "";
				string status = "";
				string lastActivity = "";

				var projectsGenerational = Projects.RecursiveChildProjectsOfId(dpDB, project.Id.Value);

				if (null != project) {

					// Project Name
					if (false == string.IsNullOrWhiteSpace(project.Name)) {
						name = project.Name.LaTeXEscape();
					}

					// parent project
					if (null != project.ParentId) {

						var parentRes = Projects.ForId(dpDB, project.ParentId.Value);
						if (parentRes.Count > 0) {

							Projects parent = parentRes.FirstOrDefault().Value;

							if (string.IsNullOrWhiteSpace(parent.Name)) {
								parentName = "No Name".LaTeXEscape();
							} else {
								parentName = parent.Name.LaTeXEscape();
							}
						}
					}

					// status
					if (null != project.StatusId) {

						var res = ProjectStatus.ForId(dpDB, project.StatusId.Value);
						if (res.Count > 0) {

							ProjectStatus s = res.FirstOrDefault().Value;

							if (string.IsNullOrWhiteSpace(s.Name)) {
								status = "Unnamed Status";
							} else {
								status = s.Name.LaTeXEscape();
							}
						}
					}


					// last activity
					if (null != project.LastModifiedBillingId ||
						false == string.IsNullOrWhiteSpace(project.LastModifiedIso8601)) {

						// date
						if (false == string.IsNullOrWhiteSpace(project.LastModifiedIso8601)) {
							DateTime date = DateTime.Parse(project.LastModifiedIso8601);
							DateTime dateLocal = date.ToLocalTime();
							string dateStr = dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture);

							lastActivity = $" {dateStr}".LaTeXEscape();
						}


						// who
						if (null != project.LastModifiedBillingId) {

							var resBillingContact = BillingContacts.ForId(billingDB, project.LastModifiedBillingId.Value);
							if (resBillingContact.Count > 0) {

								BillingContacts bc = resBillingContact.FirstOrDefault().Value;

								if (false == string.IsNullOrWhiteSpace(bc.FullName)) {
									lastActivity += $" by {bc.FullName}. ".LaTeXEscape();
								} else {
									lastActivity += " by Unknown. ".LaTeXEscape();
								}
							}
						}

					}





				}


				// Make sure empty variables at least have a default value.
				// ~ is nbsp
				if (string.IsNullOrWhiteSpace(name)) {
					name = "No Name";
				}
				if (string.IsNullOrWhiteSpace(parentName)) {
					parentName = "None";
				}
				if (string.IsNullOrWhiteSpace(status)) {
					status = "Unknown";
				}
				if (string.IsNullOrWhiteSpace(lastActivity)) {
					lastActivity = "Unknown";
				}

				sb.Append($"\\section*{{{name}}}\n");
				sb.Append($"Name\\enspace\\mydotfill\\enspace {name} \\\\ \n");
				sb.Append($"Parent Project\\enspace\\mydotfill\\enspace {parentName} \\\\ \n");
				sb.Append($"Status\\enspace\\mydotfill\\enspace {status} \\\\ \n");
				sb.Append($"Last Activity\\enspace\\mydotfill\\enspace {lastActivity} \n\n");
				sb.Append($"\\subsection*{{Address}}\n");

				if (project == null || project.Addresses.Count == 0) {

					sb.Append("No Addresses. \n");

				} else {
					foreach (Address addr in project.Addresses) {
						if (string.IsNullOrWhiteSpace(addr.Value)) {
							continue;
						}

						string label = addr.Label.LaTeXEscape();
						if (string.IsNullOrWhiteSpace(label)) {
							label = "~";
						}

						string[] valueRows = addr.Value.Split('\n');
						List<string> valueRowsMod = new List<string>();

						foreach (string row in valueRows) {
							valueRowsMod.Add(row.LaTeXEscape());
						}

						string valueRowsCombined = string.Join("\\\\\n", valueRowsMod);
						if (string.IsNullOrWhiteSpace(valueRowsCombined)) {
							valueRowsCombined = "~";
						}

						sb.Append($"{label}\\enspace\\mydotfill\\enspace\\shortstack[l]{{{valueRowsCombined}}} \\\\\n");
					}
				} // project.Addresses.Count == 0

				if (includeCompanies) {
					sb.Append($"\\subsection*{{Companies}}\n");
					if (project == null || project.Addresses.Count == 0) {

						sb.Append($"No Companies. \n");

					} else {
						foreach (LabeledCompanyId row in project.Companies) {

							string label = row.Label.LaTeXEscape();
							Guid? companyId = row.Value;

							if (null != companyId) {

								var res = Companies.ForId(dpDB, companyId.Value);

								if (res.Count > 0) {

									Companies company = res.FirstOrDefault().Value;

									string n = string.IsNullOrWhiteSpace(company.Name) ? "Unnamed Company" : company.Name.LaTeXEscape();

									sb.Append($"{label}\\enspace\\mydotfill\\enspace\\shortstack[l]{{{n}}} \\\\\n");


								}
							}
						}
						//sb.Append($"\\\\\n");
					}
				} // includeCompanies

				if (includeContacts) {
					sb.Append($"\\subsection*{{Contacts}}\n");

					if (project == null || project.Contacts.Count == 0) {
						sb.Append($"No Contacts. \n");
					} else {
						foreach (LabeledContactId row in project.Contacts) {
							string label = row.Label.LaTeXEscape();
							Guid? contactId = row.Value;

							if (null != contactId) {

								var res = Contacts.ForId(dpDB, contactId.Value);
								if (res.Count > 0) {
									Contacts contact = res.FirstOrDefault().Value;

									string n = string.IsNullOrWhiteSpace(contact.Name) ? "Unnamed Contact" : contact.Name.LaTeXEscape();

									sb.Append($"{label}\\enspace\\mydotfill\\enspace\\shortstack[l]{{{n}}} \\\\\n");

								}

							}



						}
						//sb.Append($"\\\\\n");
					}
				} // includeContacts


				if (includeSchedule) {
					sb.Append($"\\subsection*{{Schedule}}\n");

					if (project != null && project.HasStartISO8601 && !string.IsNullOrWhiteSpace(project.StartISO8601)) {

						DateTime dateISO = DateTime.Parse(project.StartISO8601);
						DateTime dateLocal = dateISO.ToLocalTime();

						switch (project.StartTimeMode) {
							default:
							case Projects.StartTimeModeE.None:
								sb.Append($"Start\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}\\\n");
								break;
							case Projects.StartTimeModeE.MorningFirstThing:
								sb.Append($"Start\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}, first thing in the morning\\\n");
								break;
							case Projects.StartTimeModeE.MorningSecondThing:
								sb.Append($"Start\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}, second thing in the morning\\\n");
								break;
							case Projects.StartTimeModeE.AfternoonFirstThing:
								sb.Append($"Start\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}, first thing in the afternoon\\\n");
								break;
							case Projects.StartTimeModeE.AfternoonSecondThing:
								sb.Append($"Start\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}, second thing in the afternoon\\\n");
								break;
							case Projects.StartTimeModeE.Time:
								sb.Append($"Start\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture).LaTeXEscape()}\\\n");
								break;
						}

					} else {
						sb.Append("No start time specified.\n");
					}

					sb.Append("\\\\\n");

					if (project != null && project.HasEndISO8601 && !string.IsNullOrWhiteSpace(project.EndISO8601)) {

						DateTime dateISO = DateTime.Parse(project.EndISO8601);
						DateTime dateLocal = dateISO.ToLocalTime();

						switch (project.EndTimeMode) {
							default:
							case Projects.EndTimeModeE.None:
								sb.Append($"End\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy", Culture).LaTeXEscape()}\\\n");
								break;
							case Projects.EndTimeModeE.Time:
								sb.Append($"End\\enspace\\mydotfill\\enspace {dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture).LaTeXEscape()}\\\n");
								break;
						}

					} else {
						sb.Append("No end time specified.\n");
					}

					//sb.Append("\\\\\n\\\\\n");
				} // includeSchedule

				if (includeNotes) {
					sb.Append("\\subsection*{{Notes}}\n");

					if (null == project) {
						sb.Append("No notes.\n");
					} else {

						var res = ProjectNotes.ForProjectId(dpDB, project.Id.Value);

						var sortedNotes = from kvp in res
										  orderby kvp.Value.LastModifiedIso8601 descending
										  select kvp.Value;

						if (!sortedNotes.Any()) {
							sb.Append("No notes.\n");
						}

						foreach (ProjectNotes note in sortedNotes) {

							string dateStr = "Unknown";
							if (false == string.IsNullOrWhiteSpace(note.LastModifiedIso8601)) {
								DateTime dateISO = DateTime.Parse(note.LastModifiedIso8601);
								DateTime dateLocal = dateISO.ToLocalTime();
								dateStr = dateLocal.ToString("MMM d, yyyy, h:mm tt", Culture);
							}

							string who = "Unknown Author";

							if (null != note.LastModifiedBillingId) {

								var resBilling = BillingContacts.ForId(billingDB, note.LastModifiedBillingId.Value);
								if (resBilling.Count > 0) {

									BillingContacts billingContacts = resBilling.FirstOrDefault().Value;

									if (false == string.IsNullOrWhiteSpace(billingContacts.FullName)) {
										who = billingContacts.FullName.LaTeXEscape();
									}

								}

							}


							sb.Append($"\\textbf{{{who} at {dateStr}:}}\n\n");


							switch (note.ContentType) {
								case ProjectNotes.ContentTypes.StyledText: {

									string html = note.StyledTextHTML ?? "";

									HtmlDocument doc = new HtmlDocument();
									doc.LoadHtml(html);

									string stripped = doc.DocumentNode.InnerText;
									string styledText = stripped.LaTeXEscape();

									sb.Append("\\begin{flushleft}\n");
									sb.Append($"\\normalsize {styledText}\n");
									sb.Append("~\\\\\n");
									sb.Append("\\end{flushleft}\n");

									break;
								}
								case ProjectNotes.ContentTypes.Checkbox: {

									string text = note.CheckboxText ?? "";

									sb.Append("\\normalsize ");

									if (note.CheckboxState == true) {
										sb.Append("\\boldcheckmark ");
									} else {
										sb.Append("\\times ");
									}

									sb.Append($" {text.LaTeXEscape()} \\\\\n");

									break;
								}
								case ProjectNotes.ContentTypes.Image: {

									string escaped = $"{Konstants.APP_BASE_URI}section/projects/${project.Id}?tab=Notes".LaTeXEscape();

									sb.Append($"Image, view at: \\ {escaped} \\\\\n");

									break;
								}
								case ProjectNotes.ContentTypes.Video: {

									if (null != note.VideoURI) {
										string escaped = note.VideoURI.LaTeXEscape();

										sb.Append($"Video, view at: \\ {escaped} \\\\\n");
									}
									

									break;
								}
							}


						}


					}
					
					
					

				}

				if (includeLabour) {
					
					var res = Labour.ForProjects(dpDB, projectsGenerational.Values);
					var sortedLabour = from kvp in res
									   orderby kvp.Value.StartISO8601
									   select kvp.Value;


					if (!sortedLabour.Any()) {
						sb.Append($"\\subsection*{{Labour}}\n");
						sb.Append("No labour entries.\n\n");
					} else {
						sb.Append(LaTeXLabour.Generate(
							billingDB,
							dpDB,
							includePreamble: false,
							includePostamble: false,
							labour: sortedLabour
						).Result);

					}
					//sb.Append($"\\\\\n");
				}

				if (includeMaterials) {
					

					var res = Materials.ForProjects(dpDB, projectsGenerational.Values);
					var sortedMaterials = from kvp in res
										  orderby kvp.Value.DateUsedISO8601
										  select kvp.Value;



					if (!sortedMaterials.Any()) {
						sb.Append($"\\subsection*{{Materials}}\n");
						sb.Append("No material entries.\n\n");
					} else {
						sb.Append(LaTeXMaterials.Generate(
							billingDB,
							dpDB,
							includePreamble: false,
							includePostamble: false,
							materials: sortedMaterials
						).Result);
					}
				}

				//sb.Append($"\\\\\n");






				return sb.ToString();
			}



			List<Task<string>> tasks = new List<Task<string>>();

			foreach (Projects p in projectsArray) {
				tasks.Add(Task<string>.Run(() => {
					return ProcessEntry(p);
				}));
			}

			await Task.WhenAll(tasks);


			foreach (Task<string> task in tasks) {
				tex.Append(task.Result);
			}









			// postamble
			if (includePostamble) {
				tex.Append("\\end{document}");
			}


			return tex.ToString();
		}

	}
}
