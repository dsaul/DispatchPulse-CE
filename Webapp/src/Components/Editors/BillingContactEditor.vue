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
					General
				</v-tab>
			</v-tabs>
				
			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<v-card flat>
						
						<v-form ref="generalForm" autocomplete="newpassword">
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Basic</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="FullName"
											autocomplete="newpassword"
											label="Full Name"
											hint="The full name for this account."
											:rules="[
												ValidateRequiredField
											]"
											class="e2e-billing-contact-editor-name"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="EMail"
											autocomplete="newpassword"
											label="E-Mail"
											hint="Used to login."
											:rules="[
												ValidateRequiredField
											]"
											class="e2e-billing-contact-editor-email"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Password</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="Password"
											autocomplete="newpassword"
											label="Password"
											hint="Password used to log into Dispatch Pulse."
											:rules="[
												ValidateRequiredField
											]"
											:disabled="connectionStatus != 'Connected'"
											type="password"
											>
										</v-text-field>
									</v-col>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="PasswordAgain"
											autocomplete="newpassword"
											label="Password (again)"
											hint="Retype the password."
											:rules="[
												ValidateRequiredField
											]"
											:disabled="connectionStatus != 'Connected'"
											type="password"
											>
										</v-text-field>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Associated Dispatch Pulse Information</div>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<ContactSelectField 
											:disabled="GetDemoMode() || connectionStatus != 'Connected'"
											v-model="BillingContactDispatchPulseContactId" 
											:showDetails="false"
											:isDialogue="isDialogue"
											/>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<AgentSelectField 
											:disabled="GetDemoMode() || connectionStatus != 'Connected'"
											v-model="BillingContactDispatchPulseAgentId" 
											:showDetails="false"
											:isDialogue="isDialogue"
											/>
									</v-col>
								</v-row>
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Communication Preferences</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-switch
											v-model="MarketingEmails"
											label="Marketing Emails"
											/>
										<v-switch
											v-model="TutorialEmails"
											label="Tutorial Emails"
											/>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="Phone"
											autocomplete="newpassword"
											label="Phone Number"
											hint=""
											:rules="[
												ValidateRequiredField
											]"
											class="e2e-billing-contact-editor-phone"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
								</v-row>
								
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Advanced</div>
									</v-col>
								</v-row>
								
								
								
								
								
								
								
								
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="Id"
											readonly="readonly"
											label="Unique ID"
											hint="The id of this contact."
											>
										</v-text-field>
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
import GenerateID from '@/Utility/GenerateID';
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
export default class BillingContactEditor extends EditorBase {

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
	
	public ResetPasswordToDefault(): void {
		this.password1 = this.password1Initial;
		this.password2 = this.password2Initial;
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
		
		this.ResetPasswordToDefault();
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