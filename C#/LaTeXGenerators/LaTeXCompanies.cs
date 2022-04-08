using Databases.Records.CRM;
using Npgsql;
using SharedCode;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTeXGenerators
{
	public static class LaTeXCompanies
	{
		public static readonly CultureInfo Culture = new CultureInfo("en-CA");

		public static string Generate(
			NpgsqlConnection billingDB,
			NpgsqlConnection dpDB,
			bool includePreamble,
			bool includePostamble,
			IEnumerable<Companies> companies
			) {
			StringBuilder tex = new StringBuilder();

			if (includePreamble) {
				tex.Append(@"
\documentclass{article}
\usepackage{wasysym}
\usepackage{multirow}
\usepackage[utf8]{inputenc}
\usepackage{blindtext}
\usepackage[hidelinks]{hyperref}
\usepackage{marvosym}
\begin{document}");
				tex.Append('\n');
			}
	
			
	
			foreach (Companies company in companies) {
				
				string name = string.IsNullOrWhiteSpace(company.Name) ? "" : company.Name.LaTeXEscape();
				string logo;
				string website = string.IsNullOrWhiteSpace(company.WebsiteURI) ? "" : company.WebsiteURI.LaTeXEscape();
				
				if (null == company.LogoURI) {
					logo = "~";
				} else if (company.LogoURI.Trim().StartsWith("data:")) {
					logo = "[image]".LaTeXEscape();
				} else {
					logo = company.LogoURI.Trim().LaTeXEscape();
				}

				tex.Append($"\\subsubsection*{{{name}}}\n");

				
				
				if (!string.IsNullOrWhiteSpace(company.LogoURI)) {
					tex.Append($"Logo: {logo} \\\\ \n");
				}
				if (!string.IsNullOrWhiteSpace(company.WebsiteURI)) {
					tex.Append($"\\Mundus~\\href{{{website}}}{{{website}}} \\\\ \n");
				}

				tex.Append('\n');
			}
	
			
			if (includePostamble) {
				tex.Append($"\\end{{document}}\n");
			}

			return tex.ToString();
		}



	}
}
