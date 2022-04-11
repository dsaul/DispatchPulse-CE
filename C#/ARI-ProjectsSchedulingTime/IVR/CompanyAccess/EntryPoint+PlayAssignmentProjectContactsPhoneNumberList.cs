using System;
using System.Collections.Generic;
using System.Text;
using AsterNET.FastAGI;
using Npgsql;
using System.Linq;
using System.Text.RegularExpressions;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void PlayAssignmentProjectContactsPhoneNumberList(AGIRequest request, AGIChannel channel,
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
			List<LabeledContactId> allContacts = project.Contacts;

			bool first = true;
			foreach (LabeledContactId entry in allContacts) {
				Guid? contactId = entry.Value;
				if (null == contactId) {
					continue;
				}

				var resContact = Contacts.ForId(data.DPDB, contactId.Value);
				if (0 == resContact.Count) {
					continue;
				}


				Contacts contact = resContact.FirstOrDefault().Value;


				if (!first) {
					PlayTTS("Next Contact. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				}

				string contactLabel = entry.Label;

				if (!string.IsNullOrWhiteSpace(contactLabel)) {
					PlayTTS($"This contact is labelled {contactLabel}.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				}



				string? contactName = contact.Name;
				if (!string.IsNullOrWhiteSpace(contactName)) {
					PlayTTS(contactName, escapeAllKeys, Engine.Neural, VoiceId.Brian);
				}


				List<PhoneNumber> phoneNumbers = contact.PhoneNumbers;

				bool firstNumber = true;
				foreach (PhoneNumber labelledNumber in phoneNumbers) {
					if (!firstNumber) {
						PlayTTS("Next Number. ", escapeAllKeys, Engine.Neural, VoiceId.Brian);
					}

					string numberLabel = labelledNumber.Label;
					string numberValue = labelledNumber.Value;

					Regex digitsOnly = new Regex(@"[^\d]");
					numberValue = digitsOnly.Replace(numberValue, " ");

					StringBuilder numberValueSB = new StringBuilder();
					numberValueSB.Append(numberValue);

					int i = numberValueSB.Length;
					while (--i > -1) {

						char current = numberValueSB.ToString()[i];
						if (Char.IsNumber(current) || current == '-') {
							numberValueSB.Insert(i + 1, ' ');
						}

					}

					numberValue = numberValueSB.ToString();


					StringBuilder sb = new StringBuilder();

					if (!string.IsNullOrWhiteSpace(numberLabel)) {
						sb.Append($"This number is labelled {numberLabel}, it is: ");
					}
					sb.Append(numberValue);

					string txt = sb.ToString();

					PlayTTS(txt, escapeAllKeys, Engine.Neural, VoiceId.Brian);

					firstNumber = false;
				}











				first = false;

			}

			// If first was false, it played at least one.
			if (first == false) {
				PlayTTS("There are no more phone numbers.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			} else {
				PlayTTS("There are no phone numbers to list.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
			}





















		}
	}
}
