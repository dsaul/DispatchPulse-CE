<template>
	<v-dialog v-model="ShowLoginDialogue" max-width="600" persistent :fullscreen="MobileDeviceWidth()"
		class="e2e-login-dialogue">
		<v-card>


			<v-row no-gutters>
				<v-col class="d-none d-sm-block" cols="12" sm="6" style="padding: 30px; text-align: center;">
					<p><img src="@/assets/Dispatch Pulse Logo v1.svg" style="max-width: 64px; border-radius: 10px;" />
					</p>
					<p class="d-none d-sm-block" style="font-size: 24px; text-align: center;">Dispatch Pulse</p>
					<p class="d-none d-sm-block">
						<v-btn color="primary" href="https://www.dispatchpulse.com/Contact" target="_blank"
							style="width: 150px;">
							<v-icon left>mdi-phone</v-icon>
							Sign Up
						</v-btn>
					</p>
					<p class="d-none d-sm-block">
						<v-btn color="primary" href="https://www.dispatchpulse.com/Contact" target="_blank"
							style="width: 150px;">
							<v-icon left>mdi-phone</v-icon>
							Support
						</v-btn>
					</p>
					<p class="d-none d-sm-block"><v-btn color="primary" @click="OnClickTryADemo"
							style="width: 150px;">Take a Look</v-btn></p>

					<p class="d-none d-sm-block"><v-btn @click="OnClickLearnMore" style="width: 150px;">Learn
							More</v-btn></p>

				</v-col>
				<v-col cols="12" sm="6" style="padding: 10px;">
					<v-form @keyup.native.enter="OnClickLogin">
						<v-container>
							<v-row style="margin-top: 12px; margin-bottom: 15px;">
								<v-col cols="10" sm="10" offset="1"
									style="padding: 0px; margin-top:0px; margin-bottom:0px;">

									<v-card-title class="headline" style="padding:0px; display: flex;">
										<img class="d-inline d-sm-none" src="@/assets/Dispatch Pulse Logo v1.svg"
											style="max-width: 64px; border-radius: 10px; margin-right: 10px;" />
										<div style="flex: 1; word-break: break-word;"><span
												class="d-inline d-sm-none">Dispatch Pulse </span>Login</div>
									</v-card-title>
								</v-col>
							</v-row>

							<v-row style="margin-top: 15px; margin-bottom: 20px;">
								<v-col cols="10" sm="10" offset="1"
									style="padding: 0px; margin-top:0px; margin-bottom:0px;">
									<v-text-field v-model="companyAbbreviation" :disabled="true"
										label="Company Abbreviation" prepend-icon="business"
										hint="The short abbreviated name for your company. Example: &quot;DP&quot; for Dispatch Pulse"
										persistent-hint class="e2e-login-company-abbreviation">
									</v-text-field>
								</v-col>
							</v-row>
							<v-row style="margin-top: 20px; margin-bottom: 20px;">
								<v-col cols="10" sm="10" offset="1"
									style="padding: 0px; margin-top:0px; margin-bottom:0px;">
									<v-text-field v-model="contactEMail" label="E-Mail Address"
										prepend-icon="account_circle" type="email" class="e2e-login-email">
									</v-text-field>
								</v-col>
							</v-row>
							<v-row style="margin-top: 20px; margin-bottom: 20px;">
								<v-col cols="10" sm="10" offset="1"
									style="padding: 0px; margin-top:0px; margin-bottom:0px;">
									<v-text-field v-model="contactPassword" label="Password" prepend-icon="lock_outline"
										type="password" class="e2e-login-password">
									</v-text-field>
								</v-col>
							</v-row>
							<v-row>
								<v-col cols="10" offset="1" style="padding: 0px;">
									<v-card-actions>
										<v-btn color="primary" @click="OnClickSignUp" class="d-flex d-sm-none"
											style="min-width: 120px;">Sign Up</v-btn>
										<v-spacer></v-spacer>
										<v-btn color="primary" @click="OnClickLogin" style="min-width: 120px;"
											class="e2e-login-submit-button">
											Login
										</v-btn>
									</v-card-actions>
								</v-col>
							</v-row>
							<v-row class="d-block d-sm-none">
								<v-col cols="10" offset="1" style="padding: 0px;">
									<v-card-actions>
										<v-btn text style="min-width: 120px; text-align: center;"
											@click="OnClickTryADemo">Try a Demo</v-btn>
										<v-spacer></v-spacer>
										<v-btn text style="min-width: 120px; text-align: center;"
											@click="OnClickLearnMore">Learn More</v-btn>
									</v-card-actions>
								</v-col>
							</v-row>
						</v-container>
					</v-form>
				</v-col>
			</v-row>

		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import SignalRConnection from '@/RPC/SignalRConnection';

@Component({
	components: {

	},
})
export default class LoginDialogue extends DialogueBase {
	public companyAbbreviation = 'CWE';
	public contactEMail = '';
	public contactPassword = '';



	protected CurrentSessionId = BillingSessions.CurrentSessionId;
	protected MobileDeviceWidth = MobileDeviceWidth;

	protected get ShowLoginDialogue(): boolean {

		const noSession = IsNullOrEmpty(BillingSessions.CurrentSessionId());
		const openDemoWindow = Dialogues.IsDialogueOpen('DemoIntroductionDialogue');
		const openRegisterWindow = Dialogues.IsDialogueOpen('RegisterDialogue');
		const demoMode = GetDemoMode();

		//console.debug('openDemoWindow', openDemoWindow);

		return noSession && !openDemoWindow && !demoMode && !openRegisterWindow;
	}

	get DialogueName(): string {
		return 'LoginDialogue';
	}

	public OnClickLearnMore(): void {
		//console.log('OnClickLearnMore');
		window.open('https://www.dispatchpulse.com/');
	}

	public OnClickTryADemo(): void {
		//console.log('OnClickTryADemo');


		Dialogues.Open({
			name: 'DemoIntroductionDialogue',
			state: null,
		});
	}

	public OnClickSignUp(): void {
		console.log('OnClickSignUp');

		Dialogues.Open({
			name: 'RegisterDialogue',
			state: null,
		});
	}

	public OnClickLogin(): void {
		console.log('OnClickLogin()', this);

		if (GetDemoMode()) {
			return;
		}

		if (!this.companyAbbreviation || IsNullOrEmpty(this.companyAbbreviation)) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Company Abbreviation cannot be empty.',
				autoClearInSeconds: 10,
			});
			return;
		}

		if (!this.contactEMail || IsNullOrEmpty(this.contactEMail)) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'E-Mail Address cannot be empty.',
				autoClearInSeconds: 10,
			});
			return;
		}

		if (!this.contactPassword || IsNullOrEmpty(this.contactPassword)) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Password cannot be empty.',
				autoClearInSeconds: 10,
			});
			return;
		}

		SignalRConnection.Ready(() => {

			const rtr = BillingSessions.PerformCreateBillingSessionForCredentials.Send({
				sessionId: null,
				companyAbbreviation: this.companyAbbreviation,
				contactEMail: this.contactEMail,
				contactPassword: this.contactPassword,
			});
			if (rtr.completeRequestPromise) {
				rtr.completeRequestPromise.then(() => {

					//console.log('then');

					this.companyAbbreviation = '';
					this.contactEMail = '';
					this.contactPassword = '';

					requestAnimationFrame(() => {
						this.$emit('OnLogin', null);
					});


				});
				rtr.completeRequestPromise.catch((error: Error) => {

					//console.log('catch');

					Notifications.AddNotification({
						severity: 'error',
						message: error.message,
						autoClearInSeconds: 10,
					});
					console.error('GetNewSession error', error);
				});
			}

		});
















	}


}
</script>