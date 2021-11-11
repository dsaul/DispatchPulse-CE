<template>
	
	<div>
	
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
			
			<v-toolbar-title class="white--text">Projects</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="ReLoadData()" />

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
						:disabled="connectionStatus != 'Connected' || !PermCRMReportProjectsPDF()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<!--<v-list-item
						@click="DoExportToCSV()"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>-->
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
						@click="$router.replace({query: { ...$route.query, tab: 'Open'}}).catch(((e) => {}));"
						class="e2e-projects-index-tab-open"
						>
						Open
					</v-tab>
					<v-tab
						@click="$router.replace({query: { ...$route.query, tab: 'Awaiting Payment'}}).catch(((e) => {}));"
						class="e2e-projects-index-tab-awaiting-payment"
						>
						Awaiting Payment
					</v-tab>
					<v-tab
						@click="$router.replace({query: { ...$route.query, tab: 'Closed'}}).catch(((e) => {}));"
						class="e2e-projects-index-tab-closed"
						>
						Closed
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs :items="breadcrumbs" style="background: white; padding-bottom: 5px; padding-top: 15px;">
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
		
		<v-tabs-items v-model="tab" style="background: transparent; flex:1'">
			<v-tab-item style="flex: 1;">
				<ProjectList
					ref="openList"
					:showOpen="true"
					:showAwaitingPayment="false"
					:showClosed="false"
					:isReverseSort="true"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
			<v-tab-item style="flex: 1;">
				<ProjectList
					ref="awaitingPaymentList"
					:showOpen="false"
					:showAwaitingPayment="true"
					:showClosed="false"
					:isReverseSort="true"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
			<v-tab-item style="flex: 1;">
				<ProjectList
					ref="closedList"
					:showOpen="false"
					:showAwaitingPayment="false"
					:showClosed="true"
					:isReverseSort="true"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
		</v-tabs-items>
		
		<div style="height: 50px;"></div>
		
		<v-footer color="#747389" class="white--text" app inset>
			<v-row
			no-gutters
			>
				<v-btn
					color="white"
					text
					rounded
					@click="OnClickNewCall()"
					class="e2e-open-new-call-dialogue"
					:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush() || !PermContactsCanPush() || !PermAssignmentCanPush()"
					>
					<v-icon left dark>call</v-icon>
					New Call
				</v-btn>
				<v-btn
					color="white"
					text
					rounded
					@click="DialoguesOpen({ 
						name: 'MergeProjectsDialogue', 
						state: null
						})"
					class="e2e-open-merge-products-dialogue"
					:disabled="connectionStatus != 'Connected' || !PermCRMViewProjectIndexMergeProjects()"
					>
					<v-icon left>mdi-merge</v-icon>
					Merge Projects
				</v-btn>
				
				
				<v-spacer />
				
				<AddMenuButton
					:disabled="connectionStatus != 'Connected'"
					>
					<v-list-item
						@click="AddProject()"
						class="e2e-add-menu-item-add-project"
						:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
						>
						<v-list-item-icon>
							<v-icon>assignment</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Project</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>
			</v-row>
		</v-footer>
	</div>
</template>

<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import ProjectList from '@/Components/Lists/ProjectList.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ProjectStatusForNewProjects from '@/Utility/DataAccess/ProjectStatusForNewProjects';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import CRMViews from '@/Permissions/CRMViews';
import { Project } from '@/Data/CRM/Project/Project';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		ProjectList,
		AddMenuButton,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
	},
})
export default class ProjectsIndex extends ViewBase {
	
	public $refs!: {
		openList: ProjectList,
		awaitingPaymentList: ProjectList,
		closedList: ProjectList,
	};
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		'Open': 0,
		'open': 0,
		'Awaiting Payment': 1,
		'awaiting payment': 1,
		'Closed': 2,
		'closed': 2,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'All Projects',
			disabled: true,
			to: '/section/projects/index',
		},
	];
	
	protected PermProjectsCanPush = Project.PermProjectsCanPush;
	protected PermContactsCanPush = Contact.PermContactsCanPush;
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	protected PermCRMViewProjectIndexMergeProjects = CRMViews.PermCRMViewProjectIndexMergeProjects;
	protected PermCRMReportProjectsPDF = Project.PermCRMReportProjectsPDF;
	protected DialoguesOpen = Dialogues.Open;
	
	protected loadingData = false;
	
	
	
	public get IsLoadingData(): boolean {
		
		if (this.$refs.openList && this.$refs.openList.IsLoadingData) {
			return true;
		}
		if (this.$refs.awaitingPaymentList && this.$refs.awaitingPaymentList.IsLoadingData) {
			return true;
		}
		if (this.$refs.closedList && this.$refs.closedList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.openList) {
			this.$refs.openList.LoadData();
		}
		if (this.$refs.awaitingPaymentList) {
			this.$refs.awaitingPaymentList.LoadData();
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
			name: 'ProjectReportDialogue', 
			state: {
				allLoadedProjects: false,
			},
		});
		
	}
	
	
	
	protected AddProject(): void {
		
		
		
		Dialogues.Open({ 
			name: 'AddProjectDialogue', 
			state: {
				json: {
					statusId: ProjectStatusForNewProjects(),
				},
			},
		});
	}
	
}
</script>
<style>

</style>