<template>
	<v-card>
		<v-card-text>
			<v-row>
				<v-col cols="12" sm="10" offset-sm="1">
					<div class="title">Phone Id</div>
				</v-col>
			</v-row>
			<v-row>
				<v-col cols="12" sm="10" offset-sm="1">
					
					<ChangeCompanyPhoneIdDialogue2 
						v-model="changeCompanyPhoneIdModel"
						:isOpen="changeCompanyPhoneIdOpen"
						@save="SaveChangeCompanyPhoneId"
						@cancel="CancelChangeCompanyPhoneId"
						ref="changeCompanyPhoneIdDialogue"
						dialogueName="Change Company Phone ID"
						/>
					
					
					
					<v-text-field
						:disabled="disabled"
						:readonly="true"
						v-model="CompanyPhoneId"
						autocomplete="newpassword"
						label="Phone ID#"
						hint="The phone id # is used when calling in to access company access."
						persistent-hint
						>
						<template v-slot:append-outer>
							<v-btn
								text
								:disabled="disabled || !PermBillingCompaniesCanModifyPhoneId() || !PermBillingCompaniesCanRequest()"
								@click="ChangePhoneId()"
								>
								Change
							</v-btn>
						</template>
					</v-text-field>
				</v-col>
			</v-row>
		</v-card-text>
	</v-card>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import CardBase from '@/Components/Cards/CardBase';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import ChangeCompanyPhoneIdDialogue2 from '@/Components/Dialogues2/BillingCompanies/ChangeCompanyPhoneIdDialogue2.vue';
import { ChangeCompanyPhoneIdModel, IChangeCompanyPhoneIdModel } from '@/Data/Models/ChangeCompanyPhoneIdModel/ChangeCompanyPhoneIdModel';
import { BillingCompanies } from '@/Data/Billing/BillingCompanies/BillingCompanies';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { IPerformUpdatePhoneIdCB } from '@/Data/Billing/BillingCompanies/RPCPerformUpdatePhoneId';

@Component({
	components: {
		PermissionsDeniedAlert,
		ChangeCompanyPhoneIdDialogue2,
	},
})
export default class PhoneIdCard extends CardBase {
	
	public $refs!: {
		changeCompanyPhoneIdDialogue: ChangeCompanyPhoneIdDialogue2,
	};
	
	
	
	protected GetDemoMode = GetDemoMode;
	protected PermBillingCompaniesCanModifyPhoneId = BillingCompanies.PermBillingCompaniesCanModifyPhoneId;
	protected PermBillingCompaniesCanRequest = BillingCompanies.PermBillingCompaniesCanRequest;
	
	protected changeCompanyPhoneIdModel: IChangeCompanyPhoneIdModel | null = null;
	protected changeCompanyPhoneIdOpen = false;
	
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		// if (this.$refs.assignmentsList && this.$refs.assignmentsList.IsLoadingData) {
		// 	return true;
		// }
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		//console.log('@@@');
		
		this.loadingData = true;
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
			
			this.loadingData = false;
			
		}, 250);
		
		// if (this.$refs.assignmentsList) {
		// 	this.$refs.assignmentsList.LoadData();
		// }
		
	}
	
	
	protected ChangePhoneId(): void {
		
		
		this.changeCompanyPhoneIdModel = ChangeCompanyPhoneIdModel.GetEmpty();
		
		
		const originalId = BillingCompanies.CompanyPhoneIdForCurrentBillingContact();
		
		this.changeCompanyPhoneIdOpen = true;
		this.changeCompanyPhoneIdModel.originalId = originalId;
		this.changeCompanyPhoneIdModel.newId = originalId;
		
		requestAnimationFrame(() => {
			if (this.$refs.changeCompanyPhoneIdDialogue) {
				this.$refs.changeCompanyPhoneIdDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
		// console.log('ChangePhoneId()');
		
		
		
	}
	
	protected SaveChangeCompanyPhoneId(): void {
		console.debug('SaveChangeCompanyPhoneId', this.changeCompanyPhoneIdModel?.newId);
		
		const oldId = this.changeCompanyPhoneIdModel?.originalId || null;
		const newId = this.changeCompanyPhoneIdModel?.newId || null;
		if (!newId || IsNullOrEmpty(newId)) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'New phone id is blank!',
				autoClearInSeconds: 10,
			});
			
			
			return;
		}
		
		
		if (oldId === newId) {
			Notifications.AddNotification({
				severity: 'warning',
				message: 'Old ID is the same as New ID, no need up update.',
				autoClearInSeconds: 10,
			});
			
			
			return;
		}
		
		
		const rtr = BillingCompanies.PerformUpdatePhoneId.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			newPhoneId: newId,
		});
		if (rtr.completeRequestPromise) {
			rtr.completeRequestPromise.then((payload: IPerformUpdatePhoneIdCB) => {
				
				if (!payload.isError) {
					
					this.changeCompanyPhoneIdOpen = false;
					
					requestAnimationFrame(() => {
						this.changeCompanyPhoneIdModel = ChangeCompanyPhoneIdModel.GetEmpty();
					});
					
				}
				
				
			});
		}
		
		
		
		
		
		
		
		
		
		
	}
	
	
	protected CancelChangeCompanyPhoneId(): void {
		// console.debug('CancelChangeCompanyPhoneId');
		
		
		this.changeCompanyPhoneIdOpen = false;
		
		requestAnimationFrame(() => {
			this.changeCompanyPhoneIdModel = ChangeCompanyPhoneIdModel.GetEmpty();
		});
	}
	
	protected get CompanyPhoneId(): string | null {
		
		return BillingCompanies.CompanyPhoneIdForCurrentBillingContact();
	}
	
	protected set CompanyPhoneId(payload: string | null) {
		
		// Can't set the phone id from the text field.
		
	}
	
}
</script>