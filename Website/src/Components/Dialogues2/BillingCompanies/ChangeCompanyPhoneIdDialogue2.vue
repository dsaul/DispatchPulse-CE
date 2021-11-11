<template>
	<v-dialog
		v-model="isOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>{{dialogueName}}</v-card-title>
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
									v-model="CurrentPhoneId"
									autocomplete="newpassword"
									label="Current Phone ID"
									type="text"
									hint=""
									:readonly="true"
									>
								</v-text-field>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="8" offset-sm="2">
								<v-text-field
									v-model="NewPhoneId"
									autocomplete="newpassword"
									label="New Phone ID"
									type="text"
									hint=""
									>
								</v-text-field>
								
								<v-alert
									v-if="companyPhoneIdInUse && CurrentPhoneId == NewPhoneId"
									type="warning"
									colored-border
									elevation="2"
									border="bottom"
									>
									Your company already has this ID.
								</v-alert>
								<v-alert
									v-else-if="companyPhoneIdInUse"
									type="error"
									colored-border
									elevation="2"
									border="bottom"
									>
									This ID is already in use, and cannot be saved.
								</v-alert>
							</v-col>
						</v-row>
					</v-container>
				</v-form>
				

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="red darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="green darken-1" text @click="Save()" :disabled="companyPhoneIdInUse">Save</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component, Vue, Prop, Watch } from 'vue-property-decorator';
import BillingContactEditor from '@/Components/Editors/BillingContactEditor.vue';
import DialogueBase2 from '@/Components/Dialogues2/DialogueBase2';
import _ from 'lodash';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { BillingCompanies } from '@/Data/Billing/BillingCompanies/BillingCompanies';
import { IChangeCompanyPhoneIdModel } from '@/Data/Models/ChangeCompanyPhoneIdModel/ChangeCompanyPhoneIdModel';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		BillingContactEditor,
	},
})
export default class ChangeCompanyPhoneIdDialogue2 extends DialogueBase2 {
	
	@Prop({ default: null }) declare public readonly value: IChangeCompanyPhoneIdModel | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	
	public $refs!: {
		
	};
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected companyPhoneIdInUse = false;
	
	public SwitchToTabFromRoute(): void {
		// if (this.$refs.editor) {
		// 	this.$refs.editor.SwitchToTabFromRoute();
		// }
		
	}
	
	
	protected get CurrentPhoneId(): guid {
		return this.value?.originalId || '';
	}
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected set CurrentPhoneId(payload: string) {
		
		//
	}
	
	protected get NewPhoneId(): guid {
		return this.value?.newId || '';
	}
	
	protected set NewPhoneId(payload: string) {
		
		if (!this.value) {
			return;
		}
		
		payload = payload.replace('a', '2');
		payload = payload.replace('b', '2');
		payload = payload.replace('c', '2');
		
		payload = payload.replace('d', '3');
		payload = payload.replace('e', '3');
		payload = payload.replace('f', '3');
		
		payload = payload.replace('g', '4');
		payload = payload.replace('h', '4');
		payload = payload.replace('i', '4');
		
		payload = payload.replace('j', '5');
		payload = payload.replace('k', '5');
		payload = payload.replace('l', '5');
		
		payload = payload.replace('m', '6');
		payload = payload.replace('n', '6');
		payload = payload.replace('o', '6');
		
		payload = payload.replace('p', '7');
		payload = payload.replace('q', '7');
		payload = payload.replace('r', '7');
		payload = payload.replace('s', '7');
		
		payload = payload.replace('t', '8');
		payload = payload.replace('u', '8');
		payload = payload.replace('v', '8');
		
		payload = payload.replace('w', '9');
		payload = payload.replace('x', '9');
		payload = payload.replace('y', '9');
		payload = payload.replace('z', '9');
		
		payload = payload.replace(/\D/g, '');
		
		//console.debug(`${payload}`);
		
		const clone = _.cloneDeep(this.value);
		clone.newId = payload;
		
		//console.debug(`${JSON.stringify(clone)}`);
		
		this.$emit('input', clone);
		
		
	}
	
	@Watch('value')
	protected valueChanged(val: string, oldVal: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars
		
		this.UpdateCheckInUse();
		
		// console.log('valueChanged', val);
	}
	
	protected UpdateCheckInUse(): void {
		
		// console.debug('UpdateCheckInUse', this.value);
		
		if (!this.value || !this.value.newId) {
			Vue.set(this, 'companyPhoneIdInUse', false);
			return;
		}
		
		const rtr = BillingCompanies.PerformCheckCompanyPhoneIdInUse.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			companyId: this.value?.newId || null,
		});
		if (rtr.completeRequestPromise) {
			rtr.completeRequestPromise.then((payload: {inUse: boolean;}) => {
				
				Vue.set(this, 'companyPhoneIdInUse', payload.inUse);
				
				// console.log(payload);
			});
		}
		
	}
	
	
	protected Cancel(): void {
		// console.log('Cancel');
		
		
		// this.$refs.editor.ResetValidation();
		
		this.$emit('cancel', null);
		
		// this.$refs.editor.SelectFirstTab();
		// this.$refs.editor.ResetPasswordToDefault();
	}
	
	protected Save(): void {
		// console.log('Save', this.$refs);
		
		// if (this.$refs.editor.IsValidated()) {
			
			this.$emit('save', this.value);
			
		// 	this.$refs.editor.ResetValidation();
		// 	this.$refs.editor.SelectFirstTab();
		// 	this.$refs.editor.ResetPasswordToDefault();
		// } else {
		// 	Notifications.AddNotification({
		// 		severity: 'error',
		// 		message: 'Some of the form fields didn\'t pass validation.',
		// 		autoClearInSeconds: 10,
		// 	});
		// }
		
	}
	
	
}
</script>