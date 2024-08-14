<template>
	<VoicemailEditor v-model="Voicemail" ref="editor" :showAppBar="true" :showFooter="true" :breadcrumbs="Breadcrumbs"
		:preselectTabName="$route.query.tab" :isMakingNew="false" :isLoadingData="loadingData" @reload="LoadData()" />

</template>

<script lang="ts">
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import '@/plugins/store/Database';
import VoicemailEditor from '@/Components/Editors/VoicemailEditor.vue';
import { DateTime } from 'luxon';
import { IVoicemail, Voicemail } from '@/Data/CRM/Voicemail/Voicemail';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		VoicemailEditor,
	},
})
export default class VoicemailDisplay extends ViewBase {

	public $refs!: {
		editor: VoicemailEditor,
	};

	protected loadingData = false;




	public LoadData(): void {

		if (!this.$route.params.id || IsNullOrEmpty(this.$route.params.id)) {
			return;
		}

		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				const rtr = Voicemail.FetchForId(this.$route.params.id);
				if (rtr.completeRequestPromise) {

					this.loadingData = true;

					rtr.completeRequestPromise.finally(() => {
						this.loadingData = false;
					});
				}

			});
		});
	}

	protected MountedAfter(): void {

		if (!this.$route.params.id
			|| IsNullOrEmpty(this.$route.params.id)
		) {
			this.$router.push(`/section/Voicemails/`).catch(((e: Error) => { }));// eslint-disable-line
		}

		this.SwitchToTabFromRoute();
		this.LoadData();
	}

	protected SwitchToTabFromRoute(): void {
		this.$refs.editor.SwitchToTabFromRoute();
	}

	protected get Breadcrumbs(): Array<{
		text: string;
		disabled: boolean;
		to: string;
	}> {
		return [
			{
				text: 'Dashboard',
				disabled: false,
				to: '/',
			},
			{
				text: 'Voicemails',
				disabled: false,
				to: '/section/voicemails/index',
			},
			{
				text: 'Voicemail',
				disabled: true,
				to: '',
			},
		];
	}

	get Voicemail(): IVoicemail | null {

		const cal = Voicemail.ForId(this.$route.params.id);

		console.debug('this.$route.params.id', this.$route.params.id, cal);





		if (!cal) {
			return null;
		}
		return _.cloneDeep(cal) as IVoicemail;
	}


	set Voicemail(val: IVoicemail | null) {
		if (!val) {
			return;
		}

		val.lastModifiedISO8601 = DateTime.utc().toISO();
		val.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

		const id = this.$route.params.id;
		if (!id || IsNullOrEmpty(id)) {
			console.error('!id || IsNullOrEmpty(id)');
			return;
		}

		const payload: { [id: string]: IVoicemail; } = {};
		payload[id] = val;

		Voicemail.UpdateIds(payload);
	}








}
</script>
