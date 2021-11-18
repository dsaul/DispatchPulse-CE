<template>
	<AssignmentEditor
		v-model="Assignment"
		ref="editor"
		:showAppBar="true"
		:showFooter="true"
		:breadcrumbs="Breadcrumbs"
		:preselectTabName="$route.query.tab"
		:isMakingNew="false"
		@reload="LoadData()"
		:isLoadingData="loadingData"
		/>
</template>

<script lang="ts">

import { Component } from 'vue-property-decorator';
import AssignmentEditor from '@/Components/Editors/AssignmentEditor.vue';
import _ from 'lodash';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { DateTime } from 'luxon';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		AssignmentEditor,
	},
})
export default class AsssignmentsDisplay extends ViewBase {
	
	public $refs!: {
		editor: AssignmentEditor,
	};
	
	protected loadingData = false;
	
	

	public LoadData(): void {
		
		if (!this.$route.params.id || IsNullOrEmpty(this.$route.params.id)) {
			return;
		}

		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				const rtr = Assignment.FetchForId(this.$route.params.id);
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
			this.$router.push(`/section/assignments/`).catch(((e: Error) => { }));// eslint-disable-line
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
		
		if (!this.Assignment) {
			return [];
		}
		
		return [
			{
				text: 'Dashboard',
				disabled: false,
				to: '/',
			},
			{
				text: 'All Assignments',
				disabled: false,
				to: '/section/assignments/index',
			},
			{
				text: 'Assignment',
				disabled: true,
				to: '',
			},
		];
	}
	
	
	get Assignment(): IAssignment | null {
		
		const assignment = Assignment.ForId(this.$route.params.id);
		if (!assignment) {
			return null;
		}
		return _.cloneDeep(assignment) as IAssignment;
	}
	
	
	set Assignment(val: IAssignment | null) {
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
		
		const payload: Record<string, IAssignment> = {};
		payload[id] = val;
		Assignment.UpdateIds(payload);
	}
	
	
	
	
	
	
	
	
	
	
	
	
}
</script>
