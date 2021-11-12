using Databases.Records;
using Databases.Records.CRM;
using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using System.Linq;
using System.Globalization;
using Databases.Records.Billing;
using SharedCode;
using SharedCode.Extensions;
using NodaTime.Text;
using NodaTime;

namespace LaTeXGenerators
{
	public class LaTeXOnCallResponder30Days
	{
		public static readonly CultureInfo Culture = new CultureInfo("en-CA");

		public static string Generate(
			NpgsqlConnection billingDB,
			NpgsqlConnection dpDB,
			bool includePreamble,
			bool includePostamble
			) {
			StringBuilder tex = new StringBuilder();

			if (includePreamble) {

				tex.Append(@"
\documentclass[12pt, letterpaper, twoside]{article}
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
\usepackage{multirow}
\usepackage{array}
\usepackage{pdflscape}
\newcolumntype{P}[1]{>{\centering\arraybackslash}p{#1}}


\begin{document}
");

			}

			var resVM = Voicemails.FromTheLast30Days(dpDB);
			Voicemails[] resVMValues = resVM.Values.ToArray();

			tex.AppendLine(@"\begin{landscape}");

			tex.Append(@"
\begin{longtable}{|p{4cm}:p{3.6cm}:p{2.3cm}:p{2.3cm}:P{10cm}|}
\caption*{On-Call Responder: Last 30 Days}
\label{tab:table3}\\
\hline
\textbf{Attendant Name} & \textbf{Date and Time} & \textbf{Caller ID} & \textbf{Callback Number} & \textbf{Result}  \\

");
			

			for (int i=0; i< resVMValues.Length; i++) {

				tex.AppendLine(@"\hline");

				string attendantName = "";
				string dateTime = "";
				string callerId = "";
				string callbackNumber = "";
				string result = "";


				Guid? attendantId = resVMValues[i].OnCallAutoAttendantId;

				do {
					if (null == attendantId)
						break;

					var resAtId = OnCallAutoAttendants.ForId(dpDB, attendantId.Value);
					if (!resAtId.Any())
						break;

					OnCallAutoAttendants attendant = resAtId.FirstOrDefault().Value;
					attendantName = attendant.Name ?? "";

				} while (false);

				

				//attendantName = resVMValues[i].OnCallAutoAttendantId

				string? iso8601 = resVMValues[i].MessageLeftAtISO8601;
				if (!string.IsNullOrWhiteSpace(iso8601)) {
					ParseResult<Instant> pr = InstantPattern.ExtendedIso.Parse(iso8601);
					if (pr.Success) {
						Instant instant = pr.Value;
#warning TODO: CORRECT THIS REPORT
						var timeZone = DateTimeZoneProviders.Tzdb[BillingCompanies.kJsonValueIANATimezoneDefault];
						ZonedDateTime dtWpg = instant.InZone(timeZone);
						dateTime = dtWpg.ToString(@"yyyy/MM/dd HH:mm:ss", new CultureInfo("en-CA")).LaTeXEscape();
					}
				}

				string? cidName = resVMValues[i].CallerIdName;
				if (!string.IsNullOrWhiteSpace(cidName)) {
					callerId = $"{resVMValues[i].CallerIdNumber} \"{cidName}\"".LaTeXEscape();
				} else {
					callerId = $"{resVMValues[i].CallerIdNumber}".LaTeXEscape();
				}

				



				callbackNumber = $"{resVMValues[i].CallbackNumber}".LaTeXEscape();
				result = $"{resVMValues[i].MarkedHandledBy}".LaTeXEscape().ReplaceEvenOdd( "\"", "''", "``");

				// insert nbsp if empty
				if (string.IsNullOrWhiteSpace(attendantName)) {                                                   
					attendantName = "~";
				}
				if (string.IsNullOrWhiteSpace(dateTime)) {
					attendantName = "~";
				}
				if (string.IsNullOrWhiteSpace(callerId)) {
					attendantName = "~";
				}
				if (string.IsNullOrWhiteSpace(callbackNumber)) {
					attendantName = "~";
				}
				if (string.IsNullOrWhiteSpace(result)) {
					attendantName = "~";
				}

				tex.AppendLine($" {attendantName} & {dateTime} & {callerId} & {callbackNumber}& {result} \\\\");

				// Last table line at the end.
				if (i == resVMValues.Length-1) {
					tex.AppendLine(@"\hline");
				}
			}
			
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//Some text & some text & some text & 5 & some text\\
//\hline
//\end{longtable}









			tex.AppendLine("\\end{longtable}");







			tex.AppendLine(@"\end{landscape}");




			if (includePostamble) {
				tex.Append("\\end{document}\n");
			}


			return tex.ToString();
		}
		

	}
}
