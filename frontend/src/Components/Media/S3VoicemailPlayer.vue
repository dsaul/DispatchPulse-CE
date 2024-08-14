<template>
	<div>
		<audio v-if="hasS3Link" controls preload="auto" autoplay>
			<source :src="s3URI">
			Your browser does not support the audio element.
		</audio>
		<!-- <a v-if="hasS3Link" :href="s3URI">test</a> -->
		<v-btn v-else class="ma-2" :loading="loading" :disabled="disabled || loading" color="green"
			style="color: white;" @click="OnClick">
			Click to Play
		</v-btn>

	</div>
</template>


<script lang="ts">
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { IPerformGetVoicemailRecordingLinkCB } from '@/Data/CRM/Voicemail/RPCPerformGetVoicemailRecordingLink';
import { IVoicemail, Voicemail } from '@/Data/CRM/Voicemail/Voicemail';
import { Component, Vue, Prop } from 'vue-property-decorator';


@Component({
	components: {

	},
})
export default class S3VoicemailPlayer extends Vue {

	@Prop({ default: null }) public readonly voicemail!: null | IVoicemail;
	@Prop({ default: false }) public readonly disabled!: boolean;

	protected loading = false;
	protected hasS3Link = false;
	protected s3URI: string | null = null;

	protected mounted(): void {

		//

	}

	protected OnClick(): void {
		if (!this.voicemail || !this.voicemail.id) {
			console.error('!this.voicemail || !this.voicemail.id');
			return;
		}

		const sessionId = BillingSessions.CurrentSessionId();
		if (!sessionId) {
			console.error('!sessionId');
			return;
		}

		const contact = BillingContacts.ForCurrentSession();
		if (!contact || !contact.companyId) {
			console.error('!contact || !contact.companyId');
			return;
		}

		this.loading = true;

		const rtr = Voicemail.PerformGetVoicemailRecordingLink.Send({
			sessionId,
			voicemailId: this.voicemail.id,
			billingCompanyId: contact.companyId,
		});
		if (rtr.completeRequestPromise) {
			rtr.completeRequestPromise.then((payload: IPerformGetVoicemailRecordingLinkCB) => {
				this.s3URI = payload.voicemailURI;
				this.hasS3Link = true;
			});
			rtr.completeRequestPromise.finally(() => {
				this.loading = false;
			});
		}

	}


}
</script>