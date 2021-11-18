<template>
	<v-app id="inspire">
		<NotificationsSnackbar />
		<Navigation />
		
		<LoginDialogue @OnLogin="OnLogin" />
		
		
		<ModifyLabourDialogue />
		<ModifyMaterialDialogue />
		<ChangePasswordDialogue />
		<LabourDeleteTimerDialogue />
		<CloseAssignmentDialogue />
		<CompleteAssignmentDialogue />
		<ConfirmLogOutDialogue />
		<DeleteAssignmentDialogue />
		<DeleteCompanyDialogue />
		<DeleteLabourDialogue />
		<DeleteMaterialDialogue />
		<DeleteContactDialogue />
		<DeleteProjectDialogue />
		<MergeProjectsDialogue />
		<NewCallDialogue />
		<ModifyContactDialogue />
		<AddProjectDialogue />
		<ModifyCompanyDialogue />
		<GlobalSearchDialogue />
		<DeleteAgentDialogue />
		<AddProjectNoteDialogue />
		<DeleteProjectNoteDialogue />
		<DeleteProductDialogue />
		<ModifyProductDialogue />
		<DeleteAssignmentStatusDialogue />
		<ModifyAssignmentStatusDialogue />
		<DeleteManHourDialogue />
		<ModifyManHourDialogue />
		<DeleteProjectStatusDialogue />
		<ModifyProjectStatusDialogue />
		<ModifyEmploymentStatusDialogue />
		<DeleteEmploymentStatusDialogue />
		<ModifyLabourExceptionDialogue />
		<DeleteLabourExceptionDialogue />
		<ModifyLabourHolidayDialogue />
		<DeleteLabourHolidayDialogue />
		<ModifyLabourNonBillableDialogue />
		<DeleteLabourNonBillableDialogue />
		<AssignmentReportDialogue />
		<CompanyReportDialogue />
		<ContactsReportDialogue />
		<LabourReportDialogue />
		<MaterialsReportDialogue />
		<ProjectReportDialogue />
		<DemoIntroductionDialogue />
		<RegisterDialogue />
		
		<v-main style="display: flex;">
			
			
			
			<router-view
				ref="routerView"
				/>
			
			
			
			
		</v-main>
		
		
		<v-overlay
				:absolute="true"
				:value="initialLoading"
				style="z-index: 11;"
				>
				<v-dialog
					v-model="initialLoading"
					hide-overlay
					persistent
					width="300"
					>
					
					<v-card
					color="primary"
					dark
					style="padding-top: 10px;"
					>
						<v-card-text>
							Waiting for connection&hellip;
							<v-progress-linear
								indeterminate
								color="white"
								class="mb-0"
								>
							</v-progress-linear>
						</v-card-text>
					</v-card>
					
				</v-dialog>
			</v-overlay>
	</v-app>
</template>

<script lang="ts">
	import { Component } from 'vue-property-decorator';
	import ComponentBase from '@/Components/ComponentBase/ComponentBase';
	import LoginDialogue from '@/Components/Dialogues/LoginDialogue.vue';
	import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
	import Dialogues from '@/Utility/Dialogues';
	import Navigation from '@/Components/Navigation/Navigation.vue';
	import '@/Data/CRM/Voicemail/Voicemail';
	
	import AddAgentsDialogue2 from '@/Components/Dialogues2/Agents/AddAgentsDialogue2.vue';
	import AddAssignmentsDialogue2 from '@/Components/Dialogues2/Assignments/AddAssignmentDialogue2.vue';
	import AddCalendarDialogue from '@/Components/Dialogues2/Calendars/AddCalendarDialogue.vue';
	import AddDIDDialogue from '@/Components/Dialogues2/DIDs/AddDIDDialogue.vue';
	import AddOnCallAutoAttendantDialogue from '@/Components/Dialogues2/OnCallAutoAttendants/AddOnCallAutoAttendantDialogue.vue';
	import AddRecordingDialogue from '@/Components/Dialogues2/Recordings/AddRecordingDialogue.vue';
	import DeleteRecordingDialogue from '@/Components/Dialogues2/Recordings/DeleteRecordingDialogue.vue';
	import EditRecordingDialogue from '@/Components/Dialogues2/Recordings/EditRecordingDialogue.vue';
	
	
	
	
	import ModifyLabourDialogue from '@/Components/Dialogues/Labour/ModifyLabourDialogue.vue';
	import ModifyMaterialDialogue from '@/Components/Dialogues/Materials/ModifyMaterialDialogue.vue';
	import ChangePasswordDialogue from '@/Components/Dialogues/ChangePasswordDialogue.vue';
	import LabourDeleteTimerDialogue from '@/Components/Dialogues/Labour/LabourDeleteTimerDialogue.vue';
	import CloseAssignmentDialogue from '@/Components/Dialogues/Assignments/CloseAssignmentDialogue.vue';
	import CompleteAssignmentDialogue from '@/Components/Dialogues/Assignments/CompleteAssignmentDialogue.vue';
	import ConfirmLogOutDialogue from '@/Components/Dialogues/ConfirmLogOutDialogue.vue';
	import DeleteAssignmentDialogue from '@/Components/Dialogues/Assignments/DeleteAssignmentDialogue.vue';
	import DeleteCompanyDialogue from '@/Components/Dialogues/Companies/DeleteCompanyDialogue.vue';
	import DeleteLabourDialogue from '@/Components/Dialogues/Labour/DeleteLabourDialogue.vue';
	import DeleteMaterialDialogue from '@/Components/Dialogues/Materials/DeleteMaterialDialogue.vue';
	import DeleteContactDialogue from '@/Components/Dialogues/Contacts/DeleteContactDialogue.vue';
	import DeleteProjectDialogue from '@/Components/Dialogues/Projects/DeleteProjectDialogue.vue';
	import MergeProjectsDialogue from '@/Components/Dialogues/Projects/MergeProjectsDialogue.vue';
	import NewCallDialogue from '@/Components/Dialogues/NewCallDialogue.vue';
	import ModifyContactDialogue from '@/Components/Dialogues/Contacts/ModifyContactDialogue.vue';
	import AddProjectDialogue from '@/Components/Dialogues/Projects/AddProjectDialogue.vue';
	import ModifyCompanyDialogue from '@/Components/Dialogues/Companies/ModifyCompanyDialogue.vue';
	import GlobalSearchDialogue from '@/Components/Dialogues/GlobalSearchDialogue.vue';
	import DeleteAgentDialogue from '@/Components/Dialogues/Agents/DeleteAgentDialogue.vue';
	import AddProjectNoteDialogue from '@/Components/Dialogues/Projects/AddProjectNoteDialogue.vue';
	import DeleteProjectNoteDialogue from '@/Components/Dialogues/Projects/DeleteProjectNoteDialogue.vue';	
	import DeleteProductDialogue from '@/Components/Dialogues/Products/DeleteProductDialogue.vue';
	import ModifyProductDialogue from '@/Components/Dialogues/Products/ModifyProductDialogue.vue';
	import DeleteAssignmentStatusDialogue from '@/Components/Dialogues/AssignmentStatus/DeleteAssignmentStatusDialogue.vue'; // tslint:disable-line
	import ModifyAssignmentStatusDialogue from '@/Components/Dialogues/AssignmentStatus/ModifyAssignmentStatusDialogue.vue'; // tslint:disable-line
	import SignalRConnection from '@/RPC/SignalRConnection';
	import DeleteManHourDialogue from '@/Components/Dialogues/ManHours/DeleteManHourDialogue.vue';
	import ModifyManHourDialogue from '@/Components/Dialogues/ManHours/ModifyManHourDialogue.vue';
	import DeleteProjectStatusDialogue from '@/Components/Dialogues/ProjectStatus/DeleteProjectStatusDialogue.vue';
	import ModifyProjectStatusDialogue from '@/Components/Dialogues/ProjectStatus/ModifyProjectStatusDialogue.vue';
	import ModifyEmploymentStatusDialogue from '@/Components/Dialogues/EmploymentStatus/ModifyEmploymentStatusDialogue.vue'; // tslint:disable-line
	import DeleteEmploymentStatusDialogue from '@/Components/Dialogues/EmploymentStatus/DeleteEmploymentStatusDialogue.vue'; // tslint:disable-line
	import ModifyLabourExceptionDialogue from '@/Components/Dialogues/LabourException/ModifyLabourExceptionDialogue.vue';
	import DeleteLabourExceptionDialogue from '@/Components/Dialogues/LabourException/DeleteLabourExceptionDialogue.vue';
	import ModifyLabourHolidayDialogue from '@/Components/Dialogues/LabourHoliday/ModifyLabourHolidayDialogue.vue';
	import DeleteLabourHolidayDialogue from '@/Components/Dialogues/LabourHoliday/DeleteLabourHolidayDialogue.vue';
	import ModifyLabourNonBillableDialogue from '@/Components/Dialogues/LabourNonBillable/ModifyLabourNonBillableDialogue.vue';
	import DeleteLabourNonBillableDialogue from '@/Components/Dialogues/LabourNonBillable/DeleteLabourNonBillableDialogue.vue';
	import AssignmentReportDialogue from '@/Components/Dialogues/Reports/AssignmentReportDialogue.vue';
	import CompanyReportDialogue from '@/Components/Dialogues/Reports/CompanyReportDialogue.vue';
	import ContactsReportDialogue from '@/Components/Dialogues/Reports/ContactsReportDialogue.vue';
	import LabourReportDialogue from '@/Components/Dialogues/Reports/LabourReportDialogue.vue';
	import MaterialsReportDialogue from '@/Components/Dialogues/Reports/MaterialsReportDialogue.vue';
	import ProjectReportDialogue from '@/Components/Dialogues/Reports/ProjectReportDialogue.vue';
	import DemoIntroductionDialogue from '@/Components/Dialogues/Demo/DemoIntroductionDialogue.vue';
	import RegisterDialogue from '@/Components/Dialogues/Register/RegisterDialogue.vue';
	import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
	import PerformLocalLogout from '@/Utility/PerformLocalLogout';
	import { Agent } from '@/Data/CRM/Agent/Agent';
	import NotificationsSnackbar from '@/Components/Alerts/NotificationsSnackbar.vue';
	import { BillingPermissionsBool } from './Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
	import { BillingPermissionsGroupsMemberships } from './Data/Billing/BillingPermissionsGroupsMemberships/BillingPermissionsGroupsMemberships';
	import { BillingPermissionsGroups } from './Data/Billing/BillingPermissionsGroups/BillingPermissionsGroups';
	import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
	import OnCallResponder30DayReportDialogue from '@/Components/Dialogues2/Reports/OnCallResponder30DayReportDialogue.vue';
	
	@Component({
		components: {
			NotificationsSnackbar,
			Navigation,
			LoginDialogue,
			
			// Dialogues must be here or they'll have spooky issues.
			AddAgentsDialogue2,
			AddAssignmentsDialogue2,
			AddCalendarDialogue,
			AddDIDDialogue,
			AddOnCallAutoAttendantDialogue,
			AddRecordingDialogue,
			DeleteRecordingDialogue,
			EditRecordingDialogue,
			
			// Dialogue 1
			ModifyLabourDialogue,
			ModifyMaterialDialogue,
			ChangePasswordDialogue,
			LabourDeleteTimerDialogue,
			CloseAssignmentDialogue,
			CompleteAssignmentDialogue,
			ConfirmLogOutDialogue,
			DeleteAssignmentDialogue,
			DeleteCompanyDialogue,
			DeleteLabourDialogue,
			DeleteMaterialDialogue,
			DeleteContactDialogue,
			DeleteProjectDialogue,
			MergeProjectsDialogue,
			NewCallDialogue,
			ModifyContactDialogue,
			AddProjectDialogue,
			ModifyCompanyDialogue,
			GlobalSearchDialogue,
			DeleteAgentDialogue,
			AddProjectNoteDialogue,
			DeleteProjectNoteDialogue,
			DeleteProductDialogue,
			ModifyProductDialogue,
			DeleteAssignmentStatusDialogue,
			ModifyAssignmentStatusDialogue,
			DeleteManHourDialogue,
			ModifyManHourDialogue,
			DeleteProjectStatusDialogue,
			ModifyProjectStatusDialogue,
			ModifyEmploymentStatusDialogue,
			DeleteEmploymentStatusDialogue,
			ModifyLabourExceptionDialogue,
			DeleteLabourExceptionDialogue,
			ModifyLabourHolidayDialogue,
			DeleteLabourHolidayDialogue,
			ModifyLabourNonBillableDialogue,
			DeleteLabourNonBillableDialogue,
			AssignmentReportDialogue,
			CompanyReportDialogue,
			ContactsReportDialogue,
			LabourReportDialogue,
			MaterialsReportDialogue,
			ProjectReportDialogue,
			DemoIntroductionDialogue,
			RegisterDialogue,
			OnCallResponder30DayReportDialogue,
		},
	})
	export default class App extends ComponentBase {
		
		public $refs!: {
			routerView: any,
		};
		
		
		
		
		protected initialLoading = true;
		
		
		
		
		
		
		
		public created(): void {
			
			
			
			(window as any).RocketChat(function(this: { hideWidget(): void; }) {
				this.hideWidget();
				document.querySelector('.rocketchat-widget')?.classList.add('rocketchat-widget-hidden');
			});
			
			const sessionUuid = localStorage.getItem('SessionUUID');
			
			SignalRConnection.Ready(() => {
				this.initialLoading = false;
			});
			
			if (!IsNullOrEmpty(sessionUuid)) {
				console.log(`Not showing login as we have ${sessionUuid} in local storage.`);
				
				this.$store.commit('SetSession', sessionUuid);
				
				SignalRConnection.Ready(() => {
					
					
					
					//SignalRConnection.RequestDatabase();
					//SignalRConnection.RequestGeneral();
					
					const rtrBillingContacts = BillingContacts.RequestBillingContactsForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtrBillingContacts.outboundRequestPromise) {
						rtrBillingContacts.outboundRequestPromise.catch((reason: Error) => {
							console.error('Error getting session information, likely remotely logged out, performing local logout.', reason);
							PerformLocalLogout();
						});
						
						if (rtrBillingContacts.completeRequestPromise) {
							rtrBillingContacts.completeRequestPromise.then(() => {
								BillingPermissionsBool.RequestBillingPermissionsBoolForCurrentSession.Send({
									sessionId: BillingSessions.CurrentSessionId(),
								});
								BillingPermissionsGroups.RequestBillingPermissionsGroupsForCurrentSession.Send({
									sessionId: BillingSessions.CurrentSessionId(),
								});
								BillingPermissionsGroupsMemberships.RequestBillingPermissionsGroupsMembershipsForCurrentSession.Send({
									sessionId: BillingSessions.CurrentSessionId(),
								});
								Agent.FetchLoggedInAgent();
							});
							
							
						}
						
					}
					
					
				});
				
				
			}
			
			// Open the demo or register dialogues if query parameter exists.
			
			if (IsNullOrEmpty(sessionUuid) && this.$route.query.action && this.$route.query.action === 'NewAccount') {
				Dialogues.Open({ name: 'RegisterDialogue', state: null});
			} else if (IsNullOrEmpty(sessionUuid) && this.$route.query.action && this.$route.query.action === 'Demo') {
				Dialogues.Open({ name: 'DemoIntroductionDialogue', state: null});
			}
			
			
		}
		
		
		
		private OnLogin() {
			//console.log('OnLogin();', this.$refs.routerView)
			if (this.$refs.routerView.ReloadFromLogin) {
				this.$refs.routerView.ReloadFromLogin();
				
				BillingPermissionsBool.RequestBillingPermissionsBoolForCurrentSession.Send({
					sessionId: BillingSessions.CurrentSessionId(),
				});
				BillingPermissionsGroups.RequestBillingPermissionsGroupsForCurrentSession.Send({
					sessionId: BillingSessions.CurrentSessionId(),
				});
				BillingPermissionsGroupsMemberships.RequestBillingPermissionsGroupsMembershipsForCurrentSession.Send({
					sessionId: BillingSessions.CurrentSessionId(),
				});
				Agent.FetchLoggedInAgent();
			}
		}
		
		
		
		
		
		
	}
</script>
<style>

/* The following is a work around for padding being wrong when the navigation is fixed on the left. */

@media screen and (min-width : 960px)  {
	main.v-content {
		padding-left: 260px !important;
	}
}

/* The following enables having content views that are max height. */

main.v-content > div.v-content__wrap
{
	display: flex;
}
main.v-content > div.v-content__wrap > div
{
	flex: 1;
	display: flex;
	flex-direction: column;
}
main.v-content > div.v-content__wrap > div > div.v-tabs-items {
	flex: 1;
	display: flex;
}

main.v-content > div.v-content__wrap > div > div.v-tabs-items > .v-window__container {
	flex: 1;
	display: flex;
}

nav.v-navigation-drawer .v-list-group--sub-group .v-list-group__header {
	min-height:0px !important;
	padding-left: 38px;
	
}

nav.v-navigation-drawer .v-list-group--sub-group .v-list-group__header .v-list-item__icon {
	margin:5px !important;
}

nav.v-navigation-drawer .v-list-group--sub-group .v-list-group__header .v-list-item__title {
	font-size: 0.8125rem;

font-weight: 500;

line-height: 1rem;
}

/*html {
	scrollbar-color: rgba(255,255,255,0.9) rgb(116, 115, 137);
}*/

/* Otherwise the tooltips can get hidden! */
.v-tooltip__content {
	z-index: 15  !important;
}


/* Fade in for 404s */

@keyframes fadeIn404 {
	0% {
		visibility: hidden;
		opacity: 0;
	}
	90% {
		opacity: 0;
		visibility: visible;
	}
	100% {
		opacity: 1;
		visibility: visible;
	}
}e

.fadeIn404 {
	animation: 4s fadeIn404;
	animation-fill-mode: forwards;
	
	visibility: hidden;
	opacity: 0;
}

.rocketchat-widget-hidden {
	pointer-events: none;
}

.rocketchat-widget {
	bottom: 50px !important;
	z-index: 1 !important;
	
}

</style>