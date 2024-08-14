<template>
	<v-skeleton-loader v-if="didRegInfo == null" type="chip" style="display: inline-block;">
	</v-skeleton-loader>
	<v-chip v-else :color="chipColor" label outlined style="margin: 4px;" small>
		{{ chipText }}
	</v-chip>
</template>


<script lang="ts">
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { DID, IDID } from '@/Data/CRM/DID/DID';
import { IPerformCheckDIDPBXRegisteredCB } from '@/Data/CRM/DID/RPCPerformCheckDIDPBXRegistered';
import { Component, Vue, Prop } from 'vue-property-decorator';


@Component({
	components: {

	},
})
export default class DIDRegisteredChip extends Vue {

	@Prop({ default: null }) public readonly DIDNumber!: IDID | null;

	public didRegInfo: IPerformCheckDIDPBXRegisteredCB | null = null;
	protected chipColor = 'orange';
	protected chipText = '';

	public LoadData(): void {

		this.didRegInfo = null;
		this.chipColor = 'orange';
		this.chipText = '';

		if (null == this.DIDNumber) {
			return;
		}

		//console.debug('DIDNumber', this.DIDNumber);

		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			console.error('!contact');
			return;
		}


		const rtr = DID.PerformCheckDIDPBXRegistered.Send({
			billingCompanyId: contact.companyId,
			did: this.DIDNumber.json.DIDNumber,
			sessionId: BillingSessions.CurrentSessionId(),
		});
		if (rtr.completeRequestPromise !== null) {
			rtr.completeRequestPromise.then((payload: IPerformCheckDIDPBXRegisteredCB) => {

				//console.debug('IPerformCheckDIDPBXRegisteredCB', payload);

				if (false === payload.isRegistered) {
					this.chipColor = 'orange';
					this.chipText = 'Not Registered';
				} else {

					if (payload.isRegisteredToDifferentCompany) {
						this.chipColor = 'red';
						this.chipText = 'Registered to a Different Company';
					} else if (payload.isRegisteredToUnknownCompany) {
						this.chipColor = 'red';
						this.chipText = 'Registered to an Unknown Company';
					} else {
						this.chipColor = 'green';
						this.chipText = 'Registered';
					}


				}


				Vue.set(this, 'didRegInfo', payload);

			});
		}

	}

	protected mounted(): void {
		this.LoadData();
	}


}
</script>