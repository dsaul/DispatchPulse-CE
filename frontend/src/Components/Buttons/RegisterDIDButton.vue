<template>
	<v-btn class="ma-2" :loading="loading" :disabled="disabled || loading" color="green" style="color: white;"
		@click="OnClick">
		Register {{ DIDNumber.json.DIDNumber }}
	</v-btn>
</template>


<script lang="ts">
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { DID, IDID } from '@/Data/CRM/DID/DID';
import { IPerformPBXRegisterDIDCB } from '@/Data/CRM/DID/RPCPerformPBXRegisterDID';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { Component, Vue, Prop } from 'vue-property-decorator';


@Component({
	components: {

	},
})
export default class RegisterDIDButton extends Vue {

	@Prop({ default: null }) public readonly DIDNumber!: IDID | null;
	@Prop({ default: false }) public readonly disabled!: boolean;
	@Prop({ default: '' }) public readonly didRegisterPasscode!: string;

	protected loading = false;

	protected mounted(): void {

		//

	}

	protected OnClick(): void {

		if (!this.DIDNumber) {
			console.error('!this.DIDNumber');
			return;
		}

		console.log('OnClick');

		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			console.error('!contact');
			return;
		}

		this.loading = true;

		const rtr = DID.PerformPBXRegisterDID.Send({
			billingCompanyId: contact.companyId,
			did: this.DIDNumber.json.DIDNumber,
			didPassword: this.didRegisterPasscode,
			sessionId: BillingSessions.CurrentSessionId(),
		});
		if (rtr.completeRequestPromise !== null) {
			rtr.completeRequestPromise.then((payload: IPerformPBXRegisterDIDCB) => {
				if (payload.complete !== true) {
					Notifications.AddNotification({
						severity: 'error',
						message: 'There was an error de-registering this number.',
						autoClearInSeconds: 10,
					});
				}



				console.debug('IPerformPBXRegisterDIDCB', payload);
			});
			rtr.completeRequestPromise.finally(() => {
				this.loading = false;
				this.$emit('reload-status');
			});
		}
	}


}
</script>