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
			
			<v-toolbar-title class="white--text">Management Dashboard</v-toolbar-title>

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
					<v-list-item disabled>
						<v-list-item-content>
							<v-list-item-title>No Items</v-list-item-title>
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
						@click="$router.replace({query: { ...$route.query, tab: 'all'}}).catch(((e) => {}));"
						class="e2e-dashboard-tab-management"
						:disabled="!PermCRMViewDashboardManagementTab()"
						>
						All
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs :items="breadcrumbs" style="padding-bottom: 0px; padding-top: 15px;">
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>
		
		<v-alert
			v-if="connectionStatus != 'Connected'"
			type="error"
			elevation="2"
			style="margin-top: 10px; margin-left: 30px; margin-right: 30px;"
			>
			Disconnected from server.
		</v-alert>
		
		<v-tabs-items v-model="tab" style="background: transparent;">
			
			
			
			<v-tab-item style="flex: 1;">
				<div v-if="PermCRMViewDashboardManagementTab()">
					<UpcomingProjectsCard
						ref="upcomingProjectsCard"
						:disabled="connectionStatus != 'Connected'"
						/>
					<PastDueProjectsCard
						ref="pastDueProjectsCard"
						:disabled="connectionStatus != 'Connected'"
						/>
					<DueProjectsWithNoLabourCard
						ref="dueProjectsWithNoLabourCard"
						:disabled="connectionStatus != 'Connected'"
						/>
				</div>
				<PermissionsDeniedAlert v-else />
			</v-tab-item>
			
		</v-tabs-items>
		
		
		
		
		<div style="height: 50px;"></div>
		
		<AddAgentDialogue2
			v-model="addAgentModel"
			:isOpen="addAgentOpen"
			@Save="SaveAddAgent"
			@Cancel="CancelAddAgent"
			ref="addAgentDialogue"
			/>
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
				
				
				<v-spacer />
				
				<AddMenuButton
					class="e2e-add-menu-button"
					:disabled="connectionStatus != 'Connected'"
					>
					
					<v-subheader style="height: 20px; padding-top: 10px;">Address Book…</v-subheader>
					<v-list-item
						@click="DialoguesOpen({ name: 'ModifyContactDialogue', state: null})"
						:disabled="connectionStatus != 'Connected' || !PermAssignmentCanPush()"
						>
						<v-list-item-icon>
							<v-icon>person</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Contact</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="DialoguesOpen({ name: 'ModifyCompanyDialogue', state: null})"
						:disabled="connectionStatus != 'Connected' || !PermCompaniesCanPush()"
						>
						<v-list-item-icon>
							<v-icon>domain</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Company</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					
					<v-divider />
					
					<v-subheader style="height: 20px; padding-top: 10px;">Projects…</v-subheader>
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
					
					<v-list-item
						@click="DialoguesOpen({ name: 'ModifyMaterialDialogue', state: null})"
						:disabled="connectionStatus != 'Connected' || !PermMaterialsCanPush()"
						>
						<v-list-item-icon>
							<v-icon>list</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Material Entry</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					
					<v-divider />
					
					<v-subheader style="height: 20px; padding-top: 10px;">Agents…</v-subheader>
					<v-list-item
						@click="OpenAddAgent()"
						:disabled="connectionStatus != 'Connected' || !PermAgentsCanPush()"
						>
						<v-list-item-icon>
							<v-icon>directions_walk</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Agent</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="DialoguesOpen({
							name: 'ModifyLabourDialogue',
							state: {
								json: {
									agentId: CurrentDPAgentId(),
								}
							}
						})"
						:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries() || !PermLabourCanPush()"
						>
						<v-list-item-icon>
							<v-icon>timelapse</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Labour Entry</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					
					<v-divider />
					
					<v-subheader style="height: 20px; padding-top: 10px;">Assignments…</v-subheader>
					<v-list-item
						@click="OpenAddAssignment()"
						class="e2e-add-menu-item-add-assignment"
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
import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import MyLabourEntriesActiveAndTodayCard from '@/Components/Cards/MyLabourEntriesActiveAndTodayCard.vue';
import MyAgendaTodayCard from '@/Components/Cards/MyAgendaTodayCard.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import ProjectStatusForNewProjects from '@/Utility/DataAccess/ProjectStatusForNewProjects';
import UnassignedAssignmentsCard from '@/Components/Cards/UnassignedAssignmentsCard.vue';
import PastDueAssignmentsCard from '@/Components/Cards/PastDueAssignmentsCard.vue';
import UpcomingProjectsCard from '@/Components/Cards/UpcomingProjectsCard.vue';
import PastDueProjectsCard from '@/Components/Cards/PastDueProjectsCard.vue';
import DueProjectsWithNoLabourCard from '@/Components/Cards/DueProjectsWithNoLabourCard.vue';
import DueAssignmentsWithNoLabourCard from '@/Components/Cards/DueAssignmentsWithNoLabourCard.vue';
import AssignmentsInBillableReviewCard from '@/Components/Cards/AssignmentsInBillableReviewCard.vue';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import AddAssignmentDialogue2 from '@/Components/Dialogues2/Assignments/AddAssignmentDialogue2.vue';
import AddAgentDialogue2 from '@/Components/Dialogues2/Agents/AddAgentsDialogue2.vue';
import CRMViews from '@/Permissions/CRMViews';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { Company } from '@/Data/CRM/Company/Company';
import { Project } from '@/Data/CRM/Project/Project';
import { Material } from '@/Data/CRM/Material/Material';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { GroupFetch } from '@/Data/GroupFetch/GroupFetch';

@Component({
	components: {
		AddMenuButton,
		MyAgendaTodayCard,
		MyLabourEntriesActiveAndTodayCard,
		OpenGlobalSearchButton,
		HelpMenuButton,
		ReloadButton,
		CommitSessionGlobalButton,
		UnassignedAssignmentsCard,
		PastDueAssignmentsCard,
		UpcomingProjectsCard,
		PastDueProjectsCard,
		DueProjectsWithNoLabourCard,
		DueAssignmentsWithNoLabourCard,
		AssignmentsInBillableReviewCard,
		AddAssignmentDialogue2,
		AddAgentDialogue2,
		PermissionsDeniedAlert,
		NotificationBellButton,
	},
})
export default class Dashboard extends ViewBase {
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		All: 0,
		all: 0,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: true,
			to: '/',
		},
	];
	
	public $refs!: {
		upcomingProjectsCard: UpcomingProjectsCard,
		pastDueProjectsCard: PastDueProjectsCard,
		dueProjectsWithNoLabourCard: DueProjectsWithNoLabourCard,
		addAssignmentDialogue: AddAssignmentDialogue2,
		addAgentDialogue: AddAgentDialogue2,
	};
	
	protected DialoguesOpen = Dialogues.Open;
	protected CurrentDPAgentId = Agent.LoggedInAgentId;
	protected PermCompaniesCanPush = Company.PermCompaniesCanPush;
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	protected PermProjectsCanPush = Project.PermProjectsCanPush;
	protected PermMaterialsCanPush = Material.PermMaterialsCanPush;
	protected PermAgentsCanPush = Agent.PermAgentsCanPush;
	protected PermLabourCanPush = Labour.PermLabourCanPush;
	protected PermCRMLabourManualEntries = Labour.PermCRMLabourManualEntries;
	protected PermCRMViewDashboardDispatchTab = CRMViews.PermCRMViewDashboardDispatchTab;
	protected PermCRMViewDashboardBillingTab = CRMViews.PermCRMViewDashboardBillingTab;
	protected PermCRMViewDashboardManagementTab = CRMViews.PermCRMViewDashboardManagementTab;
	
	protected loadingData = false;
	
	protected addAssignmentModel: IAssignment | null = null;
	protected addAssignmentOpen = false;
	
	protected addAgentModel: IAgent | null = null;
	protected addAgentOpen = false;
	
	public ReLoadData(): void {
		
		//console.log('ReLoadData', this.$refs);
		
		if (this.$refs.upcomingProjectsCard) {
			this.$refs.upcomingProjectsCard.LoadData();
		}
		if (this.$refs.pastDueProjectsCard) {
			this.$refs.pastDueProjectsCard.LoadData();
		}
		if (this.$refs.dueProjectsWithNoLabourCard) {
			this.$refs.dueProjectsWithNoLabourCard.LoadData();
		}
		
	}
	
	public LoadData(): void {
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				const session = BillingSessions.CurrentSessionId();
				if (null == session || IsNullOrEmpty(session)) {
					return;
				}
				
				const rtr = GroupFetch.RequestGroupViewDashboard.Send({
					sessionId: session,
				});
				if (rtr.completeRequestPromise) {
					
					this.loadingData = true;
					
					rtr.completeRequestPromise.finally(() => {
						this.loadingData = false;
					});
				}
			});
			
		});
	}
	
	public get IsLoadingData(): boolean {
		
		if (this.$refs.upcomingProjectsCard && this.$refs.upcomingProjectsCard.IsLoadingData) {
			return true;
		}
		if (this.$refs.pastDueProjectsCard && this.$refs.pastDueProjectsCard.IsLoadingData) {
			return true;
		}
		if (this.$refs.dueProjectsWithNoLabourCard && this.$refs.dueProjectsWithNoLabourCard.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	protected MountedAfter(): void {
		this.SwitchToTabFromRoute();
		
		//console.log('mounted');
		
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
	
	protected AddProject(): void {
		
		console.debug('ProjectStatusForNewProjects()', ProjectStatusForNewProjects());
		
		Dialogues.Open({ 
			name: 'AddProjectDialogue', 
			state: {
				json: {
					statusId: ProjectStatusForNewProjects(),
				},
			},
		});
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
		
		if (!this.addAssignmentModel || !this.addAssignmentModel.id) {
			return;
		}
		
		const payload: Record<string, IAssignment> = {};
		payload[this.addAssignmentModel.id] = this.addAssignmentModel;
		Assignment.UpdateIds(payload);
		
		this.$router.push(`/section/assignments/${this.addAssignmentModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		
	}
	
	
	
	
	
	protected OpenAddAgent(): void {
		
		//console.debug('OpenAddAgent');
		
		this.addAgentModel = Agent.GetEmpty();
		this.addAgentOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.addAgentDialogue) {
				this.$refs.addAgentDialogue.SwitchToTabFromRoute();
			}
		});
		
	}
	
	
	
	protected CancelAddAgent(): void {
		
		//console.debug('CancelAddAgent');
		
		this.addAgentOpen = false;
		
	}
	
	protected SaveAddAgent(): void {
		
		//console.debug('SaveAddAgent');
		
		this.addAgentOpen = false;
		
		if (!this.addAgentModel || !this.addAgentModel.id) {
			return;
		}
		
		const payload: Record<string, IAgent> = {};
		payload[this.addAgentModel.id] = this.addAgentModel;
		Agent.UpdateIds(payload);
		
		this.$router.push(`/section/agents/${this.addAgentModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
}
</script>
