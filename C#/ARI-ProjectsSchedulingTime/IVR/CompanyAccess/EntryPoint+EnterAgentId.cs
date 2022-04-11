using AsterNET.FastAGI;
using Amazon.Polly;
using System.Threading.Tasks;

namespace ARI.IVR.CompanyAccess
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected async Task EnterAgentId(
			AGIRequest request, 
			AGIChannel channel, 
			RequestData data
			) {

			// Request agent id.
			int attemptCounter = 0;

			while (true) {

				if (attemptCounter >= maxRetryAttempts) {
					SubMenuMaxRetryAttempts(request, channel, data);
					return;
				}

				data.AgentPhoneId = await PromptDigitsPoundTerminated(
					new AudioPlaybackEvent[] {
						new AudioPlaybackEvent {
							Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
							Text = "Please enter your agent id followed by pound.",
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

			// Confirm that we received the correct id.
			attemptCounter = 0; // Reset the attempt counter.

			while (true) {
				if (attemptCounter >= maxRetryAttempts) {
					SubMenuMaxRetryAttempts(request, channel, data);
					return;
				}

				data.AgentIdConfirmed = await PromptBooleanQuestion(new AudioPlaybackEvent[] {
					new AudioPlaybackEvent {
						Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
						Text = "Thanks, the ID i received was ",
					},
					new AudioPlaybackEvent {
						Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
						Text = data.AgentPhoneId,
					},
					new AudioPlaybackEvent {
						Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
						Text = "Is this correct?",

					},
					new AudioPlaybackEvent {
						Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
						Text = "Press 1 for yes, or 2 for no.",
					},
				});

				if (data.AgentIdConfirmed == null) {
					attemptCounter++;
					await PlayTTS("I didn't recieve an answer, please try again.", "", Engine.Neural, VoiceId.Brian);
					continue;
				}

				break;
			}

			// companyIdConfirmed should not be null at this point
			if (data.AgentIdConfirmed == null) {
				throw new PerformHangupException();
			}


			attemptCounter = 0; // Reset the attempt counter.


			if (!data.AgentIdConfirmed.Value) {
				// Start this menu over again.
				return;
			}

			await EnterPasscode(request, channel, data);
		}
	}
}
