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
			
			<v-toolbar-title class="white--text">Contact<span v-if="Name">: {{Name}}</span></v-toolbar-title>

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
						<v-list-item-title>Contact Tutorial Pages</v-list-item-title>
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
					<v-list-item
						@click="DoPrint()"
						:disabled="connectionStatus != 'Connected' || !PermCRMReportContactsPDF()"
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
						:disabled="!PermCRMExportContactsCSV()"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
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
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'General'}}).catch(((e) => {}));"
						>
						General
					</v-tab>
					<v-tab
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'Projects'}}).catch(((e) => {}));"
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
						<div class="title">Contact Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The contact no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the contact while you were opening it.</li>
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
			>
				<v-tab>
					General
				</v-tab>
				<v-tab :disabled="isMakingNew">
					Projects
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
											v-model="Name"
											autocomplete="newpassword"
											label="Name"
											hint="The name of this contact."
											:rules="[
												ValidateRequiredField
											]"
											class="e2e-contact-editor-name"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermContactsCanPush()"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-combobox
											label="Title"
											id="Title"
											v-model="Title"
											auto-select-first
											hint="This contact's title, for example: Journeyman Electrician"
											:items="$store.getters.SortedDeduplicatedContactTitles"
											class="e2e-contact-editor-title"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermContactsCanPush()"
											>
										</v-combobox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<CompanySelectField
											:isDialogue="isDialogue"
											v-model="CompanyId"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermContactsCanPush()"
											/>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-textarea
											label="Notes"
											hint="Additional information about this contact."
											v-model="Notes"
											class="e2e-contact-editor-notes"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermContactsCanPush()"
											></v-textarea>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Phone</div>
									</v-col>
								</v-row>
								
								<PhoneNumberEditRowArrayAdapter
									v-model="PhoneNumbers"
									:disabled="connectionStatus != 'Connected'"
									:readonly="!PermContactsCanPush()"
									/>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Address</div>
									</v-col>
								</v-row>
								
								
								<AddressEditRowArrayAdapter
									v-model="Addresses"
									:disabled="connectionStatus != 'Connected'"
									:readonly="!PermContactsCanPush()"
									/>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">E-Mail</div>
									</v-col>
								</v-row>
								
								<EMailEditRowArrayAdapter
									v-model="EMails"
									:disabled="connectionStatus != 'Connected'"
									:readonly="!PermContactsCanPush()"
									/>
								
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
				<v-tab-item style="flex: 1;">
					<v-alert
						border="top"
						colored-border
						type="info"
						elevation="2"
						style="margin: 15px; padding-bottom: 10px;"
						>
						Projects where {{Name}} is listed as a contact.
					</v-alert>
					<ProjectList
						:showOnlyContactId="$route.params.id"
						:isDialogue="isDialogue"
						/>
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
					:disabled="!value || connectionStatus != 'Connected' || !PermContactsCanDelete()"
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
import { Component, Vue, Prop } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import GenerateID from '@/Utility/GenerateID';
import PhoneNumberEditRowArrayAdapter from '@/Components/Rows/PhoneNumberEditRowArrayAdapter.vue';
import EMailEditRowArrayAdapter from '@/Components/Rows/EMailEditRowArrayAdapter.vue';
import AddressEditRowArrayAdapter from '@/Components/Rows/AddressEditRowArrayAdapter.vue';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import CSVDownloadContact from '@/Data/CRM/Contact/CSVDownloadContact';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { Contact, IContact } from '@/Data/CRM/Contact/Contact';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { IPhoneNumber } from '@/Data/Models/PhoneNumber/PhoneNumber';
import { IAddress } from '@/Data/Models/Address/Address';
import { IEMail } from '@/Data/Models/EMail/EMail';

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
export default class ContactEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IContact | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		generalForm: Vue,
	};
	
	protected ValidateRequiredField = ValidateRequiredField;
	protected CSVDownloadContact = CSVDownloadContact;
	protected DialoguesOpen = Dialogues.Open;
	protected PermCRMReportContactsPDF = Contact.PermCRMReportContactsPDF;
	protected PermCRMExportContactsCSV = Contact.PermCRMExportContactsCSV;
	protected PermContactsCanDelete = Contact.PermContactsCanDelete;
	protected PermContactsCanPush = Contact.PermContactsCanPush;
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
			Projects: 1,
			projects: 1,
		};
	}
	
	protected get PhoneNumbers(): IPhoneNumber[] {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.phoneNumbers
			) {
			return [];
		}
		
		return this.value.json.phoneNumbers;
	}
	
	protected set PhoneNumbers(val: IPhoneNumber[]) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.phoneNumbers = val;
		this.SignalChanged();
	}
	
	
	
	
	protected get Addresses(): IAddress[] {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.addresses
			) {
			return [];
		}
		
		return this.value.json.addresses;
	}
	
	protected set Addresses(val: IAddress[]) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.addresses = val;
		this.SignalChanged();
	}
	
	protected get EMails(): IEMail[] {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.emails
			) {
			return [];
		}
		
		return this.value.json.emails;
	}
	
	protected set EMails(val: IEMail[]) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.emails = val;
		this.SignalChanged();
	}
	
	protected get Name(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.name
			) {
			return null;
		}
		
		return this.value.json.name;
	}
	
	protected set Name(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.name = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	
	protected get Title(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.title
			) {
			return null;
		}
		
		return this.value.json.title;
	}
	
	protected set Title(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.title = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
			) {
			return null;
		}
		
		return this.value.id;
	}
	
	protected get CompanyId(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.companyId
			) {
			return null;
		}
		
		return this.value.json.companyId;
	}
	
	protected set CompanyId(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.companyId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get Notes(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.notes
			) {
			return null;
		}
		
		return this.value.json.notes;
	}
	
	protected set Notes(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.notes = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
		
	}
	
	protected AddPhoneNumber(): void {
		//console.debug('AddPhoneNumber()');
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.phoneNumbers
			) {
			return;
		}
		
		const entry: IPhoneNumber = {
			id: GenerateID(),
			label: '',
			value: '',
		};
		
		this.value.json.phoneNumbers.push(entry);
		this.SignalChanged();
		
	}
	
	protected AddEMail(): void {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.emails
			) {
			return;
		}
		
		
		const entry = {
			id: GenerateID(),
			label: '',
			value: '',
		};
		
		this.value.json.emails.push(entry);
		this.SignalChanged();
		
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
	
	
	
	
	
	
	protected DoDelete(): void {
		Dialogues.Open({ name: 'DeleteContactDialogue', state: {
			redirectToIndex: true,
			id: this.value?.id,
		}});
	}
	
	protected DoPrint(): void {
		
		Dialogues.Open({ 
			name: 'ContactsReportDialogue', 
			state: {
				allLoadedContacts: false,
				specificContacts: [{
					id: GenerateID(),
					value: this.value?.id,
					label: '',
				}],
			},
		});
		
	}
	
	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');
		
		window.open(
			'https://www.dispatchpulse.com/Support',
			'_blank');
	}
	
	
	
	
	
	//
}

</script>
<style scoped>
</style>