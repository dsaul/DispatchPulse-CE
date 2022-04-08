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
	public static class LaTeXContacts
	{
		public static string Generate(
			NpgsqlConnection billingDB,
			NpgsqlConnection dpDB,
			bool includePreamble,
			bool includePostamble,
			IEnumerable<Contacts> contacts
			) {
			StringBuilder tex = new StringBuilder();


			if (includePreamble) {
				tex.Append(@"\documentclass{article}
\usepackage{wasysym}
\usepackage{multirow}
\usepackage[utf8]{inputenc}
\usepackage{blindtext}
\usepackage[hidelinks]{hyperref}
\usepackage{tocloft}
\renewcommand{\cftsecleader}{\cftdotfill{\cftdotsep}}
\newcommand\mydotfill{\cftdotfill{\cftdotsep}}

\begin{document}");
				tex.Append('\n');
			}

			var contactsSorted = from contact in contacts
								 orderby contact.Name
								 select contact;


			foreach (Contacts contact in contactsSorted) {

				string name = string.IsNullOrWhiteSpace(contact.Name) ? "" : contact.Name.LaTeXEscape();
				string title = string.IsNullOrWhiteSpace(contact.Title) ? "" : contact.Title.LaTeXEscape();
		
				// Create multi-dimentional array with default values;
				List<List<string>> tableContents = new List<List<string>>();

				List<PhoneNumber> contactPhoneNumbers = contact.PhoneNumbers;
				var contactPhoneNumbersFiltered = (from number in contactPhoneNumbers
												   where !string.IsNullOrWhiteSpace(number.Value)
												   orderby number.Label
												   select number).ToList();



				List<EMail> contactEMails = contact.EMails;
				var contactEMailsFiltered = (from email in contactEMails
											 where !string.IsNullOrWhiteSpace(email.Value)
											 orderby email.Label
											 select email).ToList();

				List<Address> contactAddresses = contact.Addresses;
				var contactAddressesFiltered = (from address in contactAddresses
												where !string.IsNullOrWhiteSpace(address.Value)
												orderby address.Label
												select address).ToList();



				if (string.IsNullOrWhiteSpace(name)) {
					name = "No Name";
				}

				tex.Append('\n');
				tex.Append(@"\begin{minipage}{\linewidth}");
				tex.Append('\n');
				tex.Append($"\\subsection*{{{name}}}\n");
				if (false == string.IsNullOrWhiteSpace(title)) {
					tex.Append($"{title} \n\n");
				}

				if (contactPhoneNumbersFiltered.Count > 0) {
					tex.Append($"\\subsubsection*{{Phone Number(s)}}\n");
					foreach (PhoneNumber number in contactPhoneNumbersFiltered) {
						tex.Append($"{number.Label.LaTeXEscape()} \\enspace\\mydotfill\\enspace \\href{{tel:{number.Value.LaTeXEscape()}}}{{{number.Value.LaTeXEscape()}}}\n\n");
					}
				}
				
				if (contactEMailsFiltered.Count > 0) {
					tex.Append($"\\subsubsection*{{E-Mail(s)}}\n");
					foreach (EMail email in contactEMailsFiltered) {
						tex.Append($"{email.Label.LaTeXEscape()} \\enspace\\mydotfill\\enspace \\href{{mailto:{email.Value.LaTeXEscape()}}}{{{email.Value.LaTeXEscape()}}}\n\n");
					}
				}

				if (contactAddressesFiltered.Count > 0) {
					tex.Append($"\\subsubsection*{{Address(es)}}\n");
					foreach (Address address in contactAddressesFiltered) {

						string[] lines = address.Value.Split('\n');

						StringBuilder sb = new StringBuilder();
						sb.Append("\\vbox {\n");
						foreach (string line in lines) {
							if (string.IsNullOrWhiteSpace(line)) {
								continue;
							}
							sb.Append($"\\hbox{{{line.LaTeXEscape()}}}\n");
						}
						sb.Append("}\n");

						string gmapsaddr = string.Join(' ', lines);

						tex.Append($"{address.Label.LaTeXEscape()} \\enspace\\mydotfill\\enspace \\href{{https://maps.google.com/maps?q={gmapsaddr}}}{{{sb}}}\n\n");
					}
				}

				
				if (false == string.IsNullOrWhiteSpace(contact.Notes)) {
					tex.Append($"\\subsubsection*{{Notes}}\n");


					tex.Append('\n');
					tex.Append(@"\begin{flushleft}");
					tex.Append('\n');
					tex.Append($"\\normalsize	{contact.Notes.LaTeXEscape()} \\vspace{{2ex}}");
					tex.Append('\n');
					tex.Append(@"\end{flushleft}");
					tex.Append('\n');
				}
				

				
				tex.Append(@"\end{minipage}");
				tex.Append('\n');
				tex.Append("\\bigskip\n\n");
				


			}
			
			
			if (includePostamble) {
				tex.Append("\\end{document}\n");
			}


			return tex.ToString();
		}
	}
}
