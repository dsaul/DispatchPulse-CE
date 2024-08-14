<template>
	<DIDEditor v-model="DID" ref="editor" :showAppBar="true" :showFooter="true" :breadcrumbs="Breadcrumbs"
		:preselectTabName="$route.query.tab" :isMakingNew="false" :isLoadingData="loadingData" @reload="LoadData()" />

</template>

<script lang="ts">
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import '@/plugins/store/Database';
import DIDEditor from '@/Components/Editors/DIDEditor.vue';
import { DateTime } from 'luxon';
import { DID, IDID } from '@/Data/CRM/DID/DID';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		DIDEditor,
	},
})
export default class DIDDisplay extends ViewBase {

	public $refs!: {
		editor: DIDEditor,
	};

	protected loadingData = false;


	public LoadData(): void {

		if (!this.$route.params.id || IsNullOrEmpty(this.$route.params.id)) {
			return;
		}

		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				const rtr = DID.FetchForId(this.$route.params.id);
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
			this.$router.push(`/section/DIDs/`).catch(((e: Error) => { }));// eslint-disable-line
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
				text: 'Phone Numbers',
				disabled: false,
				to: '/section/dids/index',
			},
			{
				text: 'Phone Number',
				disabled: true,
				to: '',
			},
		];
	}

	get DID(): IDID | null {

		//console.debug('this.$route.params.id', this.$route.params.id);

		const cal = DID.ForId(this.$route.params.id);

		if (!cal) {
			return null;
		}
		return _.cloneDeep(cal) as IDID;
	}


	set DID(val: IDID | null) {
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

		const payload: Record<string, IDID> = {};
		payload[id] = val;
		DID.UpdateIds(payload);
	}








}
</script>
