<template>
	<div>
		<v-app-bar
			v-if="showAppBar"
			color="#747389"
			dark
			fixed
			app
			clipped-right
			>
			<v-progress-linear
				v-if="isLoadingData"
				:indeterminate="true"
				absolute
				top
				color="white"
			></v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Company Account<span v-if="FullName">: {{FullName}}</span></v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->
			
			<NotificationBellButton />
			<HelpMenuButton>
				<v-list-item
					@click="OnlineHelpFiles()"
					>
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Company Account Tutorial Pages</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</HelpMenuButton>
			<ReloadButton @reload="$emit('reload')" />

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
					<!-- <v-list-item
						@click="DoPrint()"
						:disabled="connectionStatus != 'Connected'"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="CSVDownloadContact(value)"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item> -->
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
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'General'}}).catch(((e) => {}));"
						>
						General
					</v-tab>
					<v-tab
						:disabled="!value || isMakingNew"
						@click="$router.replace({query: { ...$route.query, tab: 'Permissions'}}).catch(((e) => {}));"
						>
						Projects
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>

		<v-breadcrumbs
			v-if="breadcrumbs"
			:items="breadcrumbs"
			style="padding-bottom: 0px; padding-top: 15px; background: white;">
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
		
		<div v-if="!value" style="margin-top: 20px;" class="fadeIn404">
			<v-container>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						<div class="title">Company Account Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The company account no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the company account while you were opening it.</li>
							<li>There is trouble connecting to the internet.</li>
							<li>Your mobile phone is in a place with a poor connection.</li>
							<li>Other reasons the app can't connect to Dispatch Pulse.</li>
						</ul>
					</v-col>
				</v-row>
			</v-container>
		</div>
		<div v-else>
			<v-tabs
				v-if="!showAppBar"
				v-model="tab"
				background-color="transparent"
				grow
				show-arrows
				style="display:none;"
				>
				<v-tab>
					Permissions
				</v-tab>
			</v-tabs>
				
			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					
					<v-card flat>
						
						
						
						<v-form ref="generalForm" autocomplete="newpassword">
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-card-title style="padding-left:0px;padding-right:0px;">Groups</v-card-title>
										<v-card-subtitle style="padding-left:0px;padding-right:0px;">Select the groups for this account.</v-card-subtitle>
										<v-card-text style="padding-left:0px;padding-right:0px;">Changes to groups take effect immediately on change.</v-card-text>
										<v-select
											v-model="SelectedGroups"
											hint="Select the main group for this account."
											:items="Groups"
											label="Select"
											single-line
											multiple
											chips
											>
										</v-select>
										
										<v-expansion-panels accordion>
											<v-expansion-panel>
												<v-expansion-panel-header>Group Provided Permissions</v-expansion-panel-header>
												<v-expansion-panel-content
													v-for="(entry, index) in PermissionsForSelectedGroups"
													:key="index">
													<v-icon>done</v-icon> {{PermissionsKeyToDisplayName(entry)}}
												</v-expansion-panel-content>
											</v-expansion-panel>
											<v-expansion-panel>
												<v-expansion-panel-header>Other Permissions</v-expansion-panel-header>
												<v-expansion-panel-content>
													<v-card-subtitle style="padding-left:0px;padding-right:0px; padding-top: 0px;">In addition to the permissions given by the groups, what else should this account be able to do?</v-card-subtitle>
													
													
													
													<p style="font-weight: bold">Billing</p>
													
													
													<p style="margin-top: 20px;">Accounts</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.contacts.read-company') === -1">
														<v-switch
															v-model="PermBillingContactsReadCompany"
															:label="PermissionsKeyToDisplayName('billing.contacts.read-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.contacts.add-company') === -1">
														<v-switch
															v-model="PermBillingContactsAddCompany"
															:label="PermissionsKeyToDisplayName('billing.contacts.add-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.contacts.modify-company') === -1">
														<v-switch
															v-model="PermBillingContactsModifyCompany"
															:label="PermissionsKeyToDisplayName('billing.contacts.modify-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.contacts.delete-company') === -1">
														<v-switch
															v-model="PermBillingContactsDeleteCompany"
															:label="PermissionsKeyToDisplayName('billing.contacts.delete-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Permissions #2</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-bool.read-self') === -1">
														<v-switch
															v-model="PermBillingPermissionsBoolReadSelf"
															:label="PermissionsKeyToDisplayName('billing.permissions-bool.read-self')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-bool.read-company') === -1">
														<v-switch
															v-model="PermBillingPermissionsBoolReadCompany"
															:label="PermissionsKeyToDisplayName('billing.permissions-bool.read-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-bool.modify-company') === -1">
														<v-switch
															v-model="PermBillingPermissionsBoolModifyCompany"
															:label="PermissionsKeyToDisplayName('billing.permissions-bool.modify-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-bool.delete-company') === -1">
														<v-switch
															v-model="PermBillingPermissionsBoolDeleteCompany"
															:label="PermissionsKeyToDisplayName('billing.permissions-bool.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-bool.add-company') === -1">
														<v-switch
															v-model="PermBillingPermissionsBoolAddCompany"
															:label="PermissionsKeyToDisplayName('billing.permissions-bool.add-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Permission Groups</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-groups-memberships.read-self') === -1">
														<v-switch
															v-model="PermbillingPermissionsGroupsMembershipsReadSelf"
															:label="PermissionsKeyToDisplayName('billing.permissions-groups-memberships.read-self')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-groups.read-company') === -1">
														<v-switch
															v-model="PermBillingPermissionsGroupsReadCompany"
															:label="PermissionsKeyToDisplayName('billing.permissions-groups.read-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-groups-memberships.read-company') === -1">
														<v-switch
															v-model="PermBillingPermissionsGroupsMembershipsReadCompany"
															:label="PermissionsKeyToDisplayName('billing.permissions-groups-memberships.read-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-groups-memberships.modify-company') === -1">
														<v-switch
															v-model="PermBillingPermissionsGroupsMembershipsModifyCompany"
															:label="PermissionsKeyToDisplayName('billing.permissions-groups-memberships.modify-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.permissions-groups-memberships.delete-company') === -1">
														<v-switch
															v-model="PermBillingPermissionsGroupsMembershipsDeleteCompany"
															:label="PermissionsKeyToDisplayName('billing.permissions-groups-memberships.delete-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Sessions</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.sessions.read-self') === -1">
														<v-switch
															v-model="PermBillingSessionsReadSelf"
															:label="PermissionsKeyToDisplayName('billing.sessions.read-self')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.sessions.read-company') === -1">
														<v-switch
															v-model="PermBillingSessionsReadCompany"
															:label="PermissionsKeyToDisplayName('billing.sessions.read-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.sessions.add-company') === -1">
														<v-switch
															v-model="PermBillingSessionsAddCompany"
															:label="PermissionsKeyToDisplayName('billing.sessions.add-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.sessions.modify-company') === -1">
														<v-switch
															v-model="PermBillingSessionsModifyCompany"
															:label="PermissionsKeyToDisplayName('billing.sessions.modify-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.sessions.delete-company') === -1">
														<v-switch
															v-model="PermBillingSessionsDeleteCompany"
															:label="PermissionsKeyToDisplayName('billing.sessions.delete-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Provisioning Status</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.subscription-provisioning-status.request-company') === -1">
														<v-switch
															v-model="PermBillingSubscriptionProvisioningStatusRequestCompany"
															:label="PermissionsKeyToDisplayName('billing.subscription-provisioning-status.request-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Coupon Codes</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.coupon-codes.read-company') === -1">
														<v-switch
															v-model="PermBillingCouponCodesReadCompany"
															:label="PermissionsKeyToDisplayName('billing.coupon-codes.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Industries</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.industries.read-company') === -1">
														<v-switch
															v-model="PermBillingIndustriesReadCompany"
															:label="PermissionsKeyToDisplayName('billing.industries.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">Invoices</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.invoices.read-company') === -1">
														<v-switch
															v-model="PermBillingInvoicesReadCompany"
															:label="PermissionsKeyToDisplayName('billing.invoices.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													<p style="margin-top: 20px;">Journal Entries</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.journal-entries.read-company') === -1">
														<v-switch
															v-model="PermBillingJournalEntriesReadCompany"
															:label="PermissionsKeyToDisplayName('billing.journal-entries.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Packages</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.packages.read-company') === -1">
														<v-switch
															v-model="PermBillingPackagesReadCompany"
															:label="PermissionsKeyToDisplayName('billing.packages.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Package Types</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.packages-type.read-company') === -1">
														<v-switch
															v-model="PermBillingPackagesTypeReadCompany"
															:label="PermissionsKeyToDisplayName('billing.packages-type.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													<p style="margin-top: 20px;">Payment Frequencies</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.payment-frequencies.read-company') === -1">
														<v-switch
															v-model="PermBillingPaymentFrequenciesReadCompany"
															:label="PermissionsKeyToDisplayName('billing.payment-frequencies.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Payment Methods</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.payment-method.read-company') === -1">
														<v-switch
															v-model="PermBillingPaymentMethodReadCompany"
															:label="PermissionsKeyToDisplayName('billing.payment-method.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Subscriptions</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.subscription.read-company') === -1">
														<v-switch
															v-model="PermBillingSubscriptionReadCompany"
															:label="PermissionsKeyToDisplayName('billing.subscription.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">Currencies</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.currency.read-company') === -1">
														<v-switch
															v-model="PermBillingCurrencyReadCompany"
															:label="PermissionsKeyToDisplayName('billing.currency.read-company')"
															:hide-details="true"
															/>
													</div>
													
													<p style="margin-top: 20px;">Journal Entry Types</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('billing.journal-entries-type.read-company') === -1">
														<v-switch
															v-model="PermBillingJournalEntriesTypeReadCompany"
															:label="PermissionsKeyToDisplayName('billing.journal-entries-type.read-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													<p style="font-weight: bold; margin-top: 40px;">Dispatch Pulse</p>
													
													
													
													
													
													
													
													
													
													<p>Agents</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.agents.request-company') === -1">
														<v-switch
															v-model="PermCrmAgentsRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.agents.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.agents.delete-company') === -1">
														<v-switch
															v-model="PermCrmAgentsDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.agents.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.agents.push-company') === -1">
														<v-switch
															v-model="PermCrmAgentsPushCompany"
															:label="PermissionsKeyToDisplayName('crm.agents.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.agents.request-self') === -1">
														<v-switch
															v-model="PermCrmAgentsRequestSelf"
															:label="PermissionsKeyToDisplayName('crm.agents.request-self')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.agents.display-own') === -1">
														<v-switch
															v-model="PermCrmAgentsDisplayOwn"
															:label="PermissionsKeyToDisplayName('crm.agents.display-own')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													<p style="margin-top: 20px;">Agent's Employment Status</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.employment-status.delete-company') === -1">
														<v-switch
															v-model="PermCrmAgentsEmploymentStatusDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.employment-status.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.employment-status.push-company') === -1">
														<v-switch
															v-model="PermCrmAgentsEmploymentStatusPushCompany"
															:label="PermissionsKeyToDisplayName('crm.employment-status.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.employment-status.request-company') === -1">
														<v-switch
															v-model="PermCrmAgentsEmploymentStatusRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.employment-status.request-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													<p style="margin-top: 20px;">Assignments</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.assignments.request-company') === -1">
														<v-switch
															v-model="PermCrmAssignmentsRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.assignments.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.assignments.delete-company') === -1">
														<v-switch
															v-model="PermCrmAssignmentsDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.assignments.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.assignments.push-company') === -1">
														<v-switch
															v-model="PermCrmAssignmentsPushCompany"
															:label="PermissionsKeyToDisplayName('crm.assignments.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.assignments.request-self') === -1">
														<v-switch
															v-model="PermCrmAssignmentsRequestSelf"
															:label="PermissionsKeyToDisplayName('crm.assignments.request-self')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													<p style="margin-top: 20px;">Assignment's Status</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.assignments-status.request-company') === -1">
														<v-switch
															v-model="PermCrmAssignmentsStatusRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.assignments-status.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.assignments-status.delete-company') === -1">
														<v-switch
															v-model="PermCrmAssignmentsStatusDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.assignments-status.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.assignments-status.push-company') === -1">
														<v-switch
															v-model="PermCrmAssignmentsStatusPushCompany"
															:label="PermissionsKeyToDisplayName('crm.assignments-status.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													<p style="margin-top: 20px;">Companies</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.companies.request-company') === -1">
														<v-switch
															v-model="PermCrmCompaniesRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.companies.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.companies.delete-company') === -1">
														<v-switch
															v-model="PermCrmCompaniesDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.companies.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.companies.push-company') === -1">
														<v-switch
															v-model="PermCrmCompaniesPushCompany"
															:label="PermissionsKeyToDisplayName('crm.companies.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">Contacts</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.contacts.request-company') === -1">
														<v-switch
															v-model="PermCrmContactsRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.contacts.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.contacts.delete-company') === -1">
														<v-switch
															v-model="PermCrmContactsDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.contacts.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.contacts.push-company') === -1">
														<v-switch
															v-model="PermCrmContactsPushCompany"
															:label="PermissionsKeyToDisplayName('crm.contacts.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Labour</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour.request-company') === -1">
														<v-switch
															v-model="PermCrmLabourRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.labour.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour.delete-company') === -1">
														<v-switch
															v-model="PermCrmEstimatingManHoursDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.labour.delete-company')"
															:hide-details="true"
															/>
														
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour.push-company') === -1">
														<v-switch
															v-model="PermCrmLabourPushCompany"
															:label="PermissionsKeyToDisplayName('crm.labour.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour.manual-entries') === -1">
														<v-switch
															v-model="PermCrmLabourManualEntries"
															:label="PermissionsKeyToDisplayName('crm.labour.manual-entries')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour.delete-self') === -1">
														<v-switch
															v-model="PermCrmLabourDeleteSelf"
															:label="PermissionsKeyToDisplayName('crm.labour.delete-self')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour.push-self') === -1">
														<v-switch
															v-model="PermCrmLabourPushSelf"
															:label="PermissionsKeyToDisplayName('crm.labour.push-self')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Labour Subtype Exception</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-exception.request-company') === -1">
														<v-switch
															v-model="PermCrmLabourSubtypeExceptionRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-exception.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-exception.push-company') === -1">
														<v-switch
															v-model="PermcrmLabourSubtypeExceptionPushCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-exception.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-exception.delete-company') === -1">
														<v-switch
															v-model="PermCrmLabourSubtypeExceptionDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-exception.delete-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Labour Subtype Holidays</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-holidays.request-company') === -1">
														<v-switch
															v-model="PermCrmLabourSubtypeHolidaysRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-holidays.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-holidays.delete-company') === -1">
														<v-switch
															v-model="PermCrmLabourSubtypeHolidaysDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-holidays.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-holidays.push-company') === -1">
														<v-switch
															v-model="PermCrmLabourSubtypeHolidaysPushCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-holidays.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													<p style="margin-top: 20px;">Labour Subtype Non-billable</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-non-billable.request-company') === -1">
														<v-switch
															v-model="PermCrmLabourSubtypeNonBillableRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-non-billable.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-non-billable.delete-company') === -1">
														<v-switch
															v-model="PermCrmLabourSubtypeNonBillableDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-non-billable.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-subtype-non-billable.push-company') === -1">
														<v-switch
															v-model="PermCrmLabourSubtypeNonBillablePushCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-subtype-non-billable.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">Labour Types</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-types.request-company') === -1">
														<v-switch
															v-model="PermCrmLabourTypesRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-types.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-types.delete-company') === -1">
														<v-switch
															v-model="PermCrmLabourTypesDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-types.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.labour-types.push-company') === -1">
														<v-switch
															v-model="PermCrmLabourTypesPushCompany"
															:label="PermissionsKeyToDisplayName('crm.labour-types.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													<p style="margin-top: 20px;">Products</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.products.request-company') === -1">
														<v-switch
															v-model="PermCrmProductsRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.products.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.products.delete-company') === -1">
														<v-switch
															v-model="PermCrmProductsDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.products.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.products.push-company') === -1">
														<v-switch
															v-model="PermCrmProductsPushCompany"
															:label="PermissionsKeyToDisplayName('crm.products.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Projects</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.projects.request-company') === -1">
														<v-switch
															v-model="PermCrmProjectsRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.projects.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.projects.push-company') === -1">
														<v-switch
															v-model="PermCrmProjectsPushCompany"
															:label="PermissionsKeyToDisplayName('crm.projects.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.projects.delete-company') === -1">
														<v-switch
															v-model="PermCrmProjectsDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.projects.delete-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Project's Status</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.project-status.request-company') === -1">
														<v-switch
															v-model="PermCrmProjectStatusRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.project-status.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.project-status.delete-company') === -1">
														<v-switch
															v-model="PermCrmProjectStatusDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.project-status.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.project-status.push-company') === -1">
														<v-switch
															v-model="PermCrmProjectStatusPushCompany"
															:label="PermissionsKeyToDisplayName('crm.project-status.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.view.billing-index.merge-projects') === -1">
														<v-switch
															v-model="PermCrmViewBllingIndexMergeProjects"
															:label="PermissionsKeyToDisplayName('crm.view.billing-index.merge-projects')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Settings (User)</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.settings-user.request-company') === -1">
														<v-switch
															v-model="PermCrmSettingsUserRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.settings-user.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.settings-user.push-company') === -1">
														<v-switch
															v-model="PermCrmSettingsUserPushCompany"
															:label="PermissionsKeyToDisplayName('crm.settings-user.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.settings-user.delete-company') === -1">
														<v-switch
															v-model="PermCrmSettingsUserDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.settings-user.delete-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													<p style="margin-top: 20px;">Skills</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.skills.request-company') === -1">
														<v-switch
															v-model="PermCrmSkillsRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.skills.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.skills.push-company') === -1">
														<v-switch
															v-model="PermCrmSkillsPushCompany"
															:label="PermissionsKeyToDisplayName('crm.skills.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.skills.delete-company') === -1">
														<v-switch
															v-model="PermCrmSkillsDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.skills.delete-company')"
															:hide-details="true"
															/>
														
													</div>
													
													
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Project Notes</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.project-notes.request-company') === -1">
														<v-switch
															v-model="PermCrmProjectNotesRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.project-notes.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.project-notes.delete-company') === -1">
														<v-switch
															v-model="PermCrmProjectNotesDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.project-notes.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.project-notes.push-company') === -1">
														<v-switch
															v-model="PermCrmProjectNotesPushCompany"
															:label="PermissionsKeyToDisplayName('crm.project-notes.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													<p style="margin-top: 20px;">Man Hours</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.estimating-man-hours.delete-company') === -1">
														<v-switch
															v-model="PermCrmEstimatingManHoursDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.estimating-man-hours.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.estimating-man-hours.request-company') === -1">
														<v-switch
															v-model="PermCrmEstimatingManHoursRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.estimating-man-hours.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.estimating-man-hours.push-company') === -1">
														<v-switch
															v-model="PermCrmEstimatingManHoursPushCompany"
															:label="PermissionsKeyToDisplayName('crm.estimating-man-hours.push-company')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">Materials</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.materials.delete-company') === -1">
														<v-switch
															v-model="PermCrmMaterialsDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.materials.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.materials.push-company') === -1">
														<v-switch
															v-model="PermCrmMaterialsPushCompany"
															:label="PermissionsKeyToDisplayName('crm.materials.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.materials.request-company') === -1">
														<v-switch
															v-model="PermCrmMaterialsRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.materials.request-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.materials.request-self') === -1">
														<v-switch
															v-model="PermCrmMaterialsRequestSelf"
															:label="PermissionsKeyToDisplayName('crm.materials.request-self')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													<p style="margin-top: 20px;">Default Settings</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.settings-default.delete-company') === -1">
														<v-switch
															v-model="PermCrmSettingsDefaultDeleteCompany"
															:label="PermissionsKeyToDisplayName('crm.settings-default.delete-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.settings-default.push-company') === -1">
														<v-switch
															v-model="PermCrmSettingsDefaultPushCompany"
															:label="PermissionsKeyToDisplayName('crm.settings-default.push-company')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.settings-default.request-company') === -1">
														<v-switch
															v-model="PermCrmSettingsDefaultRequestCompany"
															:label="PermissionsKeyToDisplayName('crm.settings-default.request-company')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">Dashboard</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.view.dashboard.dispatch-tab') === -1">
														<v-switch
															v-model="PermCrmViewDashboardDispatchTab"
															:label="PermissionsKeyToDisplayName('crm.view.dashboard.dispatch-tab')"
															:hide-details="true"
															/>
													</div>
													
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.view.dashboard.billing-tab') === -1">
														<v-switch
															v-model="PermCrmViewDashboardBillingTab"
															:label="PermissionsKeyToDisplayName('crm.view.dashboard.billing-tab')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.view.dashboard.management-tab') === -1">
														<v-switch
															v-model="PermCrmViewDashboardManagementTab"
															:label="PermissionsKeyToDisplayName('crm.view.dashboard.management-tab')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													<p style="margin-top: 20px;">Navigation</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.address-book') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAddressBook"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.address-book')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.all-contacts') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAllContacts"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.all-contacts')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.all-companies') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAllCompanies"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.all-companies')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.projects') === -1">
														<v-switch
															v-model="PermCrmNavigationShowProjects"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.projects')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.all-projects') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAllProjects"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.all-projects')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.all-material-entries') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAllMaterialEntries"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.all-material-entries')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.project-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowProjectDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.project-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.product-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowProductDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.product-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.assignment-status-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAssignmentStatusDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.assignment-status-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.man-hours-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowManHoursDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.man-hours-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.project-status-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowProjectStatusDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.project-status-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.agents') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAgents"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.agents')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.all-agents') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAllAgents"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.all-agents')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.all-labour-entries') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAllLabourEntries"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.all-labour-entries')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.agents-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAgentsDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.agents-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.employment-status-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowEmploymentStatusDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.employment-status-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.labour-exception-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowLabourExceptionDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.labour-exception-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.labour-holidays-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowLabourHolidaysDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.labour-holidays-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.labour-non-billable-definitions') === -1">
														<v-switch
															v-model="PermCrmNavigationShowLabourNonBillableDefinitions"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.labour-non-billable-definitions')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.reports') === -1">
														<v-switch
															v-model="PermCrmNavigationShowReports"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.reports')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.all-reports') === -1">
														<v-switch
															v-model="PermCrmNavigationShowAllReports"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.all-reports')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.navigation.show.settings') === -1">
														<v-switch
															v-model="PermCrmNavigationShowSettings"
															:label="PermissionsKeyToDisplayName('crm.navigation.show.settings')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">Reports</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.report.contacts-pdf') === -1">
														<v-switch
															v-model="PermCrmReportContactsPdf"
															:label="PermissionsKeyToDisplayName('crm.report.contacts-pdf')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.report.companies-pdf') === -1">
														<v-switch
															v-model="PermCrmReportCompaniesPdf"
															:label="PermissionsKeyToDisplayName('crm.report.companies-pdf')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.report.projects-pdf') === -1">
														<v-switch
															v-model="PermCrmReportProjectsPdf"
															:label="PermissionsKeyToDisplayName('crm.report.projects-pdf')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.report.assignments-pdf') === -1">
														<v-switch
															v-model="PermCrmReportAssignmentsPdf"
															:label="PermissionsKeyToDisplayName('crm.report.assignments-pdf')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.report.materials-pdf') === -1">
														<v-switch
															v-model="PermCrmReportMaterialsPdf"
															:label="PermissionsKeyToDisplayName('crm.report.materials-pdf')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.report.labour-pdf') === -1">
														<v-switch
															v-model="PermCrmReportLabourPdf"
															:label="PermissionsKeyToDisplayName('crm.report.labour-pdf')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">CSV Exports</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.contacts-csv') === -1">
														<v-switch
															v-model="PermCrmExportContactsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.contacts-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.companies-csv') === -1">
														<v-switch
															v-model="PermCrmExportCompaniesCsv"
															:label="PermissionsKeyToDisplayName('crm.export.companies-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.materials-csv') === -1">
														<v-switch
															v-model="PermCrmExportMaterialsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.materials-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.product-definitions-csv') === -1">
														<v-switch
															v-model="PermCrmExportProductDefinitionsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.product-definitions-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.assignment-status-definitions-csv') === -1">
														<v-switch
															v-model="PermCrmExportAssignmentStatusDefinitionsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.assignment-status-definitions-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.man-hours-definitions-csv') === -1">
														<v-switch
															v-model="PermCrmExportManHoursDefinitionsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.man-hours-definitions-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.project-status-definitions-csv') === -1">
														<v-switch
															v-model="PermCrmExportProjectStatusDefinitionsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.project-status-definitions-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.labour-csv') === -1">
														<v-switch
															v-model="PermCrmExportLabourCsv"
															:label="PermissionsKeyToDisplayName('crm.export.labour-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.employment-status-definitions-csv') === -1">
														<v-switch
															v-model="PermCrmExportEmploymentStatusDefinitionsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.employment-status-definitions-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.labour-exception-definitions-csv') === -1">
														<v-switch
															v-model="PermCrmExportLabourExceptionDefinitionsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.labour-exception-definitions-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.labour-holidays-definitions-csv') === -1">
														<v-switch
															v-model="PermCrmExportLabourHolidaysDefinitionsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.labour-holidays-definitions-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.labour-non-billable-definitions-csv') === -1">
														<v-switch
															v-model="PermCrmExportLabourNonBillableDefinitionsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.labour-non-billable-definitions-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.agents-csv') === -1">
														<v-switch
															v-model="PermCrmExportAgentsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.agents-csv')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.export.projects-csv') === -1">
														<v-switch
															v-model="PermCrmExportProjectsCsv"
															:label="PermissionsKeyToDisplayName('crm.export.projects-csv')"
															:hide-details="true"
															/>
													</div>
													
													
													<p style="margin-top: 20px;">Backups</p>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.backups.run-local') === -1">
														<v-switch
															v-model="PermCrmBackupsRunLocal"
															:label="PermissionsKeyToDisplayName('crm.backups.run-local')"
															:hide-details="true"
															/>
													</div>
													<div v-if="PermissionsForSelectedGroups.indexOf('crm.backups.run-server') === -1">
														<v-switch
															v-model="PermCrmBackupsRunServer"
															:label="PermissionsKeyToDisplayName('crm.backups.run-server')"
															:hide-details="true"
															/>
													</div>
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
													
												</v-expansion-panel-content>
											</v-expansion-panel>
										</v-expansion-panels>
										
									</v-col>
								</v-row>
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
							</v-container>
						</v-form>
						
					</v-card>
					
					
				</v-tab-item>
			</v-tabs-items>
		</div>
		
		<v-footer
			v-if="showFooter"
			color="#747389"
			class="white--text"
			app
			inset
			>
			<v-row
				no-gutters
				>
				<v-btn
					:disabled="!value || connectionStatus != 'Connected'"
					color="white"
					text
					rounded
					@click="DoDelete()"
					>
					<v-icon left>delete</v-icon>
					Delete
				</v-btn>
			</v-row>
		</v-footer>
	</div>

</template>
<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import CompanySelectField from '@/Components/Fields/CompanySelectField.vue';
import EditorBase, { IBreadcrumb, VForm } from './EditorBase';
import ProjectList from '@/Components/Lists/ProjectList.vue';
import ContactList from '@/Components/Lists/ContactList.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component, Vue, Prop, Watch } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import GenerateID from '@/Utility/GenerateID';
import PhoneNumberEditRowArrayAdapter from '@/Components/Rows/PhoneNumberEditRowArrayAdapter.vue';
import EMailEditRowArrayAdapter from '@/Components/Rows/EMailEditRowArrayAdapter.vue';
import AddressEditRowArrayAdapter from '@/Components/Rows/AddressEditRowArrayAdapter.vue';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import CSVDownloadContact from '@/Data/CRM/Contact/CSVDownloadContact';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsGroupsMemberships } from '@/Data/Billing/BillingPermissionsGroupsMemberships/BillingPermissionsGroupsMemberships';
import { BillingPermissionsGroups } from '@/Data/Billing/BillingPermissionsGroups/BillingPermissionsGroups';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import PermissionsKeyToDisplayName from '@/Utility/PermissionsKeyToDisplayName';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { guid } from '@/Utility/GlobalTypes';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Agent } from '@/Data/CRM/Agent/Agent';
import bcrypt from 'bcryptjs';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';

@Component({
	components: {
		ProjectList,
		ContactList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		PhoneNumberEditRowArrayAdapter,
		EMailEditRowArrayAdapter,
		AddressEditRowArrayAdapter,
		CompanySelectField,
		ReloadButton,
		NotificationBellButton,
	},
	
})
export default class BillingContactPermissionsEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IBillingContacts | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		generalForm: Vue,
	};
	
	protected GetDemoMode = GetDemoMode;
	protected ValidateRequiredField = ValidateRequiredField;
	protected CSVDownloadContact = CSVDownloadContact;
	protected DialoguesOpen = Dialogues.Open;
	protected PermissionsKeyToDisplayName = PermissionsKeyToDisplayName;
	protected loadingData = false;
	
	protected password1Initial = `UNCHANGED${GenerateID()}`;
	protected password1: string = this.password1Initial;
	protected password2Initial = `UNCHANGED${GenerateID()}`;
	protected password2: string = this.password2Initial;
	
	
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	
	
	
	public GetValidatedForms(): VForm[] {
		return [
			this.$refs.generalForm as VForm,
		];
	}
	
	protected GetTabNameToIndexMap(): Record<string, number> {
		return {
			General: 0,
			general: 0,
			Permissions: 1,
			permissions: 1,
		};
	}
	
	@Watch('value')
	protected valueChanged(val: string, oldVal: string): void {// eslint-disable-line @typescript-eslint/no-unused-vars
		
		this.LoadData();
		
		console.log('valueChanged', val);
	}
	
	protected MountedAfter(): void {
		
		this.LoadData();
		
	}
	
	
	
	protected LoadData(): void {
		
		//console.log('loaddata')
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				const promises: Array<Promise<any>> = [];
			
				if (this.value && this.value.applicationData) {
				
					if (this.value.applicationData.dispatchPulseContactId && 
						!IsNullOrEmpty(this.value.applicationData.dispatchPulseContactId)) {
						
						const contact = Contact.ForId(this.value.applicationData.dispatchPulseContactId);
						if (!contact) {
							
							// Request contact.
							const rtr = Contact.FetchForId(this.value.applicationData.dispatchPulseContactId);
							if (rtr.completeRequestPromise) {
								promises.push(rtr.completeRequestPromise);
							}
							
						}
						
					}
					if (this.value.applicationData.dispatchPulseAgentId && 
						!IsNullOrEmpty(this.value.applicationData.dispatchPulseAgentId)) {
						
						const agent = Agent.ForId(this.value.applicationData.dispatchPulseAgentId);
						if (!agent) {
							
							// Request agent.
							const rtr = Agent.FetchForId(this.value.applicationData.dispatchPulseAgentId);
							if (rtr.completeRequestPromise) {
								promises.push(rtr.completeRequestPromise);
							}
							
						}
						
					}
					
				}
				
				
				if (promises.length > 0) {
					
					this.loadingData = true;
					
					Promise.all(promises).finally(() => {
						this.loadingData = false;
					});
				} else {
					this.loadingData = false;
				}
				
				
			});
		});
	}
	
	
	
	protected get PermBillingContactsReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.contacts.read-company') !== -1;
	}
	protected set PermBillingContactsReadCompany(val: boolean) {
		
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.contacts.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.contacts.read-company'],
			});
		}
		
	}
	
	protected get PermBillingPermissionsBoolReadSelf(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-bool.read-self') !== -1;
	}
	protected set PermBillingPermissionsBoolReadSelf(val: boolean) {
		
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.read-self'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.read-self'],
			});
		}
		
	}
	
	protected get PermBillingSessionsReadSelf(): boolean {
		return this.PermissionsForContact.indexOf('billing.sessions.read-self') !== -1;
	}
	protected set PermBillingSessionsReadSelf(val: boolean) {
		
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.read-self'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.read-self'],
			});
		}
		
	}
	
	protected get PermbillingPermissionsGroupsMembershipsReadSelf(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-groups-memberships.read-self') !== -1;
	}
	protected set PermbillingPermissionsGroupsMembershipsReadSelf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups-memberships.read-self'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups-memberships.read-self'],
			});
		}
	}
	
	
	
	protected get PermBillingContactsAddCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.contacts.add-company') !== -1;
	}
	protected set PermBillingContactsAddCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.contacts.add-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.contacts.add-company'],
			});
		}
	}
	
	protected get PermBillingContactsModifyCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.contacts.modify-company') !== -1;
	}
	protected set PermBillingContactsModifyCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.contacts.modify-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.contacts.modify-company'],
			});
		}
	}
	
	protected get PermBillingContactsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.contacts.delete-company') !== -1;
	}
	protected set PermBillingContactsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.contacts.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.contacts.delete-company'],
			});
		}
	}
	
	protected get PermBillingCouponCodesReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.coupon-codes.read-company') !== -1;
	}
	protected set PermBillingCouponCodesReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.coupon-codes.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.coupon-codes.read-company'],
			});
		}
	}
	
	protected get PermBillingIndustriesReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.industries.read-company') !== -1;
	}
	protected set PermBillingIndustriesReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.industries.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.industries.read-company'],
			});
		}
	}
	
	protected get PermBillingInvoicesReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.invoices.read-company') !== -1;
	}
	protected set PermBillingInvoicesReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.invoices.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.invoices.read-company'],
			});
		}
	}
	
	protected get PermBillingJournalEntriesReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.journal-entries.read-company') !== -1;
	}
	protected set PermBillingJournalEntriesReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.journal-entries.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.journal-entries.read-company'],
			});
		}
	}
	
	protected get PermBillingPackagesReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.packages.read-company') !== -1;
	}
	protected set PermBillingPackagesReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.packages.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.packages.read-company'],
			});
		}
	}
	
	protected get PermBillingPackagesTypeReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.packages-type.read-company') !== -1;
	}
	protected set PermBillingPackagesTypeReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.packages-type.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.packages-type.read-company'],
			});
		}
	}
	
	protected get PermBillingPaymentFrequenciesReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.payment-frequencies.read-company') !== -1;
	}
	protected set PermBillingPaymentFrequenciesReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.payment-frequencies.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.payment-frequencies.read-company'],
			});
		}
	}
	
	protected get PermBillingPaymentMethodReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.payment-method.read-company') !== -1;
	}
	protected set PermBillingPaymentMethodReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.payment-method.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.payment-method.read-company'],
			});
		}
	}
	
	protected get PermBillingPermissionsBoolReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-bool.read-company') !== -1;
	}
	protected set PermBillingPermissionsBoolReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.read-company'],
			});
		}
	}
	
	protected get PermBillingPermissionsBoolModifyCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-bool.modify-company') !== -1;
	}
	protected set PermBillingPermissionsBoolModifyCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.modify-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.modify-company'],
			});
		}
	}
	
	protected get PermBillingPermissionsBoolDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-bool.delete-company') !== -1;
	}
	protected set PermBillingPermissionsBoolDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.delete-company'],
			});
		}
	}
	
	protected get PermBillingSessionsReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.sessions.read-company') !== -1;
	}
	protected set PermBillingSessionsReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.read-company'],
			});
		}
	}
	
	protected get PermBillingSessionsAddCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.sessions.add-company') !== -1;
	}
	protected set PermBillingSessionsAddCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.add-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.add-company'],
			});
		}
	}
	
	protected get PermBillingSessionsModifyCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.sessions.modify-company') !== -1;
	}
	protected set PermBillingSessionsModifyCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.modify-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.modify-company'],
			});
		}
	}
	
	protected get PermBillingSubscriptionReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.subscription.read-company') !== -1;
	}
	protected set PermBillingSubscriptionReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.subscription.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.subscription.read-company'],
			});
		}
	}
	
	
	
	protected get PermBillingPermissionsGroupsReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-groups.read-company') !== -1;
	}
	protected set PermBillingPermissionsGroupsReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups.read-company'],
			});
		}
	}
	
	protected get PermBillingPermissionsGroupsMembershipsReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-groups-memberships.read-company') !== -1;
	}
	protected set PermBillingPermissionsGroupsMembershipsReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups-memberships.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups-memberships.read-company'],
			});
		}
	}
	
	protected get PermCrmAgentsRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.agents.request-company') !== -1;
	}
	protected set PermCrmAgentsRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.request-company'],
			});
		}
	}
	
	protected get PermCrmAssignmentsRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.assignments.request-company') !== -1;
	}
	protected set PermCrmAssignmentsRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments.request-company'],
			});
		}
	}
	
	protected get PermCrmAssignmentsStatusRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.assignments-status.request-company') !== -1;
	}
	protected set PermCrmAssignmentsStatusRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments-status.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments-status.request-company'],
			});
		}
	}
	
	protected get PermCrmCompaniesRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.companies.request-company') !== -1;
	}
	protected set PermCrmCompaniesRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.companies.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.companies.request-company'],
			});
		}
	}
	
	protected get PermCrmContactsRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.contacts.request-company') !== -1;
	}
	protected set PermCrmContactsRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.contacts.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.contacts.request-company'],
			});
		}
	}
	
	protected get PermCrmLabourRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour.request-company') !== -1;
	}
	protected set PermCrmLabourRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.request-company'],
			});
		}
	}
	
	protected get PermCrmLabourSubtypeExceptionRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-exception.request-company') !== -1;
	}
	protected set PermCrmLabourSubtypeExceptionRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-exception.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-exception.request-company'],
			});
		}
	}
	
	protected get PermCrmLabourSubtypeHolidaysRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-holidays.request-company') !== -1;
	}
	protected set PermCrmLabourSubtypeHolidaysRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-holidays.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-holidays.request-company'],
			});
		}
	}
	
	protected get PermCrmLabourSubtypeNonBillableRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-non-billable.request-company') !== -1;
	}
	protected set PermCrmLabourSubtypeNonBillableRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-non-billable.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-non-billable.request-company'],
			});
		}
	}
	
	protected get PermCrmLabourTypesRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-types.request-company') !== -1;
	}
	protected set PermCrmLabourTypesRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-types.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-types.request-company'],
			});
		}
	}
	
	protected get PermCrmProductsRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.products.request-company') !== -1;
	}
	protected set PermCrmProductsRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.products.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.products.request-company'],
			});
		}
	}
	
	protected get PermCrmProjectsRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.projects.request-company') !== -1;
	}
	protected set PermCrmProjectsRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.projects.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.projects.request-company'],
			});
		}
	}
	
	protected get PermCrmProjectStatusRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.project-status.request-company') !== -1;
	}
	protected set PermCrmProjectStatusRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-status.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-status.request-company'],
			});
		}
	}
	
	
	
	protected get PermCrmSettingsUserRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.settings-user.request-company') !== -1;
	}
	protected set PermCrmSettingsUserRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-user.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-user.request-company'],
			});
		}
	}
	
	protected get PermCrmSkillsRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.skills.request-company') !== -1;
	}
	protected set PermCrmSkillsRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.skills.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.skills.request-company'],
			});
		}
	}
	
	protected get PermCrmProjectNotesRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.project-notes.request-company') !== -1;
	}
	protected set PermCrmProjectNotesRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-notes.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-notes.request-company'],
			});
		}
	}
	
	protected get PermCrmAgentsEmploymentStatusDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.employment-status.delete-company') !== -1;
	}
	protected set PermCrmAgentsEmploymentStatusDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.employment-status.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.employment-status.delete-company'],
			});
		}
	}
	
	protected get PermCrmAgentsEmploymentStatusPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.employment-status.push-company') !== -1;
	}
	protected set PermCrmAgentsEmploymentStatusPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.employment-status.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.employment-status.push-company'],
			});
		}
	}
	
	protected get PermCrmAgentsEmploymentStatusRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.employment-status.request-company') !== -1;
	}
	protected set PermCrmAgentsEmploymentStatusRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.employment-status.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.employment-status.request-company'],
			});
		}
	}
	
	protected get PermCrmAssignmentsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.assignments.delete-company') !== -1;
	}
	protected set PermCrmAssignmentsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments.delete-company'],
			});
		}
	}
	
	protected get PermCrmAssignmentsPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.assignments.push-company') !== -1;
	}
	protected set PermCrmAssignmentsPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments.push-company'],
			});
		}
	}
	
	protected get PermCrmAssignmentsStatusDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.assignments-status.delete-company') !== -1;
	}
	protected set PermCrmAssignmentsStatusDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments-status.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments-status.delete-company'],
			});
		}
	}
	
	protected get PermCrmAssignmentsStatusPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.assignments-status.push-company') !== -1;
	}
	protected set PermCrmAssignmentsStatusPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments-status.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments-status.push-company'],
			});
		}
	}
	
	protected get PermCrmCompaniesDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.companies.delete-company') !== -1;
	}
	protected set PermCrmCompaniesDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.companies.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.companies.delete-company'],
			});
		}
	}
	
	protected get PermCrmCompaniesPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.companies.push-company') !== -1;
	}
	protected set PermCrmCompaniesPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.companies.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.companies.push-company'],
			});
		}
	}
	
	protected get PermCrmContactsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.contacts.delete-company') !== -1;
	}
	protected set PermCrmContactsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.contacts.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.contacts.delete-company'],
			});
		}
	}
	
	protected get PermCrmContactsPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.contacts.push-company') !== -1;
	}
	protected set PermCrmContactsPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.contacts.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.contacts.push-company'],
			});
		}
	}
	
	protected get PermCrmEstimatingManHoursDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.estimating-man-hours.delete-company') !== -1;
	}
	protected set PermCrmEstimatingManHoursDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.estimating-man-hours.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.estimating-man-hours.delete-company'],
			});
		}
	}
	
	protected get PermCrmEstimatingManHoursRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.estimating-man-hours.request-company') !== -1;
	}
	protected set PermCrmEstimatingManHoursRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.estimating-man-hours.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.estimating-man-hours.request-company'],
			});
		}
	}
	
	protected get PermCrmLabourDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour.delete-company') !== -1;
	}
	protected set PermCrmLabourDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.delete-company'],
			});
		}
	}
	
	protected get PermCrmLabourPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour.push-company') !== -1;
	}
	protected set PermCrmLabourPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.push-company'],
			});
		}
	}
	
	protected get PermcrmLabourSubtypeExceptionPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-exception.push-company') !== -1;
	}
	protected set PermcrmLabourSubtypeExceptionPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-exception.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-exception.push-company'],
			});
		}
	}
	
	
	protected get PermBillingSubscriptionProvisioningStatusRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.subscription-provisioning-status.request-company') !== -1;
	}
	protected set PermBillingSubscriptionProvisioningStatusRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.subscription-provisioning-status.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.subscription-provisioning-status.request-company'],
			});
		}
	}
	
	protected get PermCrmLabourSubtypeHolidaysDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-holidays.delete-company') !== -1;
	}
	protected set PermCrmLabourSubtypeHolidaysDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-holidays.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-holidays.delete-company'],
			});
		}
	}
	
	protected get PermCrmLabourSubtypeHolidaysPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-holidays.push-company') !== -1;
	}
	protected set PermCrmLabourSubtypeHolidaysPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-holidays.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-holidays.push-company'],
			});
		}
	}
	
	protected get PermCrmLabourSubtypeNonBillableDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-non-billable.delete-company') !== -1;
	}
	protected set PermCrmLabourSubtypeNonBillableDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-non-billable.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-non-billable.delete-company'],
			});
		}
	}
	
	protected get PermCrmLabourSubtypeNonBillablePushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-non-billable.push-company') !== -1;
	}
	protected set PermCrmLabourSubtypeNonBillablePushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-non-billable.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-non-billable.push-company'],
			});
		}
	}
	
	protected get PermCrmLabourTypesDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-types.delete-company') !== -1;
	}
	protected set PermCrmLabourTypesDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-types.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-types.delete-company'],
			});
		}
	}
	
	protected get PermCrmLabourTypesPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-types.push-company') !== -1;
	}
	protected set PermCrmLabourTypesPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-types.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-types.push-company'],
			});
		}
	}
	
	protected get PermCrmMaterialsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.materials.delete-company') !== -1;
	}
	protected set PermCrmMaterialsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.materials.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.materials.delete-company'],
			});
		}
	}
	
	protected get PermCrmMaterialsPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.materials.push-company') !== -1;
	}
	protected set PermCrmMaterialsPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.materials.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.materials.push-company'],
			});
		}
	}
	
	protected get PermCrmMaterialsRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.materials.request-company') !== -1;
	}
	protected set PermCrmMaterialsRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.materials.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.materials.request-company'],
			});
		}
	}
	
	protected get PermCrmProductsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.products.delete-company') !== -1;
	}
	protected set PermCrmProductsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.products.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.products.delete-company'],
			});
		}
	}
	
	protected get PermCrmProductsPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.products.push-company') !== -1;
	}
	protected set PermCrmProductsPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.products.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.products.push-company'],
			});
		}
	}
	
	protected get PermCrmProjectNotesDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.project-notes.delete-company') !== -1;
	}
	protected set PermCrmProjectNotesDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-notes.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-notes.delete-company'],
			});
		}
	}
	
	protected get PermCrmProjectNotesPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.project-notes.push-company') !== -1;
	}
	protected set PermCrmProjectNotesPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-notes.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-notes.push-company'],
			});
		}
	}
	
	protected get PermCrmProjectsPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.projects.push-company') !== -1;
	}
	protected set PermCrmProjectsPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.projects.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.projects.push-company'],
			});
		}
	}
	
	protected get PermCrmProjectStatusDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.project-status.delete-company') !== -1;
	}
	protected set PermCrmProjectStatusDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-status.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-status.delete-company'],
			});
		}
	}
	
	protected get PermCrmProjectStatusPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.project-status.push-company') !== -1;
	}
	protected set PermCrmProjectStatusPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-status.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.project-status.push-company'],
			});
		}
	}
	
	protected get PermCrmSettingsDefaultDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.settings-default.delete-company') !== -1;
	}
	protected set PermCrmSettingsDefaultDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-default.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-default.delete-company'],
			});
		}
	}
	
	protected get PermCrmSettingsDefaultPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.settings-default.push-company') !== -1;
	}
	protected set PermCrmSettingsDefaultPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-default.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-default.push-company'],
			});
		}
	}
	
	protected get PermCrmSettingsDefaultRequestCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.settings-default.request-company') !== -1;
	}
	protected set PermCrmSettingsDefaultRequestCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-default.request-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-default.request-company'],
			});
		}
	}
	
	
	
	protected get PermCrmSettingsUserDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.settings-user.delete-company') !== -1;
	}
	protected set PermCrmSettingsUserDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-user.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-user.delete-company'],
			});
		}
	}
	
	protected get PermCrmSkillsPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.skills.push-company') !== -1;
	}
	protected set PermCrmSkillsPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.skills.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.skills.push-company'],
			});
		}
	}
	
	protected get PermCrmAgentsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.agents.delete-company') !== -1;
	}
	protected set PermCrmAgentsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.delete-company'],
			});
		}
	}
	
	protected get PermCrmAgentsPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.agents.push-company') !== -1;
	}
	protected set PermCrmAgentsPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.push-company'],
			});
		}
	}
	
	protected get PermCrmAgentsDisplayOwn(): boolean {
		return this.PermissionsForContact.indexOf('crm.agents.display-own') !== -1;
	}
	protected set PermCrmAgentsDisplayOwn(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.display-own'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.display-own'],
			});
		}
	}
	
	
	protected get PermCrmAgentsRequestSelf(): boolean {
		return this.PermissionsForContact.indexOf('crm.agents.request-self') !== -1;
	}
	protected set PermCrmAgentsRequestSelf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.request-self'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.agents.request-self'],
			});
		}
	}
	
	protected get PermCrmSettingsUserPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.settings-user.push-company') !== -1;
	}
	protected set PermCrmSettingsUserPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-user.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.settings-user.push-company'],
			});
		}
	}
	
	protected get PermCrmEstimatingManHoursPushCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.estimating-man-hours.push-company') !== -1;
	}
	protected set PermCrmEstimatingManHoursPushCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.estimating-man-hours.push-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.estimating-man-hours.push-company'],
			});
		}
	}
	
	protected get PermCrmLabourSubtypeExceptionDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour-subtype-exception.delete-company') !== -1;
	}
	protected set PermCrmLabourSubtypeExceptionDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-exception.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour-subtype-exception.delete-company'],
			});
		}
	}
	
	protected get PermBillingCurrencyReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.currency.read-company') !== -1;
	}
	protected set PermBillingCurrencyReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.currency.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.currency.read-company'],
			});
		}
	}
	
	protected get PermBillingPermissionsBoolAddCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-bool.add-company') !== -1;
	}
	protected set PermBillingPermissionsBoolAddCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.add-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-bool.add-company'],
			});
		}
	}
	
	protected get PermBillingSessionsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.sessions.delete-company') !== -1;
	}
	protected set PermBillingSessionsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.sessions.delete-company'],
			});
		}
	}
	
	protected get PermBillingPermissionsGroupsMembershipsModifyCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-groups-memberships.modify-company') !== -1;
	}
	protected set PermBillingPermissionsGroupsMembershipsModifyCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups-memberships.modify-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups-memberships.modify-company'],
			});
		}
	}
	
	protected get PermBillingPermissionsGroupsMembershipsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.permissions-groups-memberships.delete-company') !== -1;
	}
	protected set PermBillingPermissionsGroupsMembershipsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups-memberships.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.permissions-groups-memberships.delete-company'],
			});
		}
	}
	
	protected get PermBillingJournalEntriesTypeReadCompany(): boolean {
		return this.PermissionsForContact.indexOf('billing.journal-entries-type.read-company') !== -1;
	}
	protected set PermBillingJournalEntriesTypeReadCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.journal-entries-type.read-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['billing.journal-entries-type.read-company'],
			});
		}
	}
	
	protected get PermCrmProjectsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.projects.delete-company') !== -1;
	}
	protected set PermCrmProjectsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.projects.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.projects.delete-company'],
			});
		}
	}
	
	
	
	protected get PermCrmSkillsDeleteCompany(): boolean {
		return this.PermissionsForContact.indexOf('crm.skills.delete-company') !== -1;
	}
	protected set PermCrmSkillsDeleteCompany(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.skills.delete-company'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.skills.delete-company'],
			});
		}
	}
	
	
	protected get PermCrmViewDashboardDispatchTab(): boolean {
		return this.PermissionsForContact.indexOf('crm.view.dashboard.dispatch-tab') !== -1;
	}
	protected set PermCrmViewDashboardDispatchTab(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.view.dashboard.dispatch-tab'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.view.dashboard.dispatch-tab'],
			});
		}
	}
	
	protected get PermCrmViewDashboardBillingTab(): boolean {
		return this.PermissionsForContact.indexOf('crm.view.dashboard.billing-tab') !== -1;
	}
	protected set PermCrmViewDashboardBillingTab(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.view.dashboard.billing-tab'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.view.dashboard.billing-tab'],
			});
		}
	}
	
	protected get PermCrmViewDashboardManagementTab(): boolean {
		return this.PermissionsForContact.indexOf('crm.view.dashboard.management-tab') !== -1;
	}
	protected set PermCrmViewDashboardManagementTab(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.view.dashboard.management-tab'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.view.dashboard.management-tab'],
			});
		}
	}
	
	
	protected get PermCrmLabourManualEntries(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour.manual-entries') !== -1;
	}
	protected set PermCrmLabourManualEntries(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.manual-entries'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.manual-entries'],
			});
		}
	}
	
	
	
	
	protected get PermCrmNavigationShowAddressBook(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.address-book') !== -1;
	}
	protected set PermCrmNavigationShowAddressBook(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.address-book'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.address-book'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAllContacts(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.all-contacts') !== -1;
	}
	protected set PermCrmNavigationShowAllContacts(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-contacts'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-contacts'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAllCompanies(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.all-companies') !== -1;
	}
	protected set PermCrmNavigationShowAllCompanies(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-companies'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-companies'],
			});
		}
	}
	
	protected get PermCrmNavigationShowProjects(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.projects') !== -1;
	}
	protected set PermCrmNavigationShowProjects(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.projects'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.projects'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAllProjects(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.all-projects') !== -1;
	}
	protected set PermCrmNavigationShowAllProjects(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-projects'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-projects'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAllAssignments(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.all-assignments') !== -1;
	}
	protected set PermCrmNavigationShowAllAssignments(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-assignments'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-assignments'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAllMaterialEntries(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.all-material-entries') !== -1;
	}
	protected set PermCrmNavigationShowAllMaterialEntries(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-material-entries'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-material-entries'],
			});
		}
	}
	
	protected get PermCrmNavigationShowProjectDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.project-definitions') !== -1;
	}
	protected set PermCrmNavigationShowProjectDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.project-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.project-definitions'],
			});
		}
	}
	
	protected get PermCrmNavigationShowProductDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.product-definitions') !== -1;
	}
	protected set PermCrmNavigationShowProductDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.product-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.product-definitions'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAssignmentStatusDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.assignment-status-definitions') !== -1;
	}
	protected set PermCrmNavigationShowAssignmentStatusDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.assignment-status-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.assignment-status-definitions'],
			});
		}
	}
	
	protected get PermCrmNavigationShowManHoursDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.man-hours-definitions') !== -1;
	}
	protected set PermCrmNavigationShowManHoursDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.man-hours-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.man-hours-definitions'],
			});
		}
	}
	
	protected get PermCrmNavigationShowProjectStatusDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.project-status-definitions') !== -1;
	}
	protected set PermCrmNavigationShowProjectStatusDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.project-status-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.project-status-definitions'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAgents(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.agents') !== -1;
	}
	protected set PermCrmNavigationShowAgents(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.agents'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.agents'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAllAgents(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.all-agents') !== -1;
	}
	protected set PermCrmNavigationShowAllAgents(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-agents'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-agents'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAllLabourEntries(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.all-labour-entries') !== -1;
	}
	protected set PermCrmNavigationShowAllLabourEntries(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-labour-entries'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-labour-entries'],
			});
		}
	}
	
	protected get PermCrmNavigationShowAgentsDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.agents-definitions') !== -1;
	}
	protected set PermCrmNavigationShowAgentsDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.agents-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.agents-definitions'],
			});
		}
	}
	
	
	protected get PermCrmNavigationShowEmploymentStatusDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.employment-status-definitions') !== -1;
	}
	protected set PermCrmNavigationShowEmploymentStatusDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.employment-status-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.employment-status-definitions'],
			});
		}
	}
	
	
	protected get PermCrmNavigationShowLabourExceptionDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.labour-exception-definitions') !== -1;
	}
	protected set PermCrmNavigationShowLabourExceptionDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.labour-exception-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.labour-exception-definitions'],
			});
		}
	}
	
	
	protected get PermCrmNavigationShowLabourHolidaysDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.labour-holidays-definitions') !== -1;
	}
	protected set PermCrmNavigationShowLabourHolidaysDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.labour-holidays-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.labour-holidays-definitions'],
			});
		}
	}
	
	
	protected get PermCrmNavigationShowLabourNonBillableDefinitions(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.labour-non-billable-definitions') !== -1;
	}
	protected set PermCrmNavigationShowLabourNonBillableDefinitions(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.labour-non-billable-definitions'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.labour-non-billable-definitions'],
			});
		}
	}
	
	
	protected get PermCrmNavigationShowReports(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.reports') !== -1;
	}
	protected set PermCrmNavigationShowReports(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.reports'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.reports'],
			});
		}
	}
	
	
	
	protected get PermCrmNavigationShowAllReports(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.all-reports') !== -1;
	}
	protected set PermCrmNavigationShowAllReports(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-reports'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.all-reports'],
			});
		}
	}
	
	
	protected get PermCrmNavigationShowSettings(): boolean {
		return this.PermissionsForContact.indexOf('crm.navigation.show.settings') !== -1;
	}
	protected set PermCrmNavigationShowSettings(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.settings'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.navigation.show.settings'],
			});
		}
	}
	
	protected get PermCrmReportContactsPdf(): boolean {
		return this.PermissionsForContact.indexOf('crm.report.contacts-pdf') !== -1;
	}
	protected set PermCrmReportContactsPdf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.contacts-pdf'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.contacts-pdf'],
			});
		}
	}
	
	
	protected get PermCrmReportCompaniesPdf(): boolean {
		return this.PermissionsForContact.indexOf('crm.report.companies-pdf') !== -1;
	}
	protected set PermCrmReportCompaniesPdf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.companies-pdf'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.companies-pdf'],
			});
		}
	}
	
	
	protected get PermCrmReportProjectsPdf(): boolean {
		return this.PermissionsForContact.indexOf('crm.report.projects-pdf') !== -1;
	}
	protected set PermCrmReportProjectsPdf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.projects-pdf'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.projects-pdf'],
			});
		}
	}
	
	
	protected get PermCrmReportAssignmentsPdf(): boolean {
		return this.PermissionsForContact.indexOf('crm.report.assignments-pdf') !== -1;
	}
	protected set PermCrmReportAssignmentsPdf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.assignments-pdf'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.assignments-pdf'],
			});
		}
	}
	
	
	protected get PermCrmReportMaterialsPdf(): boolean {
		return this.PermissionsForContact.indexOf('crm.report.materials-pdf') !== -1;
	}
	protected set PermCrmReportMaterialsPdf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.materials-pdf'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.materials-pdf'],
			});
		}
	}
	
	
	
	protected get PermCrmReportLabourPdf(): boolean {
		return this.PermissionsForContact.indexOf('crm.report.labour-pdf') !== -1;
	}
	protected set PermCrmReportLabourPdf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.labour-pdf'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.report.labour-pdf'],
			});
		}
	}
	
	
	protected get PermCrmExportContactsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.contacts-csv') !== -1;
	}
	protected set PermCrmExportContactsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.contacts-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.contacts-csv'],
			});
		}
	}
	
	
	protected get PermCrmExportCompaniesCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.companies-csv') !== -1;
	}
	protected set PermCrmExportCompaniesCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.companies-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.companies-csv'],
			});
		}
	}
	
	protected get PermCrmExportAssignmentsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.assignments-csv') !== -1;
	}
	protected set PermCrmExportAssignmentsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.assignments-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.assignments-csv'],
			});
		}
	}
	
	protected get PermCrmExportMaterialsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.materials-csv') !== -1;
	}
	protected set PermCrmExportMaterialsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.materials-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.materials-csv'],
			});
		}
	}
	
	protected get PermCrmExportProductDefinitionsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.product-definitions-csv') !== -1;
	}
	protected set PermCrmExportProductDefinitionsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.product-definitions-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.product-definitions-csv'],
			});
		}
	}
	
	protected get PermCrmExportAssignmentStatusDefinitionsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.assignment-status-definitions-csv') !== -1;
	}
	protected set PermCrmExportAssignmentStatusDefinitionsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.assignment-status-definitions-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.assignment-status-definitions-csv'],
			});
		}
	}
	
	protected get PermCrmExportManHoursDefinitionsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.man-hours-definitions-csv') !== -1;
	}
	protected set PermCrmExportManHoursDefinitionsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.man-hours-definitions-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.man-hours-definitions-csv'],
			});
		}
	}
	
	protected get PermCrmExportProjectStatusDefinitionsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.project-status-definitions-csv') !== -1;
	}
	protected set PermCrmExportProjectStatusDefinitionsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.project-status-definitions-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.project-status-definitions-csv'],
			});
		}
	}
	
	protected get PermCrmExportLabourCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.labour-csv') !== -1;
	}
	protected set PermCrmExportLabourCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.labour-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.labour-csv'],
			});
		}
	}
	
	protected get PermCrmExportEmploymentStatusDefinitionsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.employment-status-definitions-csv') !== -1;
	}
	protected set PermCrmExportEmploymentStatusDefinitionsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.employment-status-definitions-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.employment-status-definitions-csv'],
			});
		}
	}
	
	
	protected get PermCrmExportLabourExceptionDefinitionsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.labour-exception-definitions-csv') !== -1;
	}
	protected set PermCrmExportLabourExceptionDefinitionsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.labour-exception-definitions-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.labour-exception-definitions-csv'],
			});
		}
	}
	
	
	
	protected get PermCrmExportLabourHolidaysDefinitionsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.labour-holidays-definitions-csv') !== -1;
	}
	protected set PermCrmExportLabourHolidaysDefinitionsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.labour-holidays-definitions-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.labour-holidays-definitions-csv'],
			});
		}
	}
	
	
	protected get PermCrmExportLabourNonBillableDefinitionsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.labour-non-billable-definitions-csv') !== -1;
	}
	protected set PermCrmExportLabourNonBillableDefinitionsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.labour-non-billable-definitions-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.labour-non-billable-definitions-csv'],
			});
		}
	}
	
	
	protected get PermCrmExportAgentsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.agents-csv') !== -1;
	}
	protected set PermCrmExportAgentsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.agents-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.agents-csv'],
			});
		}
	}
	
	protected get PermCrmMaterialsRequestSelf(): boolean {
		return this.PermissionsForContact.indexOf('crm.materials.request-self') !== -1;
	}
	protected set PermCrmMaterialsRequestSelf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.materials.request-self'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.materials.request-self'],
			});
		}
	}
	
	
	protected get PermCrmLabourDeleteSelf(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour.delete-self') !== -1;
	}
	protected set PermCrmLabourDeleteSelf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.delete-self'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.delete-self'],
			});
		}
	}
	
	
	
	
	
	
	protected get PermCrmAssignmentsRequestSelf(): boolean {
		return this.PermissionsForContact.indexOf('crm.assignments.request-self') !== -1;
	}
	protected set PermCrmAssignmentsRequestSelf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments.request-self'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.assignments.request-self'],
			});
		}
	}
	
	
	protected get PermCrmLabourPushSelf(): boolean {
		return this.PermissionsForContact.indexOf('crm.labour.push-self') !== -1;
	}
	protected set PermCrmLabourPushSelf(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.push-self'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.labour.push-self'],
			});
		}
	}
	
	protected get PermCrmExportProjectsCsv(): boolean {
		return this.PermissionsForContact.indexOf('crm.export.projects-csv') !== -1;
	}
	protected set PermCrmExportProjectsCsv(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.projects-csv'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.export.projects-csv'],
			});
		}
	}
	
	
	protected get PermCrmViewBllingIndexMergeProjects(): boolean {
		return this.PermissionsForContact.indexOf('crm.view.billing-index.merge-projects') !== -1;
	}
	protected set PermCrmViewBllingIndexMergeProjects(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.view.billing-index.merge-projects'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.view.billing-index.merge-projects'],
			});
		}
	}
	
	
	
	
	
	
	
	
	protected get PermCrmBackupsRunLocal(): boolean {
		return this.PermissionsForContact.indexOf('crm.backups.run-local') !== -1;
	}
	protected set PermCrmBackupsRunLocal(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.backups.run-local'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.backups.run-local'],
			});
		}
	}
	
	
	
	protected get PermCrmBackupsRunServer(): boolean {
		return this.PermissionsForContact.indexOf('crm.backups.run-server') !== -1;
	}
	protected set PermCrmBackupsRunServer(val: boolean) {
		if (!this.value) {
			console.error('!this.value');
			return;
		}
		
		if (val) {
			BillingPermissionsBool.PerformBillingPermissionsBoolAdd.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.backups.run-server'],
			});
		} else {
			BillingPermissionsBool.PerformBillingPermissionsBoolRemove.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId: this.value.uuid,
				permissionKeys: ['crm.backups.run-server'],
			});
		}
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected get Groups(): Array<{
			text: string | number;
			value: string | number;
			disabled?: boolean;
			divider?: boolean;
			header?: string;
		}> {
		
		const all = BillingPermissionsGroups.All();
		if (!all) {
			return [];
		}
		
		const values = Object.values(all);
		
		const ret: Array<{
			text: string | number;
			value: string | number;
			disabled?: boolean;
			divider?: boolean;
			header?: string;
		}> = [];
		
		for (const membership of values) {
			ret.push({
				text: membership.name || membership.id,
				value: membership.id,
			});
		}
		
		
		
		//console.log('ret', ret);
		return ret;
		
	}
	
	
	
	protected get PermissionsForSelectedGroups(): string[] {
		
		const selectedGroups = this.SelectedGroups;
		
		
		
		
		if (selectedGroups.length === 0) {
			return [];
		}
		
		const ret = [];
		const permissionsEntries = BillingPermissionsBool.ForGroupIds(selectedGroups);
		
		//console.debug('permissionsEntries', permissionsEntries);
		
		for (const entry of permissionsEntries) {
			ret.push(entry.key);
		}
		
		//console.debug('PermissionsForSelectedGroups get', ret);
		
		return ret;
	}
	
	
	protected get PermissionsForContact(): string[] {
		if (null == this.value) {
			return [];
		}
		const ret = [];
		
		const permissionsEntries = BillingPermissionsBool.ForBillingContactId(this.value.uuid);
		
		for (const entry of permissionsEntries) {
			ret.push(entry.key);
		}
		
		
		//console.debug('PermissionsForContact', ret);
		
		return ret;
	}
	
	
	
	
	
	
	
	protected get SelectedGroups(): string[] {
		if (!this.value) {
			return [];
		}
		
		const groupIds = BillingPermissionsGroups.GroupIdsForBillingId(this.value.uuid);
		
		//console.debug('get SelectedGroups', groupIds);
		
		return groupIds;
	}
	
	protected set SelectedGroups(val: string[]) {
		
		//console.log('set SelectedGroups', val);
		
		if (!this.value) {
			return;
		}
		
		const oldGroupIds = BillingPermissionsGroups.GroupIdsForBillingId(this.value.uuid);
		
		const groupIdsRemoved = _.difference(oldGroupIds, val);
		const groupIdsAdded = _.difference(val, oldGroupIds);
		
		BillingPermissionsGroupsMemberships.PerformRemoveMemberships(this.value.uuid, groupIdsRemoved);
		BillingPermissionsGroupsMemberships.PerformAddMemberships(this.value.uuid, groupIdsAdded);
		
		// this.SignalChanged();
	}
	
	protected get FullName(): string | null {
		
		if (!this.value ||
			!this.value.fullName
			) {
			return null;
		}
		
		return this.value.fullName;
	}
	
	protected set FullName(val: string | null) {
		
		if (!this.value) {
			return;
		}
		
		this.value.fullName = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	
	protected get BillingContactDispatchPulseContactId(): guid | null {
		
		if (!this.value ||
			!this.value.applicationData ||
			!this.value.applicationData.dispatchPulseContactId
			) {
			return null;
		}
		
		return this.value.applicationData.dispatchPulseContactId;
	}
	
	protected set BillingContactDispatchPulseContactId(val: guid | null) {
		
		if (!this.value) {
			return;
		}
		
		if (!this.value.applicationData) {
			this.value.applicationData = {};
		}
		
		
		this.value.applicationData.dispatchPulseContactId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get BillingContactDispatchPulseAgentId(): guid | null {
		
		if (!this.value ||
			!this.value.applicationData ||
			!this.value.applicationData.dispatchPulseAgentId
			) {
			return null;
		}
		
		return this.value.applicationData.dispatchPulseAgentId;
	}
	
	protected set BillingContactDispatchPulseAgentId(val: guid | null) {
		
		if (!this.value) {
			return;
		}
		
		if (!this.value.applicationData) {
			this.value.applicationData = {};
		}
		
		this.value.applicationData.dispatchPulseAgentId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	
	
	
	
	
	
	
	
	
	protected get MarketingEmails(): boolean {
		
		if (!this.value ||
			!this.value.emailListMarketing
			) {
			return false;
		}
		
		return this.value.emailListMarketing;
	}
	
	protected set MarketingEmails(val: boolean) {
		
		if (!this.value) {
			return;
		}
		
		this.value.emailListMarketing = val === undefined ? false : val;
		this.SignalChanged();
	}
	
	protected get TutorialEmails(): boolean {
		
		if (!this.value ||
			!this.value.emailListTutorials
			) {
			return false;
		}
		
		return this.value.emailListTutorials;
	}
	
	protected set TutorialEmails(val: boolean) {
		
		if (!this.value) {
			return;
		}
		
		this.value.emailListTutorials = val === undefined ? false : val;
		this.SignalChanged();
	}
	
	protected get EMail(): string | null {
		
		if (!this.value ||
			!this.value.email
			) {
			return null;
		}
		
		return this.value.email;
	}
	
	protected set EMail(val: string | null) {
		
		console.debug('set EMail 1', val);
		
		if (!this.value) {
			return;
		}
		
		this.value.email = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get Password(): string {
		return this.password1;
	}
	
	protected set Password(val: string) {
		
		this.password1 = val;
		
		this.MaybeSetPasscode();
	}
	
	protected get PasswordAgain(): string {
		return this.password2;
	}
	
	protected set PasswordAgain(val: string) {
		
		this.password2 = val;
		
		this.MaybeSetPasscode();
	}
	
	protected MaybeSetPasscode(): void {
		
		if (!this.value) {
			return;
		}
		
		this.password1 = this.password1.trim();
		this.password2 = this.password2.trim();
		
		if (IsNullOrEmpty(this.password1)) {
			console.debug('Not setting passcode because it is empty.');
			return;
		}
		
		if (this.password1 !== this.password2) {
			console.debug('Not setting passcode because 1 and 2 don\'t match.');
			return;
		}
		
		if (this.password1 === this.password1Initial) {
			console.debug('Not setting because password 1 is set to the default password.');
			return;
		}
		
		if (this.password2 === this.password2Initial) {
			console.debug('Not setting because password 2 is set to the default password.');
			return;
		}
		
		const salt = bcrypt.genSaltSync(11);
		const hash = bcrypt.hashSync(this.password1, salt);
		
		this.value.passwordHash = hash;
		
		this.SignalChanged();
		
	}
	
	
	
	protected get Phone(): string | null {
		
		if (!this.value ||
			!this.value.phone
			) {
			return null;
		}
		
		return this.value.phone;
	}
	
	protected set Phone(val: string | null) {
		
		if (!this.value) {
			return;
		}
		
		this.value.phone = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get Id(): string | null {
		
		//console.debug(this.value);
		
		if (!this.value ||
			!this.value.uuid
			) {
			return null;
		}
		
		return this.value.uuid;
	}
	
	
	
	
	
	protected SignalChanged(): void {
		
		// Debounce
		
		if (this.debounceId) {
			clearTimeout(this.debounceId);
			this.debounceId = null;
		}
		
		this.debounceId = setTimeout(() => {
			this.$emit('input', this.value);
		}, 250);
	}
	
	
	
	
	
	
	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');
		
		window.open(
			'https://www.dispatchpulse.com/support/',
			'_blank');
	}
	
	
	
	
	
	//
}

</script>
<style scoped>
</style>