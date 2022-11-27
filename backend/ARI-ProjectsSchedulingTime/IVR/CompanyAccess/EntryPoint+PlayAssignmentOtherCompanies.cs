using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void PlayAssignmentOtherCompanies(
			AGIRequest request, 
			AGIChannel channel,
			RequestData data, 
			Assignments assignment
			) {
			
			if (null == assignment.ProjectId) {
				PlayTTS($"We could not find the project for this assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			if (null == data.DPDB) {
				PlayTTS($"Could not connect to database.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			var resProjects = Projects.ForId(data.DPDB, assignment.ProjectId.Value);
			if (0 == resProjects.Count) {
				PlayTTS($"Did not find a project on this assignment.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			Projects project = resProjects.FirstOrDefault().Value;

			List<LabeledCompanyId> companies = project.Companies;
			if (companies.Count == 0) {
				PlayTTS($"There are no other companies on record as working on this job.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}



			foreach (LabeledCompanyId entry in companies) {

				Guid? companyId = entry.Value;
				if (null == companyId) {
					continue;
				}

				var resCompany = Companies.ForId(data.DPDB, companyId.Value);
				if (0 == resCompany.Count) {
					continue;
				}

				Companies company = resCompany.FirstOrDefault().Value;
				if (null == company) {
					continue;
				}

				string? label = entry.Label;
				if (string.IsNullOrWhiteSpace(label)) {
					PlayTTS($"This company has no label.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				} else {
					PlayTTS($"This company is labeled {label}. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				}

				string? name = company.Name;

				if (string.IsNullOrWhiteSpace(name)) {
					PlayTTS($"There is no name on this company.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				} else {
					PlayTTS($"It is named {name}.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				}



			}










		}
	}
}
