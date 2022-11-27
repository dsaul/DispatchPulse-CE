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
						@click="$router.replace({query: { ...$route.query, tab: 'Licenses'}}).catch(((e) => {}));"
						>
						Licenses
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
					Licenses
				</v-tab>
			</v-tabs>
				
			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<v-card flat>
						
						<v-form ref="generalForm" autocomplete="newpassword">
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Licensed Products</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-switch
											v-model="LicencedProjectsSchedulingTime"
											label="Projects, Scheduling, and Time"
											:disabled="!LicencedProjectsSchedulingTime && UsedProjectsSchedulingTimeLicenses >= TotalProjectsSchedulingTimeLicenses"
											:hint="`You are currently using ${this.UsedProjectsSchedulingTimeLicenses} out of a max of ${this.TotalProjectsSchedulingTimeLicenses} projects, scheduling, and time licenses.`"
											persistent-hint
											/>
									</v-col>
									
									
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-switch
											v-model="LicencedOnCall"
											label="On Call Responder"
											:disabled="!LicencedOnCall && UsedOnCallLicenses >= TotalOnCallLicenses"
											:hint="`You are currently using ${this.UsedOnCallLicenses} out of a max of ${this.TotalOnCallLicenses} on-call responder licenses.`"
											persistent-hint
											/>
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
import PhoneNumberEditRowArrayAdapter from '@/Components/Rows/PhoneNumberEditRowArrayAdapter.vue';
import EMailEditRowArrayAdapter from '@/Components/Rows/EMailEditRowArrayAdapter.vue';
import AddressEditRowArrayAdapter from '@/Components/Rows/AddressEditRowArrayAdapter.vue';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import CSVDownloadContact from '@/Data/CRM/Contact/CSVDownloadContact';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import PermissionsKeyToDisplayName from '@/Utility/PermissionsKeyToDisplayName';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Agent } from '@/Data/CRM/Agent/Agent';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingSubscriptions, IBillingSubscriptions } from '@/Data/Billing/BillingSubscriptions/BillingSubscriptions';
import { BillingPackages } from '@/Data/Billing/BillingPackages/BillingPackages';
import { BillingContacts, IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';

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
export default class BillingContactLicensesEditor extends EditorBase {

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
	
	protected get LicencedProjectsSchedulingTime(): boolean {
		
		if (!this.value || 
			!this.value.json ||
			!this.value.json.licenseAssignedProjectsSchedulingTime) {
			return false;
		}
		
		return this.value.json.licenseAssignedProjectsSchedulingTime;
	}
	
	protected set LicencedProjectsSchedulingTime(flag: boolean) {
		
		if (!this.value) {
			console.error('set LicencedProjectsSchedulingTime !this.value');
			return;
		}
		
		if (!this.value.json) {
			this.value.json = {};
		}
		
		this.value.json.licenseAssignedProjectsSchedulingTime = flag;
		
		this.SignalChanged();
	}
	
	protected get UsedProjectsSchedulingTimeLicenses(): number {
		const allAccounts = BillingContacts.All();
		
		if (!allAccounts ||
			Object.keys(allAccounts).length === 0) {
			return 0;
		}
		
		let i = 0;
		for (const account of Object.values(allAccounts)) {
			if (account.json.licenseAssignedProjectsSchedulingTime) {
				i++;
			}
		}
		
		return i;
	}
	
	protected get TotalProjectsSchedulingTimeLicenses(): number {
		
		const subs: IBillingSubscriptions[] = BillingSubscriptions.GetAll();
		if (null === subs || subs.length === 0) {
			return 0;
		}
		
		let count = 0;
		
		for (const sub of subs) {
			
			const pkgId = sub.packageId;
			if (null === pkgId || IsNullOrEmpty(pkgId)) {
				continue;
			}
			const pkg = BillingPackages.ForId(pkgId);
			if (null === pkg) {
				continue;
			}
			
			count += pkg.provisionDispatchPulseUsers;
		}
		
		return count;
		
	}
	
	protected get UsedOnCallLicenses(): number {
		const allAccounts = BillingContacts.All();
		
		if (!allAccounts ||
			Object.keys(allAccounts).length === 0) {
			return 0;
		}
		
		let i = 0;
		for (const account of Object.values(allAccounts)) {
			if (account.json.licenseAssignedOnCall) {
				i++;
			}
		}
		
		return i;
	}
	
	protected get TotalOnCallLicenses(): number {
		
		const subs: IBillingSubscriptions[] = BillingSubscriptions.GetAll();
		if (null === subs || subs.length === 0) {
			return 0;
		}
		
		let count = 0;
		
		for (const sub of subs) {
			
			const pkgId = sub.packageId;
			if (null === pkgId || IsNullOrEmpty(pkgId)) {
				continue;
			}
			const pkg = BillingPackages.ForId(pkgId);
			if (null === pkg) {
				continue;
			}
			
			if (!pkg.json || !pkg.json.ProvisionOnCallUsers) {
				continue;
			}
			
			count += pkg.json.ProvisionOnCallUsers;
		}
		
		return count;
		
	}
	
	
	
	
	
	
	
	
	
	protected get LicencedOnCall(): boolean {
		
		if (!this.value || 
			!this.value.json ||
			!this.value.json.licenseAssignedOnCall) {
			return false;
		}
		
		return this.value.json.licenseAssignedOnCall;
	}
	
	protected set LicencedOnCall(flag: boolean) {
		
		if (!this.value) {
			console.error('set LicencedOnCall !this.value');
			return;
		}
		
		if (!this.value.json) {
			this.value.json = {};
		}
		
		this.value.json.licenseAssignedOnCall = flag;
		
		this.SignalChanged();
	}
	
	
	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');
		
		window.open(
			'https://www.dispatchpulse.com/support/',
			'_blank');
	}
	
}

</script>
<style scoped>
</style>