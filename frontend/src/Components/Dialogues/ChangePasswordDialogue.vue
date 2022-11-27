<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>Change Password</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<v-form
						autocomplete="newpassword"
						ref="generalForm"
						>
						<v-container>
							<v-row>
								<v-col cols="12" sm="8" offset-sm="2">
									<v-text-field
										v-model="CurrentPassword"
										autocomplete="newpassword"
										label="Current Password"
										type="password"
										hint=""
										:rules="[
											ValidateRequiredField,
										]"
										>
									</v-text-field>
								</v-col>
							</v-row>
							<v-row>
								<v-col cols="12" sm="8" offset-sm="2">
									<v-text-field
										v-model="NewPassword1"
										autocomplete="newpassword"
										label="New Password"
										type="password"
										hint=""
										:rules="[
											ValidateRequiredField,
											PasswordEntropyAtLeast50,
										]"
										>
									</v-text-field>
								</v-col>
							</v-row>
							<v-row>
								<v-col cols="12" sm="8" offset-sm="2">
									<v-text-field
										v-model="NewPassword2"
										autocomplete="newpassword"
										label="New Password Again"
										type="password"
										hint=""
										:rules="[
											ValidateRequiredField,
											PasswordEntropyAtLeast50,
										]"
										>
									</v-text-field>
								</v-col>
							</v-row>
						</v-container>
				</v-form>
				
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="red darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="green darken-1" text @click="Save()">Save</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import bcrypt from 'bcryptjs';
import PasswordEntropyAtLeast50 from '@/Utility/Validators/PasswordEntropyAtLeast50';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import GenerateID from '@/Utility/GenerateID';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IPerformChangeSessionPasswordCB } from '@/Data/Billing/BillingContacts/RPCPerformChangeSessionPassword';

interface ChangePasswordDialogueModelState {
	currentPassword: string | null;
	newPassword1: string | null;
	newPassword2: string | null;
}

export interface IUpdatePasswordQuery {
	session: string;
	currentPassword: string | null;
	newHash: string | null;
	nocache: number;
}

@Component({
	components: {
		
	},
})
export default class ChangePasswordDialogue extends DialogueBase {
	
	public static GenerateEmpty(): ChangePasswordDialogueModelState {
		return {
			currentPassword: null,
			newPassword1: null,
			newPassword2: null,
		};
	}
	
	
	
	public $refs!: {
		generalForm: HTMLFormElement,
	};
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	protected PasswordEntropyAtLeast50 = PasswordEntropyAtLeast50;
	protected ValidateRequiredField = ValidateRequiredField;
	
	constructor() {
		super();
		this.ModelState = ChangePasswordDialogue.GenerateEmpty();
	}
	
	get DialogueName(): string {
		return 'ChangePasswordDialogue';
	}
	
	
	get CurrentPassword(): string | null {
		
		if (!this.ModelState ||
			!this.ModelState.currentPassword
			) {
			return null;
		}
		
		return this.ModelState.currentPassword;
	}
	
	set CurrentPassword(val: string | null) {
		
		if (!this.ModelState 
			) {
			return;
		}
		
		this.ModelState.currentPassword = IsNullOrEmpty(val) ? null : val;
	}
	
	get NewPassword1(): string | null {
		
		if (!this.ModelState ||
			!this.ModelState.newPassword1
			) {
			return null;
		}
		
		return this.ModelState.newPassword1;
	}
	
	set NewPassword1(val: string | null) {
		
		if (!this.ModelState 
			) {
			return;
		}
		
		this.ModelState.newPassword1 = IsNullOrEmpty(val) ? null : val;
	}
	
	get NewPassword2(): string | null {
		
		if (!this.ModelState ||
			!this.ModelState.newPassword2
			) {
			return null;
		}
		
		return this.ModelState.newPassword2;
	}
	
	set NewPassword2(val: string | null) {
		
		if (!this.ModelState 
			) {
			return;
		}
		
		this.ModelState.newPassword2 = IsNullOrEmpty(val) ? null : val;
	}
	
	
	protected Cancel(): void {
		console.info('Cancel');
		
		
		this.$refs.generalForm.resetValidation();
		Dialogues.Close(this.DialogueName);
		this.ModelState = ChangePasswordDialogue.GenerateEmpty();
	}
	
	protected Save(): void {
		
		console.info('Save');
		
		// This is a big function so lets decouple it from the click handler.
		Vue.nextTick(() => {
			
			if (this.CurrentPassword == null || IsNullOrEmpty(this.CurrentPassword)) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: 'The current password is blank.',
					autoClearInSeconds: 10,
				});
				
				return;
			}
			
			if (this.NewPassword1 !== this.NewPassword2) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: 'The new passwords don\'t match.',
					autoClearInSeconds: 10,
				});
				
				return;
			}
			
			if (this.NewPassword1 == null || 
				IsNullOrEmpty(this.NewPassword1) || 
				this.NewPassword2 == null ||
				IsNullOrEmpty(this.NewPassword2)
			) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'The new passwords cannot be empty.',
					autoClearInSeconds: 10,
				});
				return;
			}
			
			if (!this.$refs.generalForm.validate()) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Some of the form fields didn\'t pass validation.',
					autoClearInSeconds: 10,
				});
				return;
			}
			
			
			const salt = bcrypt.genSaltSync(11);
			const hash = bcrypt.hashSync(this.NewPassword1, salt);
			
			const currentSessionId = BillingSessions.CurrentSessionId();
			
			if (currentSessionId) {
				const rtr = BillingContacts.PerformChangeSessionPassword.Send({
					idempotencyToken: GenerateID(),
					currentPassword: this.CurrentPassword,
					newHash: hash,
					sessionId: currentSessionId,
				});
				
				if (!rtr || !rtr.completeRequestPromise) {
					return;
				}
				
				
				rtr.completeRequestPromise.then((payload: IPerformChangeSessionPasswordCB) => {
					
					if (payload.isError) {
						
						Notifications.AddNotification({
							severity: 'error',
							message: payload.errorMessage,
							autoClearInSeconds: 10,
						});
						
					} else if (payload.passwordChanged) {
						
						console.log('Change Password Success');
						
						this.$refs.generalForm.resetValidation();
						Dialogues.Close(this.DialogueName);
						this.ModelState = ChangePasswordDialogue.GenerateEmpty();
						
					} else {
						
						Notifications.AddNotification({
							severity: 'info',
							message: 'No error, but not success either.',
							autoClearInSeconds: 10,
						});
						
					}
					
					
				});
		
				console.log('change password rtr ', rtr);
			}
			
			
			
			
			
			
			
			
			
		});
		
		
		
		
	}
	
}
</script>