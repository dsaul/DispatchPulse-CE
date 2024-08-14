<template>
	
	<div >
	
		<v-app-bar color="#747389" dark fixed app clipped-right >
			<v-progress-linear
				v-if="IsLoadingData"
				:indeterminate="true"
				absolute
				top
				color="white"
			></v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Assignments</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="LoadData()" />

			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn
					dark
					icon
					v-on="on"
					>
						<v-icon>more_vert</v-icon>
					</v-btn>
				</template>

				<v-list dense>
					<v-list-item
						@click="DoPrint()"
						:disabled="connectionStatus != 'Connected' || !PermCRMReportAssignmentPDF()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="CSVDownloadAssignments()"
						:disabled="!PermCRMExportAssignmentCSV()"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list>
			</v-menu>
			
			
			<template v-slot:extension>
				<v-tabs
				v-model="tab"
				
				background-color="transparent"
				align-with-title
				show-arrows
				>
					<v-tabs-slider color="white"></v-tabs-slider>

					<v-tab
						@click="$router.replace({query: { ...$route.query, tab: 'Scheduler'}}).catch(((e) => {}));"
						class="e2e-assignments-index-tab-scheduler"
						>
						Scheduler
					</v-tab>
					<v-tab
						@click="$router.replace({query: { ...$route.query, tab: 'Unassigned'}}).catch(((e) => {}));"
						class="e2e-assignments-index-tab-unassigned"
						>
						Unassigned
					</v-tab>
					<v-tab
						@click="$router.replace({query: { ...$route.query, tab: 'Open'}}).catch(((e) => {}));"
						class="e2e-assignments-index-tab-open"
						>
						Open
					</v-tab>
					<v-tab
						@click="$router.replace({query: { ...$route.query, tab: 'Closed'}}).catch(((e) => {}));"
						class="e2e-assignments-index-tab-closed"
						>
						Closed
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs :items="breadcrumbs" style="background: white; padding-bottom: 5px; padding-top: 15px; z-index:1;">
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>
		
		<v-alert
			v-if="connectionStatus != 'Connected'"
			type="error"
			elevation="2"
			style="margin-top: 10px; margin-left: 15px; margin-right: 15px;"
			>
			Disconnected from server.
		</v-alert>
			
		<v-tabs-items v-model="tab" style="background: transparent;">
			<v-tab-item style="flex: 1;display:flex; flex-direction: column;">
				<Scheduler 
					v-if="PermAssignmentCanRequest()"
					ref="scheduler"
					:date="schedulerDate"
					@OnDateChanged="OnSchedulerDateChanged"
					/>
			</v-tab-item>
			<v-tab-item style="flex: 1;">
				<AssignmentsList
					ref="unassignedList"
					:showOnlyUnassigned="true"
					:focusIsProject="true"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
			<v-tab-item style="flex: 1;">
				<AssignmentsList
					ref="openList"
					:showOnlyOpenAssignments="true"
					:focusIsProject="true"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
			<v-tab-item style="flex: 1;">
				<AssignmentsList
					ref="closedList"
					:showOnlyClosedAssignments="true"
					:focusIsProject="true"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
		</v-tabs-items>
		<AddAssignmentDialogue2 
			v-model="addAssignmentModel"
			:isOpen="addAssignmentOpen"
			@Save="SaveAddAssignment"
			@Cancel="CancelAddAssignment"
			ref="addAssignmentDialogue"
			/>
		<v-footer color="#747389" class="white--text" app inset>
			<v-row
			no-gutters
			>
				<v-btn
					color="white"
					text
					rounded
					@click="OnClickNewCall()"
					:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush() || !PermContactsCanPush() || !PermAssignmentCanPush()"
					>
					<v-icon left dark>call</v-icon>
					New Call
				</v-btn>
				
				
				<v-spacer />
				
				<AddMenuButton
					:disabled="connectionStatus != 'Connected'"
					>
					<v-list-item
						@click="OpenAddAssignment()"
						class="e2e-add-assignment-menu-item"
						:disabled="connectionStatus != 'Connected' || !PermAssignmentCanPush()"
						>
						<v-list-item-icon>
							<v-icon>local_shipping</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Assignment</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>
			</v-row>
		</v-footer>
	</div>
</template>

<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import { DateTime } from 'luxon';
import AssignmentsList from '@/Components/Lists/AssignmentsList.vue';
import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import Scheduler from '@/Components/Scheduler/Scheduler.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import CSVDownloadAssignments from '@/Data/CRM/Assignment/CSVDownloadAssignments';
import ViewBase from '@/Components/Views/ViewBase';
import AddAssignmentDialogue2 from '@/Components/Dialogues2/Assignments/AddAssignmentDialogue2.vue';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { Project } from '@/Data/CRM/Project/Project';
import { Contact } from '@/Data/CRM/Contact/Contact';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		AssignmentsList,
		AddMenuButton,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		Scheduler,
		ReloadButton,
		AddAssignmentDialogue2,
		NotificationBellButton,
	},
})
export default class AsssignmentsIndex extends ViewBase {
	
	public $refs!: {
		scheduler: Scheduler,
		unassignedList: AssignmentsList,
		openList: AssignmentsList,
		closedList: AssignmentsList,
		addAssignmentDialogue: AddAssignmentDialogue2,
	};
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		'Scheduler': 0,
		'scheduler': 0,
		'Unassigned': 1,
		'unassigned': 1,
		'Open & Assigned': 2,
		'open & assigned': 2,
		'Open': 2,
		'open': 2,
		'Closed': 3,
		'closed': 3,
	};

	
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'All Assignments',
			disabled: true,
			to: '/section/assignments/index',
		},
	];

	protected CSVDownloadAssignments = CSVDownloadAssignments;
	protected DialoguesOpen = Dialogues.Open;
	protected PermAssignmentCanRequest = Assignment.PermAssignmentCanRequest;
	protected PermProjectsCanPush = Project.PermProjectsCanPush;
	protected PermContactsCanPush = Contact.PermContactsCanPush;
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	protected PermCRMReportAssignmentPDF = Assignment.PermCRMReportAssignmentPDF;
	protected PermCRMExportAssignmentCSV = Assignment.PermCRMExportAssignmentCSV;
	
	protected schedulerDate: string = DateTime.local().toFormat('yyyy-MM-dd');
	
	protected loadingData = false;
	
	protected addAssignmentModel: IAssignment | null = null;
	protected addAssignmentOpen = false;
	
	
	public get IsLoadingData(): boolean {
		
		if (this.$refs.scheduler && this.$refs.scheduler.IsLoadingData) {
			return true;
		}
		if (this.$refs.unassignedList && this.$refs.unassignedList.IsLoadingData) {
			return true;
		}
		if (this.$refs.openList && this.$refs.openList.IsLoadingData) {
			return true;
		}
		if (this.$refs.closedList && this.$refs.closedList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.scheduler) {
			this.$refs.scheduler.LoadData();
		}
		if (this.$refs.unassignedList) {
			this.$refs.unassignedList.LoadData();
		}
		if (this.$refs.openList) {
			this.$refs.openList.LoadData();
		}
		if (this.$refs.closedList) {
			this.$refs.closedList.LoadData();
		}
		
	}
	
	
	public LoadData(): void {
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				this.loadingData = true;
			
				setTimeout(() => {
					this.loadingData = false;
				}, 250);
				
			});
		});
		
	}
	
	
	
	
	
	protected MountedAfter(): void {
		this.SwitchToTabFromRoute();
		
		// Import the date from the query string.
		if (IsNullOrEmpty(this.$route.query.schedulerDate as string | null)) {
			this.schedulerDate = DateTime.local().toFormat('yyyy-MM-dd');
		} else {
			this.schedulerDate = this.$route.query.schedulerDate as string;
		}

		this.LoadData();
	}
	
	protected SwitchToTabFromRoute(): void {
		// Select the tab in the query string.
		if (IsNullOrEmpty(this.$route.query.tab as string | null)) {
			this.tab = 0;
		} else {
			const index = this.tabNameToIndex[this.$route.query.tab as string];
			this.tab = index;
		}
	}
	
	protected OnClickNewCall(): void {
		console.log('OnClickNewCall()');
		
		Dialogues.Open({ 
			name: 'NewCallDialogue', 
			state: {},
		});
	}
	
	protected DoPrint(): void {
		
		Dialogues.Open({ 
			name: 'AssignmentReportDialogue', 
			state: {
				allLoadedProjects: false,
			},
		});
		
	}
	
	protected OnSchedulerDateChanged(newDate: string): void {
		this.schedulerDate = newDate;
		
		
		
		
		this.$router.replace({query: { ...this.$route.query, schedulerDate: newDate}}).catch((() => {
			//
		}));
	}
	
	protected OpenAddAssignment(): void {
		
		//console.debug('OpenAddAssignment');
		
		this.addAssignmentModel = Assignment.GetEmpty();
		this.addAssignmentOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.addAssignmentDialogue) {
				this.$refs.addAssignmentDialogue.SwitchToTabFromRoute();
			}
		});
	}
	
	protected CancelAddAssignment(): void {
		
		//console.debug('CancelAddAssignment');
		
		this.addAssignmentOpen = false;
		
	}
	
	protected SaveAddAssignment(): void {
		
		//console.debug('SaveAddAssignment');
		
		this.addAssignmentOpen = false;
		
		if (!this.addAssignmentModel || !this.addAssignmentModel.id || IsNullOrEmpty(this.addAssignmentModel.id)) {
			console.error('!this.addAssignmentModel || !this.addAssignmentModel.id ' + 
				'|| IsNullOrEmpty(this.addAssignmentModel.id)');
			return;
		}
		
		const payload: Record<string, IAssignment> = {};
		payload[this.addAssignmentModel.id] = this.addAssignmentModel;
		Assignment.UpdateIds(payload);
		
		this.$router.push(`/section/assignments/${this.addAssignmentModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		
	}
	
	
}
</script>

<style scoped>



	
</style>