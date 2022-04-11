using AsterNET.FastAGI;
using Npgsql;
using System.Linq;
using SharedCode.DatabaseSchemas;
using Amazon.Polly;
using System.Threading.Tasks;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected async Task EnterPasscode(AGIRequest request, AGIChannel channel, RequestData data) {
			// Request company id.
			int attemptCounter = 0;

			while (true) {

				if (attemptCounter >= maxRetryAttempts) {
					SubMenuMaxRetryAttempts(request, channel, data);
					return;
				}

				data.EnteredPasscode = await PromptDigitsPoundTerminated(
					new AudioPlaybackEvent[] {
						new AudioPlaybackEvent {
							Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
							Text = "Please enter your passcode followed by pound.",
						},
					},
					escapeAllKeys
				);

				if (string.IsNullOrEmpty(data.AgentPhoneId)) {
					attemptCounter++;
					await PlayTTS("I didn't recieve an answer, please try again.", "", Engine.Neural, VoiceId.Brian);
					continue;
				}

				break;
			}

			await PlayTTS("Let me check that...", "", Engine.Neural, VoiceId.Brian);

			if (null == data.Subscription || string.IsNullOrWhiteSpace(data.Subscription.ProvisionedDatabaseName)) {
				await PlayTTS("There was an error while reading the database, please try again later. Code 621", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}


			if (null == data.DPDB) {
				data.ConnectToDPDBName(data.Subscription.ProvisionedDatabaseName);
			}
			if (null == data.DPDB) {
				await PlayTTS("There was an error while reading the database, please try again later. Code 233", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			var resAgents = Agents.ForPhoneId(data.DPDB, data.AgentPhoneId);
			if (0 == resAgents.Count) {
				await PlayTTS("Sorry, I wasn't able to find an agent for those credentials. Code 9db", "", Engine.Neural, VoiceId.Brian);
				data.AgentPhoneId = null;
				data.AgentIdConfirmed = null;
				data.EnteredPasscode = null;
				await EnterAgentId(request, channel, data);
				return;
			}

			data.Agent = resAgents.FirstOrDefault().Value;

			if (string.IsNullOrWhiteSpace(data.Agent.Json)) {
				await PlayTTS("Sorry, I wasn't able to find an agent for those credentials. Code 42x", "", Engine.Neural, VoiceId.Brian);
				data.AgentPhoneId = null;
				data.AgentIdConfirmed = null;
				data.EnteredPasscode = null;
				await EnterAgentId(request, channel, data);
				return;
			}


			// Check the passcode.

			string? passcode = data.Agent.PhonePasscode;
			if (string.IsNullOrWhiteSpace(passcode)) {
				await PlayTTS("Sorry, I wasn't able to find an agent for those credentials. Code 1e2", "", Engine.Neural, VoiceId.Brian);
				data.AgentPhoneId = null;
				data.AgentIdConfirmed = null;
				data.EnteredPasscode = null;
				await EnterAgentId(request, channel, data);
				return;
			}


			if (passcode != data.EnteredPasscode) {
				await PlayTTS("Sorry, I wasn't able to find an agent for those credentials. Code 199", "", Engine.Neural, VoiceId.Brian);
				data.AgentPhoneId = null;
				data.AgentIdConfirmed = null;
				data.EnteredPasscode = null;
				await EnterAgentId(request, channel, data);
				return;
			}


			// Greet the agent if we have a name.
			do {
				data.AgentName = data.Agent.Name;
				if (string.IsNullOrWhiteSpace(data.AgentName))
					break;

				await PlayTTS($"Greetings {data.AgentName}.", escapeAllKeys, Engine.Neural, VoiceId.Brian);

			} while (false);

			await AgentOverview(request, channel, data);
		}
	}
}
