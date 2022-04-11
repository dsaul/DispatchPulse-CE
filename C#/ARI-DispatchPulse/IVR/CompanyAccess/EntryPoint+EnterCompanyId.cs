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
		protected async Task EnterCompanyId(
			AGIRequest request, 
			AGIChannel channel, 
			RequestData data
			) {
			// Request company id.
			int attemptCounter = 0;

			while (true) {

				if (attemptCounter >= maxRetryAttempts) {
					SubMenuMaxRetryAttempts(request, channel, data);
					return;
				}


				data.CompanyPhoneId = await PromptDigitsPoundTerminated(
					new AudioPlaybackEvent[] {
						new AudioPlaybackEvent {
							Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
							Text = "Please enter your company id followed by pound.",
						},
					},
					escapeAllKeys
				);

				if (string.IsNullOrEmpty(data.CompanyPhoneId)) {
					attemptCounter++;
					await PlayTTS("I didn't recieve a company id, please try again.", "", Engine.Neural, VoiceId.Brian);
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

				data.CompanyIdConfirmed = await PromptBooleanQuestion(new AudioPlaybackEvent[] {
					new AudioPlaybackEvent {
						Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
						Text = "Thanks, the ID I received was ",
					},
					new AudioPlaybackEvent {
						Type = AudioPlaybackEvent.AudioPlaybackEventType.TTSText,
						Text = data.CompanyPhoneId,
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

				if (data.CompanyIdConfirmed == null) {
					attemptCounter++;
					await PlayTTS("I didn't recieve an answer, please try again.", "", Engine.Neural, VoiceId.Brian);
					continue;
				}

				break;
			}

			// companyIdConfirmed should not be null at this point
			if (data.CompanyIdConfirmed == null) {
				throw new PerformHangupException();
			}

			attemptCounter = 0; // Reset the attempt counter.


			if (!data.CompanyIdConfirmed.Value) {
				// Start this menu over again.
				return;
			}

			if (null == data.BillingDB) {
				data.ConnectToBillingDB();
			}
			if (null == data.BillingDB) {
				await PlayTTS("There was an error while reading the database, please try again later. Code 233", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}


			var resCompany = BillingCompanies.ForPhoneId(data.BillingDB, data.CompanyPhoneId);
			if (0 == resCompany.Count) {
				await PlayTTS("I couldn't find a company for that ID, please try again.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			BillingCompanies company = resCompany.FirstOrDefault().Value;
			data.Company = company;

			if (null == company.Uuid) {
				await PlayTTS("There was an error while reading the database, please try again later. Code kl3.", escapeAllKeys, Engine.Neural, VoiceId.Brian);
				return;
			}

			await PlayTTS($"Ok, got it, you're from {data.Company.FullName}!", escapeAllKeys, Engine.Neural, VoiceId.Brian);

			// Look up dispatch pulse database name.

			var resPackages = BillingPackages.ForProvisionDispatchPulse(data.BillingDB, true);
			var resSubs = BillingSubscriptions.ForCompanyIdPackageIdsAndHasDatabase(data.BillingDB, company.Uuid.Value, resPackages.Keys);

			if (0 == resSubs.Count) {
				await PlayTTS("Hmm, I wasn't able to connect to your database, please contact support. Code kl2.", "", Engine.Neural, VoiceId.Brian);
				return;
			}



			data.Subscription = resSubs.FirstOrDefault().Value;

			if (null == data.Subscription || string.IsNullOrWhiteSpace(data.Subscription.ProvisionedDatabaseName)) {
				await PlayTTS("Hmm, I wasn't able to connect to your database, please contact support. Code kl1.", "", Engine.Neural, VoiceId.Brian);
				return;
			}

			await EnterAgentId(request, channel, data);
		}
	}
}
