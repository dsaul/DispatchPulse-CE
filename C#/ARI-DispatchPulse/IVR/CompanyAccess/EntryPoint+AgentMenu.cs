using AsterNET.FastAGI;
using Amazon.Polly;
using Databases.Records.CRM;
using System.Linq;
using System.Collections.Generic;
using SharedCode;
using System;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void AgentMenu(
			AGIRequest request, 
			AGIChannel channel, 
			RequestData data
			) {

			// Try to get active labour
			if (null != data.DPDB && null != data.Agent && null != data.Agent.Id) {
				data.AgentActiveLabour.Clear();
				var resLabour = Labour.ForAgentIDIsActive(data.DPDB, data.Agent.Id.Value, true);
				data.AgentActiveLabour.AddRange(resLabour);
			}
			

			//

			while (true) {

				char key = PlayTTS("Main Menu.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

				//string hex = Convert.ToByte(key).ToString("x2");

				//Log.Debug($"key {hex} data.AgentActiveLabour.Count {data.AgentActiveLabour.Count}");

				if (key == '\0' && data.AgentActiveLabour.Count > 0) {
					key = PlayTTS("You have an active labour entry, press 9 to jump straight to this entry.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				}
				if (key == '\0') {
					key = PlayTTS("Press 1 to go over your assignments. Press 0 to hear the overview again. Press star to hear the menu again, or press pound to log-off.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				}
				if (key == '\0') {
					key = WaitForDigit(5000);
				}

				switch (key) {
					case '0':
						AgentOverview(request, channel, data);
						return;
					case '1':
						AssignmentsList(request, channel, data);
						return;
					case '*':
						return;
					case '#':
						throw new PerformHangupException();
					case '9':
						if (0 == data.AgentActiveLabour.Count) {
							PlayTTS("That isn't a valid option, please try again.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
							break;
						}
						if (null == data.DPDB)
							break;

						foreach (KeyValuePair<Guid, Labour> kvp in data.AgentActiveLabour) {
							if (kvp.Value.AssignmentId == null) {
								PlayTTS("Labour entries that don't have an assignment can't currently be accessed over the phone, please go to a p p dot dispatch pulse dot ca and login there to edit these.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
								continue;
							}

							var resAssignment = Assignments.ForId(data.DPDB, kvp.Value.AssignmentId.Value);
							if (resAssignment.Count == 0) {
								PlayTTS("We cannot find the assignment to edit, please go to a p p dot dispatch pulse dot ca and login there to edit this entry.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
								continue;
							}

							Assignments assignment = resAssignment.FirstOrDefault().Value;
							AssignmentDetail(request, channel, data, assignment);
						}

						break;
					default:
						PlayTTS("That isn't a valid option, please try again.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
						break;
				}


			}


		}
	}
}
