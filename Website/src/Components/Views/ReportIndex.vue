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
			
			<v-toolbar-title class="white--text">All Reports</v-toolbar-title>

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
					<!--<v-list-item
						@click="DoPrint()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>-->
					<!--<v-list-item
						@click="ExportToCSV()"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>-->
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

					<v-tab @click="$router.replace({query: { ...$route.query, tab: 'All'}}).catch(((e) => {}));">
						All
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
		
		<v-tabs-items v-model="tab" style="background: transparent;">
			<v-tab-item style="flex: 1;">
				
				<v-container>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
				
						<v-list>
							<v-list-item-group color="primary">
								
								<v-list-item
									v-if="cacheLicensedProjectsSchedulingTime"
									@click="OpenDialogueAssignmentReport()"
									two-line
									class="e2e-report-index-assignment-report"
									:disabled="connectionStatus != 'Connected' || !PermCRMReportAssignmentPDF()"
									>
									<v-list-item-content>
										<v-list-item-title>Assignment Report</v-list-item-title>
										<v-list-item-subtitle>Prints out service order style print out of assignments.</v-list-item-subtitle>
									</v-list-item-content>
								</v-list-item>
								
								<v-list-item
									v-if="cacheLicensedProjectsSchedulingTime"
									@click="OpenDialogueCompanyReport()"
									two-line
									class="e2e-report-index-company-report"
									:disabled="connectionStatus != 'Connected' || !PermCRMReportCompaniesPDF()"
									>
									<v-list-item-content>
										<v-list-item-title>Company Report</v-list-item-title>
										<v-list-item-subtitle>Prints out a company list.</v-list-item-subtitle>
									</v-list-item-content>
								</v-list-item>
								
								<v-list-item
									v-if="cacheLicensedProjectsSchedulingTime"
									@click="OpenDialogueContactsReport()"
									two-line
									class="e2e-report-index-contacts-report"
									:disabled="connectionStatus != 'Connected' || !PermCRMReportContactsPDF()"
									>
									<v-list-item-content>
										<v-list-item-title>Contacts Report</v-list-item-title>
										<v-list-item-subtitle>Creates an Address Book printout for contacts.</v-list-item-subtitle>
									</v-list-item-content>
								</v-list-item>
								
								<v-list-item
									v-if="cacheLicensedProjectsSchedulingTime"
									@click="OpenDialogueLabourReport()"
									two-line
									class="e2e-report-index-labour-report"
									:disabled="connectionStatus != 'Connected' || !PermCRMReportLabourPDF()"
									>
									<v-list-item-content>
										<v-list-item-title>Labour Report</v-list-item-title>
										<v-list-item-subtitle>Prints out a list of labour.</v-list-item-subtitle>
									</v-list-item-content>
								</v-list-item>
								
								<v-list-item
									v-if="cacheLicensedProjectsSchedulingTime"
									@click="OpenDialogueMaterialsReport()"
									:disabled="connectionStatus != 'Connected' || !PermCRMReportMaterialsPDF()"
									two-line
									class="e2e-report-index-materials-report"
									>
									<v-list-item-content>
										<v-list-item-title>Materials Report</v-list-item-title>
										<v-list-item-subtitle>Prints out a list of materials.</v-list-item-subtitle>
									</v-list-item-content>
								</v-list-item>
								
								<v-list-item
									v-if="cacheLicensedProjectsSchedulingTime"
									@click="OpenDialogueProjectReport()"
									two-line
									class="e2e-report-index-project-report"
									:disabled="connectionStatus != 'Connected' || !PermCRMReportProjectsPDF()"
									>
									<v-list-item-content>
										<v-list-item-title>Project Report</v-list-item-title>
										<v-list-item-subtitle>Creates a print out record of all the relevant information from a project.</v-list-item-subtitle>
									</v-list-item-content>
								</v-list-item>

								<v-list-item
									v-if="cacheLicensedOnCall"
									@click="OpenOnCallResponder30DayReport()"
									two-line
									class="e2e-report-index-project-report"
									:disabled="connectionStatus != 'Connected' || !PermCRMReportProjectsPDF()"
									>
									<v-list-item-content>
										<v-list-item-title>On Call Responder Last 30 Days Overview</v-list-item-title>
										<v-list-item-subtitle>Creates a print out report detailing the events of the past month.</v-list-item-subtitle>
									</v-list-item-content>
								</v-list-item>
								
							</v-list-item-group>
						</v-list>
						
					</v-col>
				</v-row>
				</v-container>
				
				
			</v-tab-item>
		</v-tabs-items>
		
		<OnCallResponder30DayReportDialogue 
			v-model="ocr30DayReportModel"
			:isOpen="ocr30DayReportOpen"
			@cancel="CancelOnCallResponder30DayReport"
			@start-over="StartOverOnCallResponder30DayReport"
			ref="ocr30DayReportDialogue"
			/>
		
		
		<div style="height: 50px;"></div>
		
		<!-- <v-footer color="#747389" class="white--text" app inset>
			<v-row
				no-gutters
				>
				<v-spacer />
				
				<AddMenuButton>
					<v-list-item
						@click="AddQuote()"
						>
						<v-list-item-icon>
							<v-icon>attach_money</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Quote</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>
				
			</v-row>
		</v-footer> -->
		
	</div>
</template>

<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import { Company } from '@/Data/CRM/Company/Company';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { Material } from '@/Data/CRM/Material/Material';
import { Project } from '@/Data/CRM/Project/Project';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import CRMNavigation from '@/Permissions/CRMNavigation';
import OnCallResponder30DayReportDialogue from '@/Components/Dialogues2/Reports/OnCallResponder30DayReportDialogue.vue';
import { IOnCallResponder30DayReportModelState, OnCallResponder30DayReportModelState } from '@/Data/Models/OnCallResponder30DayReportModelState/OnCallResponder30DayReportModelState';

@Component({
	components: {
		AddMenuButton,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
		OnCallResponder30DayReportDialogue,
	},
})
export default class ReportsIndex extends ViewBase {
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		All: 0,
		all: 0,
	};
	
	public $refs!: {
		ocr30DayReportDialogue: OnCallResponder30DayReportDialogue,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'All Reports',
			disabled: true,
			to: '/section/reports/index',
		},
	];
	
	protected ocr30DayReportModel: IOnCallResponder30DayReportModelState | null = null;
	protected ocr30DayReportOpen = false;
	
	protected PermCRMReportCompaniesPDF = Company.PermCRMReportCompaniesPDF;
	protected PermCRMReportAssignmentPDF = Assignment.PermCRMReportAssignmentPDF;
	protected PermCRMReportContactsPDF = Contact.PermCRMReportContactsPDF;
	protected PermCRMReportLabourPDF = Labour.PermCRMReportLabourPDF;
	protected PermCRMReportMaterialsPDF = Material.PermCRMReportMaterialsPDF;
	protected PermCRMReportProjectsPDF = Project.PermCRMReportProjectsPDF;
	
	
	protected cacheLicensedProjectsSchedulingTime = false;
	protected cacheLicensedOnCall = false;

	protected _periodicInterval: ReturnType<typeof setTimeout> | null = null;

	protected loadingData = false;

	public get IsLoadingData(): boolean {
		
		// if (this.$refs.openList && this.$refs.openList.IsLoadingData) {
		// 	return true;
		// }
		// if (this.$refs.awaitingPaymentList && this.$refs.awaitingPaymentList.IsLoadingData) {
		// 	return true;
		// }
		// if (this.$refs.closedList && this.$refs.closedList.IsLoadingData) {
		// 	return true;
		// }
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		// if (this.$refs.openList) {
		// 	this.$refs.openList.LoadData();
		// }
		// if (this.$refs.awaitingPaymentList) {
		// 	this.$refs.awaitingPaymentList.LoadData();
		// }
		// if (this.$refs.closedList) {
		// 	this.$refs.closedList.LoadData();
		// }
		
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
		this._periodicInterval = setInterval(this.Periodic, 500);
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

	protected Periodic(): void {
		this.cacheLicensedProjectsSchedulingTime = CRMNavigation.PermCRMNavigationLicensedProjectsSchedulingTime();
		this.cacheLicensedOnCall = CRMNavigation.PermCRMNavigationLicensedOnCall();
	}
	
	
	
	protected OpenDialogueAssignmentReport(): void {
		
		Dialogues.Open({ name: 'AssignmentReportDialogue', state: null});
		
	}
	
	protected OpenDialogueCompanyReport(): void {
		
		Dialogues.Open({ name: 'CompanyReportDialogue', state: null});
		
	}
	
	protected OpenDialogueContactsReport(): void {
		
		Dialogues.Open({ name: 'ContactsReportDialogue', state: null});
		
	}
	
	protected OpenDialogueLabourReport(): void {
		
		Dialogues.Open({ name: 'LabourReportDialogue', state: null});
		
	}
	
	protected OpenDialogueMaterialsReport(): void {
		
		Dialogues.Open({ name: 'MaterialsReportDialogue', state: null});
		
	}
	
	protected OpenDialogueProjectReport(): void {
		
		Dialogues.Open({ name: 'ProjectReportDialogue', state: null});
		
	}
	
	
	
	
	
	
	protected OpenOnCallResponder30DayReport(): void {
		
		//console.debug('OpenAddCalendars');
		
		this.ocr30DayReportModel = OnCallResponder30DayReportModelState.GetEmpty();
		this.ocr30DayReportOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.ocr30DayReportDialogue) {
				this.$refs.ocr30DayReportDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
	}
	
	protected CancelOnCallResponder30DayReport(): void {
		
		//console.debug('CancelAddRecording');
		this.ocr30DayReportModel = OnCallResponder30DayReportModelState.GetEmpty();
		this.ocr30DayReportOpen = false;
		
	}
	
	protected StartOverOnCallResponder30DayReport(): void {
		this.ocr30DayReportModel = OnCallResponder30DayReportModelState.GetEmpty();
	}
	
	
}
</script>
