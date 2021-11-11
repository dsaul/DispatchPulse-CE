<template>
	<CompanyEditor
		v-model="Company"
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
import CompanyEditor from '@/Components/Editors/CompanyEditor.vue';
import _ from 'lodash';
import { DateTime } from 'luxon';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { Company, ICompany } from '@/Data/CRM/Company/Company';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		CompanyEditor,
	},
})
export default class CompanyDisplay extends ViewBase {
	
	public $refs!: {
		editor: CompanyEditor,
	};
	
	protected loadingData = false;
	
	

	public LoadData(): void {
		
		if (!this.$route.params.id || IsNullOrEmpty(this.$route.params.id)) {
			return;
		}
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				const rtr = Company.FetchForId(this.$route.params.id);
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
			this.$router.push(`/section/companies/`).catch(((e: Error) => { }));// eslint-disable-line
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
				text: 'All Companies',
				disabled: false,
				to: '/section/companies/index',
			},
			{
				text: 'Company',
				disabled: true,
				to: '',
			},
		];
	}
	
	
	
	get Company(): ICompany | null {
		
		const company = Company.ForId(this.$route.params.id);
		if (!company) {
			return null;
		}
		return _.cloneDeep(company) as ICompany;
	}
	
	
	set Company(val: ICompany | null) {
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
		
		const payload: Record<string, ICompany> = {};
		payload[id] = val;
		Company.UpdateIds(payload);
	}
	
	
	
	
	
}
</script>
