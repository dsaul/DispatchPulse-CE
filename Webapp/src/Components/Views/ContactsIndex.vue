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
			
			<v-toolbar-title class="white--text">All Contacts</v-toolbar-title>

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
						@click="CSVDownloadContacts()"
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
						@click="$router.replace({query: { ...$route.query, tab: 'Contacts'}}).catch(((e) => {}));"
						class="e2e-contacts-index-tab-contacts"
						>
						Contacts
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
				<ContactList
					ref="contactList"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
		</v-tabs-items>
		
		<div style="height: 50px;"></div>
		
		<v-footer color="#747389" class="white--text" app inset>
			<v-row
				no-gutters
				>
				<v-spacer />
				
				<AddMenuButton
					:disabled="connectionStatus != 'Connected'"
					>
					<v-list-item
						@click="AddContact()"
						class="e2e-add-menu-add-contact"
						:disabled="connectionStatus != 'Connected' || !PermContactsCanPush()"
						>
						<v-list-item-icon>
							<v-icon>person</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Contact</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>
				
			</v-row>
		</v-footer>
		
	</div>
</template>

<script lang="ts">

import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import ContactList from '@/Components/Lists/ContactList.vue';
import CompanyList from '@/Components/Lists/CompanyList.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import CSVDownloadContacts from '@/Data/CRM/Contact/CSVDownloadContacts';
import Dialogues from '@/Utility/Dialogues';
import ViewBase from '@/Components/Views/ViewBase';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { Contact } from '@/Data/CRM/Contact/Contact';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		AddMenuButton,
		CompanyList,
		ContactList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
	},
})
export default class ContactsIndex extends ViewBase {
	
	public $refs!: {
		contactList: ContactList,
	};
	
	
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		Contacts: 0,
		contacts: 0,
		Companies: 1,
		companies: 1,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'All Contacts',
			disabled: true,
			to: '/section/contacts/index',
		},
	];
	
	protected CSVDownloadContacts = CSVDownloadContacts;
	protected PermContactsCanPush = Contact.PermContactsCanPush;
	protected PermCRMReportContactsPDF = Contact.PermCRMReportContactsPDF;
	protected PermCRMExportContactsCSV = Contact.PermCRMExportContactsCSV;
	protected PermContactsCanDelete = Contact.PermContactsCanDelete;
	
	protected loadingData = false;
	
	
	
	
	public get IsLoadingData(): boolean {
		
		if (this.$refs.contactList && this.$refs.contactList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.contactList) {
			this.$refs.contactList.LoadData();
		}
		
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
		this.SwitchToTabFromRoute();
		this.LoadData();
		//console.log('contactsindex mounted', this.$refs);
		
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
	
	protected DoPrint(): void {
		
		Dialogues.Open({ 
			name: 'ContactsReportDialogue', 
			state: {
				allLoadedContacts: true,
			},
		});
		
	}
	
	protected AddContact(): void {
		Dialogues.Open({ name: 'ModifyContactDialogue', state: null});
	}
	
}
</script>
