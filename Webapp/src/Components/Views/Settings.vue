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
			
			<v-toolbar-title class="white--text">Settings</v-toolbar-title>

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

					<v-tab  @click="$router.replace({query: { ...$route.query, tab: 'My Account'}}).catch(((e) => {}));">
						My Account
					</v-tab>
					<v-tab
						:disabled="!PermBillingCompaniesCanModify()"
						@click="$router.replace({query: { ...$route.query, tab: 'Company Accounts'}}).catch(((e) => {}));"
						>
						Company Accounts
					</v-tab>
					<v-tab
						:disabled="!PermBillingCompaniesCanModify()"
						@click="$router.replace({query: { ...$route.query, tab: 'Company Settings'}}).catch(((e) => {}));"
						>
						Company Settings
					</v-tab>
					<v-tab
						:disabled="!PermCRMCanRequestSubscriptions() && !PermCRMCanRequestInvoices() && !PermCRMCanRequestJournalEntries()"
						@click="$router.replace({query: { ...$route.query, tab: 'Billing'}}).catch(((e) => {}));"
						>
						Billing
					</v-tab>
					<v-tab  @click="$router.replace({query: { ...$route.query, tab: 'Admin, Backup & Restore'}}).catch(((e) => {}));">
						Admin, Backup &amp; Restore
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs :items="breadcrumbs" style=" padding-bottom: 5px; padding-top: 15px;">
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
				<v-card style="margin: 30px; margin-top: 15px;">
					
					<v-card-text>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<div class="title">Basic</div>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<v-text-field
									:disabled="GetDemoMode() || connectionStatus != 'Connected'"
									:readonly="!PermCRMCanModifySelf()"
									v-model="BillingContactName"
									autocomplete="newpassword"
									label="Name"
									hint="Your name."
									>
								</v-text-field>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<v-btn
									:disabled="GetDemoMode() || connectionStatus != 'Connected' || !PermCRMCanModifySelf()"
									color="primary"
									@click="DialoguesOpen({ name: 'ChangePasswordDialogue', state: null})"
									>Change Password</v-btn>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<div class="title">Contact</div>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="5" offset-sm="1">
								<v-text-field
									:disabled="GetDemoMode() || connectionStatus != 'Connected'"
									:readonly="!PermCRMCanModifySelf()"
									v-model="BillingContactPhone"
									autocomplete="newpassword"
									label="Telephone Number"
									type="phone"
									hint=""
									>
								</v-text-field>
							</v-col>
							<v-col cols="12" sm="5">
								<v-text-field
									:disabled="GetDemoMode() || connectionStatus != 'Connected'"
									:readonly="!PermCRMCanModifySelf()"
									v-model="BillingContactEMail"
									autocomplete="newpassword"
									label="E-Mail"
									type="email"
									hint=""
									>
								</v-text-field>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<div class="subtitle-1">Contact Preferences</div>
								<p>We will always send messages related to app functionality and billing, but for anything not required here you can manage your preferences:</p>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="5" offset-sm="1">
								<v-switch
									:disabled="GetDemoMode() || connectionStatus != 'Connected'"
									:readonly="!PermCRMCanModifySelf()"
									v-model="BillingContactEmailListTutorials"
									label="Tutorials"
									>
								</v-switch>
							</v-col>
							<v-col cols="12" sm="5">
								<v-switch
									:disabled="GetDemoMode() || connectionStatus != 'Connected'"
									:readonly="!PermCRMCanModifySelf()"
									v-model="BillingContactEmailListMarketing"
									label="Marketing"
									>
								</v-switch>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<div class="title">Associated Dispatch Pulse Information</div>
							</v-col>
						</v-row>
						
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<ContactSelectField 
									:disabled="GetDemoMode() || connectionStatus != 'Connected'"
									:readonly="!PermCRMCanModifySelf()"
									v-model="BillingContactDispatchPulseContactId" 
									:showDetails="false"
									/>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<AgentSelectField 
									:disabled="GetDemoMode() || connectionStatus != 'Connected'"
									:readonly="!PermCRMCanModifySelf()"
									v-model="BillingContactDispatchPulseAgentId" 
									/>
							</v-col>
						</v-row>
						
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<div class="title">Advanced</div>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<v-text-field
									v-model="BillingContactUniqueID"
									readonly="readonly"
									label="Unique ID"
									hint="The id of your user."
									>
								</v-text-field>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<v-text-field
									v-model="BillingContactMarketingCampaign"
									readonly="readonly"
									label="How you found out about us."
									hint=""
									>
								</v-text-field>
							</v-col>
						</v-row>
					</v-card-text>
				</v-card>
			</v-tab-item>
			<v-tab-item style="flex: 1;">
				<v-card
					v-if="PermBillingCompaniesCanModify()"
					style="margin: 30px; margin-top: 15px;">
					
					<v-card-title>Accounts</v-card-title>
					<ModifyBillingContactDialogue2 
						v-model="modifyBillingContactModel"
						:isOpen="modifyBillingContactOpen"
						@save="SaveModifyBillingContact"
						@cancel="CancelModifyBillingContact"
						ref="modifyBillingContactDialogue"
						dialogueName="Modify Company Account"
						:isMakingNew="false"
						/>
					<ModifyBillingContactPermissionsDialogue2 
						v-model="modifyBillingContactPermissionsModel"
						:isOpen="modifyBillingContactPermissionsOpen"
						@save="SaveModifyBillingContactPermissions"
						@close="CloseModifyBillingContactPermissions"
						ref="modifyBillingContactPermissionsDialogue"
						dialogueName="Modify Company Account Permissions"
						:isMakingNew="false"
						/>
					<ModifyBillingContactLicensesDialogue2 
						v-model="modifyBillingContactLicensesModel"
						:isOpen="modifyBillingContactLicensesOpen"
						@save="SaveModifyBillingContactLicenses"
						@close="CloseModifyBillingContactLicenses"
						ref="modifyBillingContactLicensesDialogue"
						dialogueName="Modify Company Account Licenses"
						:isMakingNew="false"
						/>
						
						
						
					<ModifyBillingContactDialogue2 
						v-model="newBillingContactModel"
						:isOpen="newBillingContactOpen"
						@save="SaveNewBillingContact"
						@cancel="CancelNewBillingContact"
						ref="newBillingContactDialogue"
						dialogueName="New Company Account"
						:isMakingNew="true"
						/>
					
					<DeleteBillingContactDialogue2
						v-model="deleteBillingContactModel"
						:isOpen="deleteBillingContactOpen"
						@save="SaveDeleteBillingContact"
						@cancel="CancelDeleteBillingContact"
						ref="deleteBillingContactDialogue"
						
						/>
					
					<v-list>
						<v-list-item-group
							color="primary"
							>
							
							<BilingContactListItem
								v-for="(item, i) in CompanyAccounts"
								:key="i"
								v-model="CompanyAccounts[i]"
								@edit-entry="OpenModifyBillingContact"
								@edit-entry-permissions="OpenModifyBillingContactPermissions"
								@edit-entry-licenses="OpenModifyBillingContactLicenses"
								@delete-entry="OpenDeleteBillingContact"
								/>
							
							
							
						</v-list-item-group>
					</v-list>
					
					
					
					
					<v-toolbar
						flat
						
						>
						<v-spacer />
						<v-btn
							text
							@click="OpenNewBillingContact"
							>
							<v-icon>add</v-icon>
							Add Account
						</v-btn>
						
					</v-toolbar>
					
					
				</v-card>
				<PermissionsDeniedAlert v-else />
			</v-tab-item>
			<v-tab-item style="flex: 1;">
				<PhoneIdCard style="margin: 30px; margin-top: 15px;" :disabled="GetDemoMode() || connectionStatus != 'Connected'" />
				<v-card style="margin: 30px; margin-top: 15px;">
					<v-card-text>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<div class="title">Advanced</div>
							</v-col>
						</v-row>
						<v-row>
							<v-col cols="12" sm="10" offset-sm="1">
								<v-text-field
									v-model="BillingContactCompanyID"
									readonly="readonly"
									label="Company ID"
									hint="The id of your company."
									>
								</v-text-field>
							</v-col>
						</v-row>
					</v-card-text>
				</v-card>
			</v-tab-item>
			<v-tab-item style="flex: 1;">
				<BillingPaymentsCard style="margin: 30px; margin-top: 15px;" :disabled="GetDemoMode() || connectionStatus != 'Connected'" />
				<BillingSubscriptionsCard style="margin: 30px; margin-top: 15px;" :disabled="GetDemoMode() || connectionStatus != 'Connected'" />
				<BillingInvoicesCard style="margin: 30px; margin-top: 15px;" :disabled="GetDemoMode() || connectionStatus != 'Connected'" />
				<BillingJournalEntriesCard style="margin: 30px; margin-top: 15px;" :disabled="GetDemoMode() || connectionStatus != 'Connected'" />
			</v-tab-item>
			<v-tab-item style="flex: 1;"> <!--Admin, Backup & Restore-->
				<!--<v-card style="margin: 30px; margin-top: 15px;">
					<v-card-title>Maintenance Mode</v-card-title>
					<v-card-text style="padding-bottom: 0px;">
						<p>
							This is used to disable other people from using the app while doing administrative tasks.
							When enabled, it gives people 5 minutes to finish any changes that are underway before 
							logging them out.
						</p>
						<p>
							<v-switch v-model="MaintenanceMode" label="Enable Maintenance Mode"></v-switch>
						</p>
					</v-card-text>
				</v-card>-->
				<BackupLocalDataCard style="margin: 30px; margin-top: 15px;" :disabled="GetDemoMode() || connectionStatus != 'Connected'" />
				<v-card style="margin: 30px; margin-top: 15px;">
					<v-card-title>Restore</v-card-title>
					<v-card-text>
						<p>
							Please contact support to restore the database.
						</p>
					</v-card-text>
				</v-card>
			</v-tab-item>
		</v-tabs-items>
		
		<div style="height: 50px;"></div>
		
		<!--<v-footer color="#747389" class="white--text" app inset>
			<v-row
			no-gutters
			>
				<v-btn
					color="white"
					text
					rounded
					class="my-2"
					>
					test
				</v-btn>
			</v-row>
		</v-footer>-->
	</div>
</template>

<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import { Component } from 'vue-property-decorator';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import _ from 'lodash';
import FormatCurrency from '@/Utility/Formatters/FormatCurrency';
import { BillingPackages } from '@/Data/Billing/BillingPackages/BillingPackages';
import ISO8601ToLocalDateOnly from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDateOnly';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { BillingJournalEntries } from '@/Data/Billing/BillingJournalEntries/BillingJournalEntries';
import { BillingInvoices } from '@/Data/Billing/BillingInvoices/BillingInvoices';
import { BillingContacts, IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingSubscriptions } from '@/Data/Billing/BillingSubscriptions/BillingSubscriptions';
import ViewBase from '@/Components/Views/ViewBase';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import store from '@/plugins/store/store';
import BilingContactListItem from '@/Components/ListItems/BillingContactListItem.vue';
import ModifyBillingContactDialogue2 from '@/Components/Dialogues2/BillingContacts/ModifyBillingContactDialogue2.vue';
import ModifyBillingContactPermissionsDialogue2 from '@/Components/Dialogues2/BillingContacts/ModifyBillingContactPermissionsDialogue2.vue';
import ModifyBillingContactLicensesDialogue2 from '@/Components/Dialogues2/BillingContacts/ModifyBillingContactLicensesDialogue2.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import DeleteBillingContactDialogue2 from '@/Components/Dialogues2/BillingContacts/DeleteBillingContactDialogue2.vue';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import CRMBackups from '@/Permissions/CRMBackups';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingCompanies } from '@/Data/Billing/BillingCompanies/BillingCompanies';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { BillingPaymentMethod } from '@/Data/Billing/BillingPaymentMethod/BillingPaymentMethod';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingPermissionsGroupsMemberships } from '@/Data/Billing/BillingPermissionsGroupsMemberships/BillingPermissionsGroupsMemberships';
import { BillingPermissionsGroups } from '@/Data/Billing/BillingPermissionsGroups/BillingPermissionsGroups';
import { BillingSubscriptionsProvisioningStatus } from '@/Data/Billing/BillingSubscriptionsProvisioningStatus/BillingSubscriptionsProvisioningStatus';
import BillingSubscriptionsCard from '@/Components/Cards/Billing/BillingSubscriptionsCard.vue';
import BillingInvoicesCard from '@/Components/Cards/Billing/BillingInvoicesCard.vue';
import BillingJournalEntriesCard from '@/Components/Cards/Billing/BillingJournalEntriesCard.vue';
import BillingPaymentsCard from '@/Components/Cards/Billing/BillingPaymentsCard.vue';
import BackupLocalDataCard from '@/Components/Cards/Backup/BackupLocalDataCard.vue';
import PhoneIdCard from '@/Components/Cards/PhoneIdCard.vue';

@Component({
	components: {
		ReloadButton,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		BilingContactListItem,
		ModifyBillingContactDialogue2,
		ModifyBillingContactPermissionsDialogue2,
		ModifyBillingContactLicensesDialogue2,
		DeleteBillingContactDialogue2,
		PermissionsDeniedAlert,
		NotificationBellButton,
		BillingSubscriptionsCard,
		BillingInvoicesCard,
		BillingJournalEntriesCard,
		BillingPaymentsCard,
		BackupLocalDataCard,
		PhoneIdCard,
	},
})
export default class Settings extends ViewBase {
	
	
	public $refs!: {
		newBillingContactDialogue: ModifyBillingContactDialogue2,
		modifyBillingContactDialogue: ModifyBillingContactDialogue2,
		modifyBillingContactPermissionsDialogue: ModifyBillingContactPermissionsDialogue2,
		modifyBillingContactLicensesDialogue: ModifyBillingContactLicensesDialogue2,
		deleteBillingContactDialogue: DeleteBillingContactDialogue2,
		
		
	};
	
	
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		'My Account': 0,
		'my account': 0,
		'Other Accounts': 1,
		'other accounts': 1,
		'Company Accounts': 1,
		'company accounts': 1,
		'Company Settings': 2,
		'company settings': 2,
		'Billing': 3,
		'billing': 3,
		'Admin, Backup & Restore': 4,
		'admin, backup & restore': 4,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'Settings',
			disabled: true,
			to: '/settings',
		},
	];
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected GetDemoMode = GetDemoMode;
	
	protected FormatCurrency = FormatCurrency;
	protected CurrentBillingContact = BillingContacts.ForCurrentSession;
	protected ISO8601ToLocalDateOnly = ISO8601ToLocalDateOnly;
	protected DialoguesOpen = Dialogues.Open;
	protected PermCRMCanModifySelf = BillingContacts.PermCRMCanModifySelf;
	protected PermBillingCompaniesCanModify = BillingCompanies.PermBillingCompaniesCanModify;
	protected PermCRMCanRequestSubscriptions = BillingSubscriptions.PermCRMCanRequestSubscriptions;
	protected PermCRMCanRequestInvoices = BillingInvoices.PermCRMCanRequestInvoices;
	protected PermCRMCanRequestJournalEntries = BillingJournalEntries.PermCRMCanRequestJournalEntries;
	protected PermCRMBackupsRunLocal = CRMBackups.PermCRMBackupsRunLocal;
	protected loadingData = false;
	
	protected newBillingContactModel: IBillingContacts | null = null;
	protected newBillingContactOpen = false;
	
	protected modifyBillingContactModel: IBillingContacts | null = null;
	protected modifyBillingContactOpen = false;
	
	protected modifyBillingContactPermissionsModel: IBillingContacts | null = null;
	protected modifyBillingContactPermissionsOpen = false;
	
	protected modifyBillingContactLicensesModel: IBillingContacts | null = null;
	protected modifyBillingContactLicensesOpen = false;
	
	protected deleteBillingContactModel: IBillingContacts | null = null;
	protected deleteBillingContactOpen = false;
	
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		
	}

	public LoadData(): void {
		
		if (GetDemoMode()) {
			return;
		}
		
		//console.debug('#1');
		
		SignalRConnection.Ready(() => {
			
			//console.debug('#2');
			
			BillingPermissionsBool.Ready(() => {
				
				const promises: Array<Promise<any>> = [];
				
				
				//console.debug('#3', BillingPermissionsBool.AllForBillingContactId());
				
				
				if (BillingCompanies.PermBillingCompaniesCanRequest()) {
					const rtr = BillingCompanies.RequestBillingCompanyForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				} else {
					console.warn('No permissions to request billing company.');
				}
				
				if (BillingContacts.PermBillingContactsCanRequest()) {
					const rtr = BillingContacts.RequestBillingContactsForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				if (Contact.PermContactsCanRequest()) {
					const rtr = Contact.RequestContacts.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingInvoices.PermBillingInvoicesCanRequest()) {
					const rtr = BillingInvoices.RequestBillingInvoicesForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingJournalEntries.PermCRMCanRequestJournalEntries()) {
					const rtr = BillingJournalEntries.RequestBillingJournalEntriesForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingPackages.PermBillingPackagesCanRequest()) {
					const rtr = BillingPackages.RequestBillingPackagesForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingPaymentMethod.PermBillingPaymentMethodCanRequest()) {
					const rtr = BillingPaymentMethod.RequestBillingPaymentMethodForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingPermissionsBool.PermBillingPermissionsBoolCanRequest()) {
					const rtr = BillingPermissionsBool.RequestBillingPermissionsBoolForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingPermissionsGroups.PermBillingPermissionsGroupsCanRequest()) {
					const rtr = BillingPermissionsGroups.RequestBillingPermissionsGroupsForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingPermissionsGroupsMemberships.PermBillingPermissionsGroupsMembershipsCanRequest()) {
					const rtr = BillingPermissionsGroupsMemberships.
						RequestBillingPermissionsGroupsMembershipsForCurrentSession.Send({
							sessionId: BillingSessions.CurrentSessionId(),
						});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingSessions.PermBillingSessionsCanRequest()) {
					const rtr = BillingSessions.RequestBillingSessionsForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingSubscriptions.PermCRMCanRequestSubscriptions()) {
					const rtr = BillingSubscriptions.RequestBillingSubscriptionsForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				if (BillingSubscriptionsProvisioningStatus.PermBillingSubscriptionsProvisioningStatusCanRequest()) {
					const rtr = BillingSubscriptionsProvisioningStatus.
						RequestBillingSubscriptionsProvisioningStatusForCurrentSession.Send({
							sessionId: BillingSessions.CurrentSessionId(),
						});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
				}
				
				//console.debug('#3', promises);
				
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
		
		console.log('settings mounted');
		
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
	
	
	// protected get UsedBillingAccounts(): number {
	// 	const accounts = BillingContacts.All();
	// 	if (accounts === null || Object.keys(accounts).length === 0) {
	// 		return 0;
	// 	}
	// 	return Object.keys(accounts).length;
	// }
	
	// protected get ProvisionedLicensedAccounts(): number {
		
	// 	const subs: IBillingSubscriptions[] = BillingSubscriptions.GetAll();
	// 	if (null === subs || subs.length === 0) {
	// 		return 0;
	// 	}
		
	// 	let count = 0;
		
	// 	for (const sub of subs) {
			
	// 		const pkgId = sub.packageId;
	// 		if (null === pkgId || IsNullOrEmpty(pkgId)) {
	// 			continue;
	// 		}
	// 		const pkg = BillingPackages.ForId(pkgId);
	// 		if (null === pkg) {
	// 			continue;
	// 		}
			
	// 		count += pkg.provisionDispatchPulseUsers;
	// 	}
		
	// 	return count;
	// }
	
	
	
	
	
	
	
	
	
	
	protected get MaintenanceMode(): boolean {
		return false;
	}
	
	protected set MaintenanceMode(flag: boolean) {
		
		console.debug('set MaintenanceMode ', flag);
		
	}
	
	protected get BillingContactName(): string | null {
		
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return '';
		}
		
		return contact.fullName;
	}
	
	protected set BillingContactName(payload: string | null) {

		BillingSessions.PushBillingSessionName.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			fullName: (payload == null || IsNullOrEmpty(payload)) ? 'Unnamed' : payload,
		});

	}
	
	
	
	
	
	protected get BillingContactPhone(): string | null {
		
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return '';
		}
		
		return contact.phone;
	}
	
	protected set BillingContactPhone(payload: string | null) {

		BillingSessions.PushBillingSessionPhone.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			phone: (payload == null || IsNullOrEmpty(payload)) ? '' : payload,
		});

	}
	
	protected get BillingContactEMail(): string | null {
		
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return '';
		}
		
		return contact.email;
	}
	
	protected set BillingContactEMail(payload: string | null) {

		BillingSessions.PushBillingSessionEMail.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			eMail: payload,
		});

	}
	
	
	protected get BillingContactEmailListTutorials(): boolean | null {
		
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return false;
		}
		
		return contact.emailListTutorials;
	}
	
	protected set BillingContactEmailListTutorials(payload: boolean | null) {

		BillingSessions.PushBillingSessionEMailListTutorials.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			eMailListTutorials: payload,
		});

	}
	
	
	protected get BillingContactEmailListMarketing(): boolean | null {
		
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return false;
		}
		
		return contact.emailListMarketing;
	}
	
	protected set BillingContactEmailListMarketing(payload: boolean | null) {

		BillingSessions.PushBillingSessionEMailListMarketing.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			eMailListMarketing: payload,
		});

	}
	
	protected get BillingContactUniqueID(): string {
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return '';
		}
		
		return contact.uuid;
	}
	
	protected set BillingContactUniqueID(payload: string) {
		console.error('set BillingContactUniqueID not implemented');
	}
	
	protected get BillingContactCompanyID(): string | null {
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return '';
		}
		
		return contact.companyId;
	}
	
	protected set BillingContactCompanyID(payload: string | null) {
		console.error('set BillingContactCompanyID not implemented');
	}
	
	protected get BillingContactMarketingCampaign(): string | null {
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return '';
		}
		
		return contact.marketingCampaign;
	}
	
	protected set BillingContactMarketingCampaign(payload: string | null) {
		// Not updatable by the user.
	}
	
	protected get BillingContactDispatchPulseContactId(): string | null {
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			return null;
		}
		
		const appData = contact.applicationData;
		if (!appData) {
			return null;
		}
		
		if (!appData.hasOwnProperty('dispatchPulseContactId')) {
			return null;
		}
		
		const contactId = appData.dispatchPulseContactId;
		if (!contactId || IsNullOrEmpty(contactId)) {
			return null;
		}
		
		
		return contactId;
	}
	
	protected set BillingContactDispatchPulseContactId(payload: string | null) {

		BillingSessions.PushBillingSessionContactId.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			contactId: payload,
		});

	}
	
	protected get BillingContactDispatchPulseAgentId(): string | null {
		return Agent.LoggedInAgentId();
	}
	
	protected set BillingContactDispatchPulseAgentId(newId: string | null) {
		
		BillingSessions.PushBillingSessionAgentId.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			agentId: newId,
		});
		
	}
	
	
	protected get CompanyAccounts(): Record<string, any> {
		
		return store.state.Database.billingContacts;
		
		
		
		
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected OpenNewBillingContact(): void {
		
		console.debug('OpenBewBillingContact');
		
		
		
		
		this.newBillingContactModel = BillingContacts.GetEmpty();
		this.newBillingContactModel.emailListMarketing = false;
		this.newBillingContactModel.emailListTutorials = false;
		this.newBillingContactModel.marketingCampaign = 'Client Added User';
		this.newBillingContactModel.companyId = BillingContacts.ForCurrentSession()?.companyId || null;
		this.newBillingContactOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.newBillingContactDialogue) {
				this.$refs.newBillingContactDialogue.SwitchToTabFromRoute();
			}
		});
		
	}
	
	protected CancelNewBillingContact(): void {
		
		//console.debug('CancelNewBillingContact');
		
		this.newBillingContactOpen = false;
		
	}
	
	protected SaveNewBillingContact(): void {
		
		console.debug('SaveNewBillingContact', this.newBillingContactModel);
		
		this.newBillingContactOpen = false;
		
		if (null == this.newBillingContactModel) {
			return;
		}
		
		
		const copy = _.cloneDeep(this.newBillingContactModel) as any;
		
		
		
		if (copy.hasOwnProperty('applicationData')) {
			copy.applicationData = JSON.stringify(copy.applicationData);
		}
		
		if (copy.hasOwnProperty('json')) {
			copy.json = JSON.stringify(copy.json);
		}
		
		BillingContacts.Add(copy);
	}
	
	
	
	
	
	
	
	
	
	
	protected OpenModifyBillingContact(id: string): void {
		console.debug('OpenModifyBillingContact', id);
		
		if (null === id || IsNullOrEmpty(id)) {
			return;
		}
		
		
		
		
		const toModify = _.cloneDeep(BillingContacts.ForId(id));
		
		
		this.modifyBillingContactModel = toModify;
		//this.modifyBillingContactModel.json.agentIds = [this.$route.params.id];
		this.modifyBillingContactOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.modifyBillingContactDialogue) {
				this.$refs.modifyBillingContactDialogue.SwitchToTabFromRoute();
			}
		});
		
	}
	
	protected CancelModifyBillingContact(): void {
		
		//console.debug('CancelModifyBillingContact');
		
		this.modifyBillingContactOpen = false;
		
	}
	
	protected SaveModifyBillingContact(): void {
		
		console.debug('SaveModifyBillingContact', this.modifyBillingContactModel);
		
		this.modifyBillingContactOpen = false;
		
		if (null == this.modifyBillingContactModel) {
			return;
		}
		
		
		const copy = _.cloneDeep(this.modifyBillingContactModel) as any;
		
		
		
		if (copy.hasOwnProperty('applicationData')) {
			copy.applicationData = JSON.stringify(copy.applicationData);
		}
		
		if (copy.hasOwnProperty('json')) {
			copy.json = JSON.stringify(copy.json);
		}
		
		const payload: Record<string, IBillingContacts> = {};
		payload[copy.uuid] = copy;
		
		
		
		BillingContacts.PerformUpdateBillingContactDetails.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			billingContacts: payload,
		});
		
		
		
	}
	
	
	
	
	
	
	
	
	
	
	
	
	protected OpenModifyBillingContactPermissions(id: string): void {
		console.debug('OpenModifyBillingContactPermissions', id);
		
		if (null === id || IsNullOrEmpty(id)) {
			return;
		}
		
		
		
		
		const toModify = _.cloneDeep(BillingContacts.ForId(id));
		
		
		this.modifyBillingContactPermissionsModel = toModify;
		//this.modifyBillingContactPermissionsModel.json.agentIds = [this.$route.params.id];
		this.modifyBillingContactPermissionsOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.modifyBillingContactPermissionsDialogue) {
				this.$refs.modifyBillingContactPermissionsDialogue.SwitchToTabFromRoute();
			}
		});
		
	}
	
	protected CloseModifyBillingContactPermissions(): void {
		
		//console.debug('CancelModifyBillingContact');
		
		this.modifyBillingContactPermissionsOpen = false;
		
	}
	
	protected SaveModifyBillingContactPermissions(): void {
		
		console.debug('SaveModifyBillingContactPermissions', this.modifyBillingContactPermissionsModel);
		
		if (null == this.modifyBillingContactPermissionsModel) {
			return;
		}
		
		
		const copy = _.cloneDeep(this.modifyBillingContactPermissionsModel) as any;
		
		
		
		if (copy.hasOwnProperty('applicationData')) {
			copy.applicationData = JSON.stringify(copy.applicationData);
		}
		
		if (copy.hasOwnProperty('json')) {
			copy.json = JSON.stringify(copy.json);
		}
		
		const payload: Record<string, IBillingContacts> = {};
		payload[copy.uuid] = copy;
		
		console.debug('payload', payload);
		
		BillingContacts.PerformUpdateBillingContactDetails.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			billingContacts: payload,
		});
		
		
		
	}
	
	
	
	
	
	
	
	protected OpenModifyBillingContactLicenses(id: string): void {
		console.debug('OpenModifyBillingContactLicenses', id);
		
		if (null === id || IsNullOrEmpty(id)) {
			return;
		}
		
		
		
		
		const toModify = _.cloneDeep(BillingContacts.ForId(id));
		
		
		this.modifyBillingContactLicensesModel = toModify;
		//this.modifyBillingContactLicensesModel.json.agentIds = [this.$route.params.id];
		this.modifyBillingContactLicensesOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.modifyBillingContactLicensesDialogue) {
				this.$refs.modifyBillingContactLicensesDialogue.SwitchToTabFromRoute();
			}
		});
		
	}
	
	protected CloseModifyBillingContactLicenses(): void {
		
		//console.debug('CancelModifyBillingContact');
		
		this.modifyBillingContactLicensesOpen = false;
		
	}
	
	protected SaveModifyBillingContactLicenses(): void {
		
		console.debug('SaveModifyBillingContactLicenses', this.modifyBillingContactLicensesModel);
		
		if (null == this.modifyBillingContactLicensesModel) {
			return;
		}
		
		
		const copy = _.cloneDeep(this.modifyBillingContactLicensesModel) as any;
		
		
		
		if (copy.hasOwnProperty('applicationData')) {
			copy.applicationData = JSON.stringify(copy.applicationData);
		}
		
		if (copy.hasOwnProperty('json')) {
			copy.json = JSON.stringify(copy.json);
		}
		
		const payload: Record<string, IBillingContacts> = {};
		payload[copy.uuid] = copy;
		
		
		
		BillingContacts.PerformUpdateBillingContactDetails.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			billingContacts: payload,
		});
		
		
		
	}
	
	
	
	protected OpenDeleteBillingContact(obj: IBillingContacts): void {
		
		if (null === obj) {
			return;
		}
		
		console.debug('OpenDeleteBillingContact', obj);
		
		this.deleteBillingContactModel = obj;
		this.deleteBillingContactOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.deleteBillingContactDialogue) {
				this.$refs.deleteBillingContactDialogue.SwitchToTabFromRoute();
			}
		});
		
	}
	
	protected CancelDeleteBillingContact(): void {
		
		//console.debug('CancelDeleteBillingContact');
		
		this.deleteBillingContactOpen = false;
		
	}
	
	protected SaveDeleteBillingContact(): void {
		
		console.debug('SaveDeleteBillingContact', this.deleteBillingContactModel);
		
		this.deleteBillingContactOpen = false;
		
		if (null == this.deleteBillingContactModel ||
			null == this.deleteBillingContactModel.uuid ||
			IsNullOrEmpty(this.deleteBillingContactModel.uuid)
			) {
			return;
		}
		
		const rtr = BillingContacts.DeleteId(this.deleteBillingContactModel.uuid);
		if (rtr?.completeRequestPromise) {
			rtr?.completeRequestPromise.then(() => {
				this.deleteBillingContactOpen = false;
				this.deleteBillingContactModel = null;
			});
		}
		
		
		
		
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
}
</script>
<style scoped>
.title { color: black; }
.subtitle-1 { color: black; }
p { color: black; }
</style>