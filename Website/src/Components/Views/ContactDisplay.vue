<template>
	<ContactEditor
		v-model="Contact"
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
import ContactEditor from '@/Components/Editors/ContactEditor.vue';
import { DateTime } from 'luxon';
import { Contact, IContact } from '@/Data/CRM/Contact/Contact';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		ContactEditor,
	},
})
export default class ContactDisplay extends ViewBase {
	
	public $refs!: {
		editor: ContactEditor,
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
				
				const rtr = Contact.FetchForId(this.$route.params.id);
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
			this.$router.push(`/section/contacts/`).catch(((e: Error) => { }));// eslint-disable-line
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
				text: 'All Contacts',
				disabled: false,
				to: '/section/contacts/index?tab=Contacts',
			},
			{
				text: 'Contact',
				disabled: true,
				to: '',
			},
		];
	}
	
	get Contact(): IContact | null {
		
		//console.debug('this.$route.params.id', this.$route.params.id);
		
		const contact = Contact.ForId(this.$route.params.id);
		
		if (!contact) {
			return null;
		}
		return _.cloneDeep(contact) as IContact;
	}
	
	
	set Contact(val: IContact | null) {
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
		
		const payload: Record<string, IContact> = {};
		payload[id] = val;
		Contact.UpdateIds(payload);
	}

	
	
	
	
	
	
	
}
</script>
