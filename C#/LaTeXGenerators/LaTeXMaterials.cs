using Databases.Records;
using Databases.Records.CRM;
using Npgsql;
using SharedCode;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTeXGenerators
{
	public static class LaTeXMaterials
	{
		public static async Task<string> Generate(
			NpgsqlConnection billingDB,
			NpgsqlConnection dpDB,
			bool includePreamble,
			bool includePostamble,
			IEnumerable<Materials> materials
			) {
			StringBuilder tex = new StringBuilder();



			if (includePreamble) {
				tex.Append(@"\documentclass[12pt, letterpaper, twoside]{article}
				\usepackage{longtable}
				\usepackage[margin=0.5in]{geometry}
				\usepackage{booktabs}
				\usepackage{rotating}
				\usepackage{dashrule}
				\usepackage{amssymb}
				\usepackage{pdfrender}
				\usepackage[skip=0pt]{caption}
				\usepackage[table]{xcolor}
				\usepackage{arydshln}
				\usepackage[utf8]{inputenc}
				\usepackage{pdflscape}
	
				\newcommand*{\boldcheckmark}{%
					\textpdfrender{
						TextRenderingMode=FillStroke,
						LineWidth=.5pt, % half of the line width is outside the normal glyph
					}{\checkmark}%
				}
	
				\begin{document}");
				tex.Append('\n');
			}

			tex.Append("\\begin{landscape}\n");
			tex.Append(@"\newcolumntype{P}[2]{%
			>{\begin{turn}{#1}\begin{minipage}{#2}\small\raggedright\hspace{0pt}}l%
					<{\end{minipage}\end{turn}}%
		}
		
		\begin{center}
			\begin{longtable}{|p{2cm}:p{4cm}:p{2cm}:p{8cm}:p{4cm}:p{0.5cm}:p{0.5cm}|}
				\caption*{Materials} \\
				
				\hline
				\multicolumn{1}{|c|}{\textbf{Date}} &
				\multicolumn{1}{l|}{\textbf{Project}} &
				\multicolumn{1}{c|}{\textbf{Quantity}} &
				\multicolumn{1}{c|}{\textbf{Product}} &
				\multicolumn{1}{c|}{\textbf{Location}} &
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Extra} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Billed} \\ \smallskip}}\\
				
				\hline 
				\endfirsthead
				
				\multicolumn{7}{c}{{Materials -- continued from previous page}} \\
				\hline
				\multicolumn{1}{|c|}{\textbf{Date}} &
				\multicolumn{1}{l|}{\textbf{Project}} &
				\multicolumn{1}{c|}{\textbf{Quantity}} &
				\multicolumn{1}{c|}{\textbf{Product}} &
				\multicolumn{1}{c|}{\textbf{Location}} &
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Extra} \\ \smallskip}}&
				\multicolumn{1}{P{90}{2cm}@{}|}{\shortstack{\textbf{Billed} \\ \smallskip}}\\
				\hline 
				\endhead
				
				\hline
				\multicolumn{7}{r}{{Continued on next page}} \\
				%\hline
				\endfoot
				
				\hline
				\endlastfoot");
			tex.Append('\n');


			var materialsSorted = from l in materials
							   orderby l.DateUsedISO8601 descending
							   select l;



			Materials[] materialsArray = materialsSorted.ToArray();

			// Fetch objects
			HashSet<Guid> projectIdToFetch = new HashSet<Guid>(materialsArray.Length);
			HashSet<Guid> productIdToFetch = new HashSet<Guid>();

			foreach (Materials entry in materialsArray) {

				if (null != entry.ProjectId) {
					projectIdToFetch.Add(entry.ProjectId.Value);
				}

				if (null != entry.ProductId) {
					productIdToFetch.Add(entry.ProductId.Value);
				}
			}

			Dictionary<Guid, Projects> projectsCache = Projects.ForIds(dpDB, projectIdToFetch);
			Dictionary<Guid, Products> productsCache = Products.ForIds(dpDB, productIdToFetch);


			string ProcessEntry(Materials entry) {


				Projects? projectObj = null;
				Products? productObj = null;



				string dateStr = "";
				string descriptionStr = "";
				string quantityStr = "";
				string productStr = "";
				string extraStr = "";
				string billedStr = "";
				string locationStr = "";









				if (null != entry) {

					if (null != entry.ProjectId) {
						projectObj = projectsCache.GetValueOrDefault(entry.ProjectId.Value);
					}
					if (null != entry.ProductId) {
						productObj = productsCache.GetValueOrDefault(entry.ProductId.Value);
					}



					// date

					if (!string.IsNullOrWhiteSpace(entry.DateUsedISO8601)) {

						// https://www.c-sharpcorner.com/blogs/date-and-time-format-in-c-sharp-programming1

						DateTime startDate = DateTime.Parse(entry.DateUsedISO8601);
						DateTime localDate = startDate.ToLocalTime();
						string formatted = localDate.ToString("yyyy-MM-dd");
						dateStr = formatted.LaTeXEscape();
					}

					// description

					if (null != projectObj) {
						List<string> lines = new List<string>();

						foreach (Address addr in projectObj.Addresses) {

							if (string.IsNullOrWhiteSpace(addr.Value)) {
								continue;
							}

							string escaped = addr.Value.Trim().LaTeXEscape();
							string[] split = escaped.Split('\n');

							lines.Add(split.First());
						}

						if (!string.IsNullOrWhiteSpace(projectObj.Name)) {
							string escaped = projectObj.Name.Trim().LaTeXEscape();

							string[] split = escaped.Split('\n');
							lines.AddRange(split);
						}

						descriptionStr = string.Join(' ', lines);
						descriptionStr = descriptionStr.Truncate(40, true, true);
						
					}

					// quantity

					if (null != entry.Quantity) {

						string qty = $"{entry.Quantity}".LaTeXEscape();
						string unit = "x";

						if (!string.IsNullOrWhiteSpace(entry.QuantityUnit)) {
							unit = $"{entry.QuantityUnit}";
						}

						quantityStr = $"{qty} {unit}".LaTeXEscape();
					}

					// product

					if (null != productObj) {

						string name = "Unnamed";
						if (!string.IsNullOrWhiteSpace(productObj.Name)) {
							name = productObj.Name.LaTeXEscape();
						}

						productStr = name;

						productStr = productStr.Truncate(30, true, true);
					}

					// location

					if (!string.IsNullOrWhiteSpace(entry.Location)) {

						locationStr = entry.Location.LaTeXEscape();
						locationStr = locationStr.Truncate(20, true, true);
					}

					// extra

					if (null != entry.IsExtra && entry.IsExtra.Value) {
						extraStr = @"\checkmark";
					}


					// billed

					if (null != entry.IsBilled && entry.IsBilled.Value) {
						billedStr = @"\checkmark";
					}
				}

				if (string.IsNullOrWhiteSpace(dateStr)) {
					dateStr = @"~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(descriptionStr)) {
					descriptionStr = @"~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(quantityStr)) {
					quantityStr = @"~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(productStr)) {
					productStr = @"~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(locationStr)) {
					locationStr = @"~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(extraStr)) {
					extraStr = @"~"; // nbsp
				}
				if (string.IsNullOrWhiteSpace(billedStr)) {
					billedStr = @"~"; // nbsp
				}

				return $" {dateStr} & {descriptionStr} & {quantityStr} & {productStr} & {locationStr} & {extraStr}  &{billedStr} \\\\ \\hline\n";
			}

			List<Task<string>> tasks = new List<Task<string>>();

			foreach (Materials m in materialsArray) {
				tasks.Add(Task<string>.Run(() => {
					return ProcessEntry(m);
				}));
			}

			await Task.WhenAll(tasks);

			foreach (Task<string> task in tasks) {
				tex.Append(task.Result);
			}

			tex.Append(@"\end{longtable}
		\end{center}");
			tex.Append('\n');


			tex.Append("\\end{landscape}\n");
	
			if (includePostamble) {
				tex.Append("\\end{document}\n");
			}
			return tex.ToString();
		}
	}
}
