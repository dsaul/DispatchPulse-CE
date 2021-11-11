<template>
	<ProjectEditor
		v-model="Project"
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

import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import ProjectEditor from '@/Components/Editors/ProjectEditor.vue';
import { Component } from 'vue-property-decorator';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({
	components: {
		ProjectEditor,
	},
})
export default class ProjectsDisplay extends ViewBase {
	
	public $refs!: {
		editor: ProjectEditor,
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
				
				const promises: Array<Promise<any>> = [];
				
				
				if (Project.PermProjectsCanRequest()) {
					const rtr = Project.RequestProjects.Send({
						sessionId: BillingSessions.CurrentSessionId(),
						limitToIds: [
							this.$route.params.id,
						],
						showChildrenOfProjectIdAsWell: true,
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (promises.length > 0) {
					
					this.loadingData = true;
					
					Promise.all(promises).finally(() => {
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
			this.$router.push(`/section/projects/`).catch(((e: Error) => { }));// eslint-disable-line
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
				text: 'All Projects',
				disabled: false,
				to: '/section/projects/index',
			},
			{
				text: 'Project Display',
				disabled: true,
				to: '',
			},
		];
	}
	
	
	
	
	get Project(): IProject | null {
		
		const ret = Project.ForId(this.$route.params.id);
		if (!ret) {
			return null;
		}
		
		
		return _.cloneDeep(ret) as IProject;
	}
	
	
	set Project(val: IProject | null) {
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
		
		const payload: Record<string, IProject> = {};
		payload[id] = val;
		Project.UpdateIds(payload);
	}
	
	
	
	
}
</script>
