<template>
	<OnCallAutoAttendantEditor
		v-model="OnCallAutoAttendant"
		ref="editor"
		:showAppBar="true"
		:showFooter="true"
		:breadcrumbs="Breadcrumbs"
		:preselectTabName="$route.query.tab"
		:isMakingNew="false"
		:isLoadingData="loadingData"
		@reload="LoadData()"
		/>
	
</template>

<script lang="ts">
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import '@/plugins/store/Database';
import OnCallAutoAttendantEditor from '@/Components/Editors/OnCallAutoAttendantEditor.vue';
import { DateTime } from 'luxon';
import { IOnCallAutoAttendant, OnCallAutoAttendant } from '@/Data/CRM/OnCallAutoAttendant/OnCallAutoAttendant';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		OnCallAutoAttendantEditor,
	},
})
export default class OnCallAutoAttendantDisplay extends ViewBase {
	
	public $refs!: {
		editor: OnCallAutoAttendantEditor,
	};
	
	protected loadingData = false;
	
	constructor() {
		super();
		
	}
	
	
	
	public LoadData(): void {
		
		if (!this.$route.params.id || IsNullOrEmpty(this.$route.params.id)) {
			return;
		}
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				const rtr = OnCallAutoAttendant.FetchForId(this.$route.params.id);
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
			this.$router.push(`/section/OnCallAutoAttendants/`).catch(((e: Error) => { }));// eslint-disable-line
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
				text: 'On Call Responders',
				disabled: false,
				to: '/section/on-call/index',
			},
			{
				text: 'Responder',
				disabled: true,
				to: '',
			},
		];
	}
	
	get OnCallAutoAttendant(): IOnCallAutoAttendant | null {
		
		//console.debug('this.$route.params.id', this.$route.params.id);
		
		const cal = OnCallAutoAttendant.ForId(this.$route.params.id);
		
		if (!cal) {
			return null;
		}
		return _.cloneDeep(cal) as IOnCallAutoAttendant;
	}
	
	
	set OnCallAutoAttendant(val: IOnCallAutoAttendant | null) {
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
		
		const payload: { [id: string]: IOnCallAutoAttendant; } = {};
		payload[id] = val;
		
		OnCallAutoAttendant.UpdateIds(payload);
	}

	
	
	
	
	
	
	
}
</script>
