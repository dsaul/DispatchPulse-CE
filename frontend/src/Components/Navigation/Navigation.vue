<template>
	<v-navigation-drawer width="260px" v-model="$store.state.drawers.showNavigation" fixed app style="z-index: 10"
		mobile-breakpoint="960">
		<v-img src="/env/NAV_PICTURE_URI" style="max-height: 185px;" dark>


			<v-avatar size="64px"
				style="margin-left: 32px; margin-top: 32px; filter: drop-shadow(2px 2px 2px rgba(0, 0, 0, 1));"
				color="indigo">
				<v-icon dark>account_circle</v-icon>
			</v-avatar>



			<v-spacer></v-spacer>

			<div
				style="text-shadow: 2px 2px 2px rgba(0, 0, 0, 1); padding-left: 32px;margin-top: 20px; padding-top: 5px; color: white; font-size: 18px; font-weight: bold; background: linear-gradient(to bottom, rgba(0,0,0,0) 0%,rgba(0,0,0,0.4) 10%,rgba(0,0,0,0.4) 100%); min-height:32px;">
				{{ CurrentBillingContactName() }}</div>
			<div
				style="text-shadow: 2px 2px 2px rgba(0, 0, 0, 1); padding-left: 32px; padding-top: 5px; padding-bottom: 10px; background: rgba(0,0,0,0.4); color: white; min-height: 39px;">
				{{ CurrentBillingContactEmail() }}</div>

		</v-img>
		<div v-if="GetDemoMode()"
			style="color: white; background: #ffc107; padding: 3px;padding-left: 72px;padding-right: 15px;">
			Demo Mode
		</div>

		<v-list expand>


			<v-list-group prepend-icon="dashboard" v-model="sideNavExpandDashboard">
				<template v-slot:activator>
					<v-list-item-title>Dashboard</v-list-item-title>
				</template>

				<v-list-item v-if="cacheLicensedProjectsSchedulingTime" to="/section/dashboard/agent"
					class="e2e-sidenav-dashboard" style="min-height: 25px; padding-left:72px;" dense>
					<v-list-item-content>
						<v-list-item-title>Agent Dashboard</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item v-if="cacheLicensedProjectsSchedulingTime" to="/section/dashboard/dispatch"
					class="e2e-sidenav-dashboard" style="min-height: 25px; padding-left:72px;" dense>
					<v-list-item-content>
						<v-list-item-title>Dispatch Dashboard</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item v-if="cacheLicensedProjectsSchedulingTime" to="/section/dashboard/billing"
					class="e2e-sidenav-dashboard" style="min-height: 25px; padding-left:72px;" dense>
					<v-list-item-content>
						<v-list-item-title>Billing Dashboard</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item v-if="cacheLicensedProjectsSchedulingTime" to="/section/dashboard/management"
					class="e2e-sidenav-dashboard" style="min-height: 25px; padding-left:72px;" dense>
					<v-list-item-content>
						<v-list-item-title>Management Dashboard</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item v-if="cacheLicensedOnCall" to="/section/dashboard/on-call" class="e2e-sidenav-dashboard"
					style="min-height: 25px; padding-left:72px;" dense>
					<v-list-item-content>
						<v-list-item-title>On-Call Dashboard</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</v-list-group>

			<v-divider />

			<div v-if="!cacheAnyPermissions"
				style="padding: 5px; padding-left: 20px; padding-right: 20px; text-align: center;">
				<!-- No Permissions -->
				<!-- <v-progress-circular
					indeterminate
					color="primary"
					>
				</v-progress-circular> -->
			</div>






			<v-list-group v-if="cachePermCRMNavigationShowAddressBook" prepend-icon="contacts"
				v-model="sideNavExpandAddressBook">
				<template v-slot:activator>
					<v-list-item-title>Address Book</v-list-item-title>
				</template>

				<v-list-item v-if="cachePermCRMNavigationShowAllContacts" dense to="/section/contacts/index"
					style="min-height: 25px; padding-left:72px;" class="e2e-sidenav-all-contacts">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>All Contacts</v-list-item-title>
					</v-list-item-content>
				</v-list-item>

				<v-list-item v-if="cachePermCRMNavigationShowAllCompanies" dense to="/section/companies/index"
					style="min-height: 25px; padding-left:72px;" class="e2e-sidenav-all-companies">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>All Companies</v-list-item-title>
					</v-list-item-content>
				</v-list-item>

			</v-list-group>

			<v-list-group v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowProjects"
				prepend-icon="assignment" v-model="sideNavExpandProjects">
				<template v-slot:activator>
					<v-list-item-title>Projects</v-list-item-title>
				</template>

				<v-list-item v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowAllProjects" dense
					to="/section/projects" style="min-height: 25px; padding-left:72px;"
					class="e2e-sidenav-all-projects">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>All Projects</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowAllAssignments"
					dense to="/section/assignments" style="min-height: 25px; padding-left:72px;"
					class="e2e-sidenav-all-assignments">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>All Assignments</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowAllMaterialEntries"
					dense to="/section/material-entries" style="min-height: 25px; padding-left:72px;"
					class="e2e-sidenav-all-material-entries">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>All Material Entries</v-list-item-title>
					</v-list-item-content>
				</v-list-item>

				<!--<v-list-item 
					dense
					to="/section/quotes"
					style="min-height: 25px; padding-left:72px;"
					>
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>*All Quotes</v-list-item-title>
					</v-list-item-content>
				</v-list-item>-->
				<v-list-group v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowProjectDefinitions"
					dense no-action sub-group v-model="sideNavExpandProjectDefinitions" style="min-height: 0px;"
					class="e2e-sidenav-project-definitons-expander">
					<template v-slot:activator>
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Definitions</v-list-item-title>
						</v-list-item-content>
					</template>

					<v-list-item
						v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowProductsDefinitions"
						dense to="/section/product-definitions" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-product-definitions">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Products</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowAssignmentStatusDefinitions"
						dense to="/section/assignment-status-definitions" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-assignment-status-definitions">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Assignment Status</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowManHoursDefinitions"
						dense to="/section/man-hours-definitions" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-man-hours-definitions">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Man Hours</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowProjectStatusDefinitions"
						dense to="/section/project-status-definitions" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-project-status-definitions">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Project Status</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list-group>
			</v-list-group>





			<v-list-group v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowAgents"
				prepend-icon="directions_walk" v-model="sideNavExpandAgents">
				<template v-slot:activator>
					<v-list-item-title>Agents &amp; Time</v-list-item-title>
				</template>
				<v-list-item v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowAllAgents" dense
					to="/section/agents" style="min-height: 25px; padding-left:72px;" class="e2e-sidenav-all-agents">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>All Agents</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowAllLabourEntries"
					dense to="/section/labour" style="min-height: 25px; padding-left:72px;"
					class="e2e-sidenav-all-labour-entries">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>All Labour/Time Entries</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-group v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowAgentsDefinitions"
					dense no-action sub-group v-model="sideNavExpandAgentDefinitions" style="min-height: 0px;"
					class="e2e-agent-definitons-expansion">
					<template v-slot:activator>
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Definitions</v-list-item-title>
						</v-list-item-content>
					</template>
					<v-list-item
						v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowEmploymentStatusDefinitions"
						dense to="/section/employment-status-definitions" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-employment-status-definitions">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Employment Status</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowLabourExceptionDefinitions"
						dense to="/section/labour-exception-definitions" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-labour-exception-definitions">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Labour Exceptions</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowLabourHolidaysDefinitions"
						dense to="/section/labour-holiday-definitions" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-labour-holidays-definitons">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Labour Holidays</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						v-if="cacheLicensedProjectsSchedulingTime && cachePermCRMNavigationShowLabourNonBillableDefinitions"
						dense to="/section/labour-non-billable-definitions" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-labour-non-billable-definitions">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Labour Non Billable</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list-group>
			</v-list-group>

			<v-divider />

			<v-list-group v-if="cacheLicensedOnCall && cachePermCRMNavigationShowPhoneServices" prepend-icon="phone"
				v-model="sideNavExpandPhoneServices">
				<template v-slot:activator>
					<v-list-item-title>Phone Services</v-list-item-title>
				</template>

				<v-list-item v-if="cacheLicensedOnCall && cachePermCRMNavigationShowPhoneServicesVoicemails" dense
					to="/section/voicemails" style="min-height: 25px; padding-left:72px;"
					class="e2e-sidenav-all-reports">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>Voicemails</v-list-item-title>
					</v-list-item-content>
				</v-list-item>

				<v-list-group v-if="cacheLicensedOnCall && cachePermCRMNavigationShowPhoneServicesSetup" dense no-action
					sub-group v-model="sideNavExpandPhoneServicesSetup" style="min-height: 0px;"
					class="e2e-sidenav-project-definitons-expander">
					<template v-slot:activator>
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Setup</v-list-item-title>
						</v-list-item-content>
					</template>

					<v-list-item v-if="cacheLicensedOnCall && cachePermCRMNavigationShowPhoneServicesPhoneNumbers" dense
						to="/section/dids" style="min-height: 25px; padding-left:72px;" class="e2e-sidenav-all-reports">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Phone Numbers</v-list-item-title>
						</v-list-item-content>
					</v-list-item>

					<v-list-item v-if="cacheLicensedOnCall && cachePermCRMNavigationShowPhoneServicesOnCallResponders"
						dense to="/section/on-call" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-all-reports">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>On Call Responders</v-list-item-title>
						</v-list-item-content>
					</v-list-item>

					<v-list-item v-if="cacheLicensedOnCall && cachePermCRMNavigationShowPhoneServicesRecordings" dense
						to="/section/recordings" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-all-reports">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Recordings</v-list-item-title>
						</v-list-item-content>
					</v-list-item>

					<v-list-item v-if="cacheLicensedOnCall && cachePermCRMNavigationShowPhoneServicesCalendars" dense
						to="/section/calendars" style="min-height: 25px; padding-left:72px;"
						class="e2e-sidenav-all-reports">
						<v-list-item-content style="padding: 0px;">
							<v-list-item-title>Calendars</v-list-item-title>
						</v-list-item-content>
					</v-list-item>

				</v-list-group>

			</v-list-group>

			<v-divider />

			<v-list-group v-if="cachePermCRMNavigationShowReports" prepend-icon="mdi-file-chart"
				v-model="sideNavExpandReports">
				<template v-slot:activator>
					<v-list-item-title>Reports</v-list-item-title>
				</template>
				<v-list-item v-if="cachePermCRMNavigationShowAllReports" dense to="/section/reports"
					style="min-height: 25px; padding-left:72px;" class="e2e-sidenav-all-reports">
					<v-list-item-content style="padding: 0px;">
						<v-list-item-title>All Reports</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</v-list-group>



			<v-divider />


			<v-divider />
			<v-list-item v-if="cachePermCRMNavigationShowSettings" to="/settings" class="e2e-sidenav-settings">
				<v-list-item-action>
					<v-icon>settings</v-icon>
				</v-list-item-action>
				<v-list-item-content>
					<v-list-item-title>Settings and Account</v-list-item-title>
				</v-list-item-content>
			</v-list-item>
			<v-list-item v-if="!GetDemoMode()" :disabled="GetDemoMode()" @click="OpenLogOut()">
				<v-list-item-action>
					<v-icon>exit_to_app</v-icon>
				</v-list-item-action>
				<v-list-item-content>
					<v-list-item-title>Log Out</v-list-item-title>
				</v-list-item-content>
			</v-list-item>
			<v-list-item v-if="GetDemoMode()" @click="ExitDemo()">
				<v-list-item-action>
					<v-icon>exit_to_app</v-icon>
				</v-list-item-action>
				<v-list-item-content>
					<v-list-item-title>Exit Demo</v-list-item-title>
				</v-list-item-content>
			</v-list-item>

			<!-- Work around for iOS Safari's navigation controls going over list items -->
			<v-spacer style="height: 100px;" class="d-flex d-sm-none" />

		</v-list>
	</v-navigation-drawer>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import ComponentBase from '@/Components/ComponentBase/ComponentBase';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import CRMNavigation from '@/Permissions/CRMNavigation';
import Dialogues from '@/Utility/Dialogues';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component(
	{
		components: {
		},
	}
)
export default class Navigation extends ComponentBase {

	protected CurrentBillingContactEmail = BillingContacts.EMailForCurrentSession;
	protected CurrentBillingContactName = BillingContacts.NameForCurrentSession;
	protected GetDemoMode = GetDemoMode;

	protected sideNavExpandDashboard = true;
	protected sideNavExpandAddressBook = true;
	protected sideNavExpandProjects = true;
	protected sideNavExpandProjectDefinitions = false;
	protected sideNavExpandAgents = true;
	protected sideNavExpandAgentDefinitions = false;
	protected sideNavExpandPhoneServices = true;
	protected sideNavExpandPhoneServicesSetup = false;
	protected sideNavExpandReports = true;

	protected _periodicInterval: ReturnType<typeof setTimeout> | null = null;

	protected cacheAnyPermissions = false;
	protected cachePermCRMNavigationShowAddressBook = false;
	protected cachePermCRMNavigationShowAllContacts = false;
	protected cachePermCRMNavigationShowAllCompanies = false;
	protected cachePermCRMNavigationShowProjects = false;
	protected cachePermCRMNavigationShowAllProjects = false;
	protected cachePermCRMNavigationShowAllAssignments = false;
	protected cachePermCRMNavigationShowAllMaterialEntries = false;
	protected cachePermCRMNavigationShowProjectDefinitions = false;
	protected cachePermCRMNavigationShowProductsDefinitions = false;
	protected cachePermCRMNavigationShowAssignmentStatusDefinitions = false;
	protected cachePermCRMNavigationShowManHoursDefinitions = false;
	protected cachePermCRMNavigationShowProjectStatusDefinitions = false;
	protected cachePermCRMNavigationShowAgents = false;
	protected cachePermCRMNavigationShowAllAgents = false;
	protected cachePermCRMNavigationShowAllLabourEntries = false;
	protected cachePermCRMNavigationShowAgentsDefinitions = false;
	protected cachePermCRMNavigationShowEmploymentStatusDefinitions = false;
	protected cachePermCRMNavigationShowLabourExceptionDefinitions = false;
	protected cachePermCRMNavigationShowLabourHolidaysDefinitions = false;
	protected cachePermCRMNavigationShowLabourNonBillableDefinitions = false;
	protected cachePermCRMNavigationShowReports = false;
	protected cachePermCRMNavigationShowAllReports = false;
	protected cachePermCRMNavigationShowSettings = false;

	protected cachePermCRMNavigationShowPhoneServices = false;
	protected cachePermCRMNavigationShowPhoneServicesSetup = false;
	protected cachePermCRMNavigationShowPhoneServicesPhoneNumbers = false;
	protected cachePermCRMNavigationShowPhoneServicesOnCallResponders = false;
	protected cachePermCRMNavigationShowPhoneServicesCalendars = false;
	protected cachePermCRMNavigationShowPhoneServicesVoicemails = false;
	protected cachePermCRMNavigationShowPhoneServicesRecordings = false;



	protected cacheLicensedProjectsSchedulingTime = false;
	protected cacheLicensedOnCall = false;


	public get SideNavExpandDashboard(): boolean {
		return this.sideNavExpandDashboard;
	}

	public set SideNavExpandDashboard(payload: boolean) {
		this.sideNavExpandDashboard = payload;
	}




	public get SideNavExpandAddressBook(): boolean {
		return this.sideNavExpandAddressBook;
	}

	public set SideNavExpandAddressBook(payload: boolean) {
		this.sideNavExpandAddressBook = payload;
	}

	public get SideNavExpandProjects(): boolean {
		return this.sideNavExpandProjects;
	}

	public set SideNavExpandProjects(payload: boolean) {
		this.sideNavExpandProjects = payload;
	}

	public get SideNavExpandProjectDefinitions(): boolean {
		return this.sideNavExpandProjectDefinitions;
	}

	public set SideNavExpandProjectDefinitions(payload: boolean) {
		this.sideNavExpandProjectDefinitions = payload;
	}

	public get SideNavExpandAgents(): boolean {
		return this.sideNavExpandAgents;
	}

	public set SideNavExpandAgents(payload: boolean) {
		this.sideNavExpandAgents = payload;
	}

	public get SideNavExpandAgentDefinitions(): boolean {
		return this.sideNavExpandAgentDefinitions;
	}

	public set SideNavExpandAgentDefinitions(payload: boolean) {
		this.sideNavExpandAgentDefinitions = payload;
	}

	public mounted(): void {
		this._periodicInterval = setInterval(this.Periodic, 1000);
	}

	public destroyed(): void {
		if (this._periodicInterval != null) {
			clearInterval(this._periodicInterval);
		}
	}

	public Periodic(): void {
		this.cacheAnyPermissions = BillingPermissionsBool.HasAnyPermissions();
		this.cachePermCRMNavigationShowAddressBook = CRMNavigation.PermCRMNavigationShowAddressBook();
		this.cachePermCRMNavigationShowAllContacts = CRMNavigation.PermCRMNavigationShowAllContacts();
		this.cachePermCRMNavigationShowAllCompanies = CRMNavigation.PermCRMNavigationShowAllCompanies();
		this.cachePermCRMNavigationShowProjects = CRMNavigation.PermCRMNavigationShowProjects();
		this.cachePermCRMNavigationShowAllProjects = CRMNavigation.PermCRMNavigationShowAllProjects();
		this.cachePermCRMNavigationShowAllAssignments = CRMNavigation.PermCRMNavigationShowAllAssignments();
		this.cachePermCRMNavigationShowAllMaterialEntries = CRMNavigation.PermCRMNavigationShowAllMaterialEntries();
		this.cachePermCRMNavigationShowProjectDefinitions = CRMNavigation.PermCRMNavigationShowProjectDefinitions();
		this.cachePermCRMNavigationShowProductsDefinitions = CRMNavigation.PermCRMNavigationShowProductsDefinitions();
		this.cachePermCRMNavigationShowAssignmentStatusDefinitions =
			CRMNavigation.PermCRMNavigationShowAssignmentStatusDefinitions();
		this.cachePermCRMNavigationShowManHoursDefinitions = CRMNavigation.PermCRMNavigationShowManHoursDefinitions();
		this.cachePermCRMNavigationShowProjectStatusDefinitions =
			CRMNavigation.PermCRMNavigationShowProjectStatusDefinitions();
		this.cachePermCRMNavigationShowAgents = CRMNavigation.PermCRMNavigationShowAgents();
		this.cachePermCRMNavigationShowAllAgents = CRMNavigation.PermCRMNavigationShowAllAgents();
		this.cachePermCRMNavigationShowAllLabourEntries = CRMNavigation.PermCRMNavigationShowAllLabourEntries();
		this.cachePermCRMNavigationShowAgentsDefinitions = CRMNavigation.PermCRMNavigationShowAgentsDefinitions();
		this.cachePermCRMNavigationShowEmploymentStatusDefinitions =
			CRMNavigation.PermCRMNavigationShowEmploymentStatusDefinitions();
		this.cachePermCRMNavigationShowLabourExceptionDefinitions =
			CRMNavigation.PermCRMNavigationShowLabourExceptionDefinitions();
		this.cachePermCRMNavigationShowLabourHolidaysDefinitions =
			CRMNavigation.PermCRMNavigationShowLabourHolidaysDefinitions();
		this.cachePermCRMNavigationShowLabourNonBillableDefinitions =
			CRMNavigation.PermCRMNavigationShowLabourNonBillableDefinitions();
		this.cachePermCRMNavigationShowReports = CRMNavigation.PermCRMNavigationShowReports();
		this.cachePermCRMNavigationShowAllReports = CRMNavigation.PermCRMNavigationShowAllReports();
		this.cachePermCRMNavigationShowSettings = CRMNavigation.PermCRMNavigationShowSettings();

		this.cachePermCRMNavigationShowPhoneServices = CRMNavigation.PermCRMNavigationShowPhoneServices();
		this.cachePermCRMNavigationShowPhoneServicesPhoneNumbers =
			CRMNavigation.PermCRMNavigationShowPhoneServicesPhoneNumbers();
		this.cachePermCRMNavigationShowPhoneServicesSetup =
			CRMNavigation.PermCRMNavigationShowPhoneServicesSetup();
		this.cachePermCRMNavigationShowPhoneServicesOnCallResponders =
			CRMNavigation.PermCRMNavigationShowPhoneServicesOnCallResponders();
		this.cachePermCRMNavigationShowPhoneServicesRecordings =
			CRMNavigation.PermCRMNavigationShowPhoneServicesRecordings();
		this.cachePermCRMNavigationShowPhoneServicesCalendars = CRMNavigation.PermCRMNavigationShowPhoneServicesCalendars();
		this.cachePermCRMNavigationShowPhoneServicesVoicemails = CRMNavigation.PermCRMNavigationShowPhoneServicesVoicemails();
		//console.log('#########', this, CRMNavigation.PermCRMNavigationShowPhoneServicesPhoneNumbers());

		this.cacheLicensedProjectsSchedulingTime = CRMNavigation.PermCRMNavigationLicensedProjectsSchedulingTime();
		this.cacheLicensedOnCall = CRMNavigation.PermCRMNavigationLicensedOnCall();

	}






	protected OpenLogOut(): void {
		Dialogues.Open({ name: 'ConfirmLogOutDialogue', state: null });
	}

	protected ExitDemo(): void {
		window.location.reload();
	}

}
</script>