using System;
using AsterNET.FastAGI;
using System.Linq;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using System.Threading.Tasks;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected async Task<char> PlayAssignmentProjectName(AGIRequest request, AGIChannel channel,
			RequestData data, Assignments assignment) {

			char key = '\0';

			if (null == data.Subscription || string.IsNullOrWhiteSpace(data.Subscription.ProvisionedDatabaseName)) {
				await PlayTTS("There was an error while reading the database, please try again later. Code 1sd", "", Engine.Neural, VoiceId.Brian);
				return key;
			}

			if (null == data.DPDB) {
				data.ConnectToDPDBName(data.Subscription.ProvisionedDatabaseName);
			}
			if (null == data.DPDB) {
				await PlayTTS("There was an error while reading the database, please try again later. Code 233", "", Engine.Neural, VoiceId.Brian);
				return key;
			}


			Guid? projectId = assignment.ProjectId;
			if (null == projectId) {
				await PlayTTS("There is no project for this assignment.", "", Engine.Neural, VoiceId.Brian);
				return key;
			}
			var projectRes = Projects.ForId(data.DPDB, projectId.Value);
			if (0 == projectRes.Count) {
				await PlayTTS("We cannot find the project listed on this assignment.", "", Engine.Neural, VoiceId.Brian);
				return key;
			}

			Projects project = projectRes.FirstOrDefault().Value;
			string? projectName = project.Name;
			if (string.IsNullOrWhiteSpace(projectName)) {
				key = await PlayTTS("This project does not have a name.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			} else {
				key = await PlayTTS($"The project name is {projectName}", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			}

			return key;
		}
	}
}
