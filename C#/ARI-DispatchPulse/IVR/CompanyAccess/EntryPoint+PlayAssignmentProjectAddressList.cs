using System;
using System.Collections.Generic;
using System.Text;
using AsterNET.FastAGI;
using Npgsql;
using System.Linq;
using Newtonsoft.Json.Linq;
using SharedCode;
using System.IO;
using Afk.ZoneInfo;
using System.Text.RegularExpressions;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void PlayAssignmentProjectAddressList(AGIRequest request, AGIChannel channel,
			RequestData data, Assignments assignment) {

			if (null == data.Subscription || string.IsNullOrWhiteSpace(data.Subscription.ProvisionedDatabaseName)) {
				PlayTTS("There was an error while reading the database, please try again later. Code 912", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			if (null == data.DPDB) {
				data.ConnectToDPDBName(data.Subscription.ProvisionedDatabaseName);
			}
			if (null == data.DPDB) {
				PlayTTS("There was an error while reading the database, please try again later. Code 233", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}


			Guid? projectId = assignment.ProjectId;
			if (null == projectId) {
				PlayTTS("There is no project for this assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}


			var resProjects = Projects.ForId(data.DPDB, projectId.Value);
			if (0 == resProjects.Count) {
				PlayTTS("We cannot find the project listed on this assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			Projects project = resProjects.FirstOrDefault().Value;

			List<Address> allAddresses = project.Addresses;


			bool first = true;
			foreach (Address entry in allAddresses) {
				if (!first) {
					PlayTTS("Next Address. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				}

				string label = entry.Label;
				string value = entry.Value;

				using StringReader sr = new StringReader(value);

				StringBuilder firstLine = new StringBuilder();
				firstLine.Append(sr.ReadLine());

				int i = firstLine.Length;
				while (--i > -1) {

					char current = firstLine.ToString()[i];
					if (Char.IsNumber(current) || current == '-') {
						firstLine.Insert(i + 1, ' ');
					}

				}
				firstLine.Replace("-", "dash");


				StringBuilder sb = new StringBuilder();

				if (!string.IsNullOrWhiteSpace(label)) {
					sb.Append($"This address is labelled {label}, it is: ");
				}
				sb.Append(firstLine);

				string txt = sb.ToString();

				PlayTTS(txt, escapeAllKeys, Engine.Neural, VoiceId.Brian);

				first = false;

			}


		}
	}
}
