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
using SharedCode.DatabaseSchemas;
using Databases.Records;
using Amazon.Polly;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected char PlayAssignmentProjectName(AGIRequest request, AGIChannel channel,
			RequestData data, Assignments assignment) {

			char key = '\0';

			if (null == data.Subscription || string.IsNullOrWhiteSpace(data.Subscription.ProvisionedDatabaseName)) {
				PlayTTS("There was an error while reading the database, please try again later. Code 1sd", "", Engine.Neural, VoiceId.Brian);
				return key;
			}

			if (null == data.DPDB) {
				data.ConnectToDPDBName(data.Subscription.ProvisionedDatabaseName);
			}
			if (null == data.DPDB) {
				PlayTTS("There was an error while reading the database, please try again later. Code 233", "", Engine.Neural, VoiceId.Brian);
				return key;
			}


			Guid? projectId = assignment.ProjectId;
			if (null == projectId) {
				PlayTTS("There is no project for this assignment.", "", Engine.Neural, VoiceId.Brian);
				return key;
			}
			var projectRes = Projects.ForId(data.DPDB, projectId.Value);
			if (0 == projectRes.Count) {
				PlayTTS("We cannot find the project listed on this assignment.", "", Engine.Neural, VoiceId.Brian);
				return key;
			}

			Projects project = projectRes.FirstOrDefault().Value;
			string? projectName = project.Name;
			if (string.IsNullOrWhiteSpace(projectName)) {
				key = PlayTTS("This project does not have a name.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			} else {
				key = PlayTTS($"The project name is {projectName}", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			}

			return key;
		}
	}
}
