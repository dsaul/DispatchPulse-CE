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
			
			<v-toolbar-title class="white--text">On-Call Dashboard</v-toolbar-title>

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
						class="e2e-dashboard-tab-agent"
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
				
				
				<v-container>
					<v-row dense>
						<v-col cols="12" md="4">
							<v-card>
								<v-card-title class="text-h5">
									Voicemails
								</v-card-title>
								<v-card-subtitle>
									Access all current voicemails.
								</v-card-subtitle>
								<v-card-actions>
									<v-btn
										to="/section/voicemails"
										text
										>
										Open Voicemails
									</v-btn>
								</v-card-actions>
							</v-card>
						</v-col>
						<v-col cols="12" md="4">
							<v-card>
								<v-card-title class="text-h5">
									Phone Numbers
								</v-card-title>
								<v-card-subtitle>
									View your phone numbers.
								</v-card-subtitle>
								<v-card-actions>
									<v-btn
										to="/section/dids"
										text
										>
										Open Phone Numbers
									</v-btn>
								</v-card-actions>
							</v-card>
						</v-col>
						<v-col cols="12" md="4">
							<v-card>
								<v-card-title class="text-h5">
									On-Call Responders
								</v-card-title>
								<v-card-subtitle>
									You on-call menus.
								</v-card-subtitle>
								<v-card-actions>
									<v-btn
										text
										to="/section/on-call"
										>
										Open Responders
									</v-btn>
								</v-card-actions>
							</v-card>
						</v-col>
						<v-col cols="12" md="4">
							<v-card>
								<v-card-title class="text-h5">
									Calendars
								</v-card-title>
								<v-card-subtitle>
									Manage who is responsible, where to contact them, and when.
								</v-card-subtitle>
								<v-card-actions>
									<v-btn 
										to="/section/calendars"
										text>
										Open Calendars
									</v-btn>
								</v-card-actions>
							</v-card>
						</v-col>
						<v-col cols="12" md="4">
							<v-card>
								<v-card-title class="text-h5">
									Recordings
								</v-card-title>
								<v-card-subtitle>
									Manage telephone recordings.
								</v-card-subtitle>
								<v-card-actions>
									<v-btn 
										to="/section/recordings"
										text>
										Open Recordings
									</v-btn>
								</v-card-actions>
							</v-card>
						</v-col>
					</v-row>
				</v-container>


			</v-tab-item>
			
			
			
		</v-tabs-items>
		
		
		
		
		<div style="height: 50px;"></div>
		
			
		
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
import { Agent } from '@/Data/CRM/Agent/Agent';
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
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
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
		Agent: 0,
		agent: 0,
		Dispatch: 1,
		dispatch: 1,
		Billing: 2,
		billing: 2,
		Management: 3,
		management: 3,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: true,
			to: '/',
		},
	];
	
	public $refs!: {
		
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
	
	public ReLoadData(): void {
		
		//console.log('ReLoadData', this.$refs);
		
		
		
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
	
	
	
	
	
	
	
	
	
	
	
	
	
	
}
</script>
