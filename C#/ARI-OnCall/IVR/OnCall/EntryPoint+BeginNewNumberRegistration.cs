using AsterNET.FastAGI;
using SharedCode;
using Amazon.Polly;

namespace ARI.IVR.OnCall
{
	public partial class EntryPoint : AGIScriptPlus
	{
		protected void BeginNewNumberRegistration(AGIRequest request, AGIChannel channel, LeaveMessageRequestData data) {

			

			bool? response = PromptBooleanQuestion(new AudioPlaybackEvent[] {

				new AudioPlaybackEvent {
						Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
						Text = $"Welcome to On Call Responder, by Dispatch Pulse. We see " +
							$"that you called from {data.CallerIdNonDigitsRemovedWithSpaces}. This " +
							$"number is not in our database. Did you want to register it?",
					},
					new AudioPlaybackEvent {
						Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
						Text = "Press 1 for yes, or 2 for no.",
					},
				});

			if (null != response && response.Value && null != data.CallerIdNonDigitsRemoved) {

				string badHash = BadPhoneHash.CreateBadPhoneHash(data.CallerIdNonDigitsRemoved).WithSpacesBetweenLetters();

				PlayTTS($"Your registration code for the phone number {data.CallerIdNonDigitsRemovedWithSpaces} " +
					$"is {badHash}. Once again, your registration code is {badHash}. Please enter this number " +
					$"into the website to complete this registration.", string.Empty, Engine.Neural, VoiceId.Brian);

				throw new PerformHangupException();
			} else {

				PlayTTS($"Thank you, goodbye.", string.Empty, Engine.Neural, VoiceId.Brian);

				throw new PerformHangupException();
			}



		}
	}
}
