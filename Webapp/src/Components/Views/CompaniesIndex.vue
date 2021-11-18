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
			
			<v-toolbar-title class="white--text">All Companies</v-toolbar-title>

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
						:disabled="connectionStatus != 'Connected' || !PermCRMReportCompaniesPDF()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="CSVDownloadCompanies()"
						:disabled="!PermCRMExportCompaniesCSV()"
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
						@click="$router.replace({query: { ...$route.query, tab: 'Companies'}}).catch(((e) => {}));"
						class="e2e-companies-index-tab-companies"
						>
						Companies
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
				<CompanyList 
					ref="companyList"
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
						@click="AddCompany()"
						class="e2e-add-menu-add-company"
						:disabled="connectionStatus != 'Connected' || !PermCompaniesCanPush()"
						>
						<v-list-item-icon>
							<v-icon>domain</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Company</v-list-item-title>
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
import CompanyList from '@/Components/Lists/CompanyList.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import CSVDownloadCompanies from '@/Data/CRM/Company/CSVDownloadCompanies';
import Dialogues from '@/Utility/Dialogues';
import ViewBase from '@/Components/Views/ViewBase';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { Company } from '@/Data/CRM/Company/Company';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		AddMenuButton,
		CompanyList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
	},
})
export default class CompaniesIndex extends ViewBase {
	
	public $refs!: {
		companyList: CompanyList,
	};
	
	
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		Companies: 0,
		companies: 0,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'All Companies',
			disabled: true,
			to: '/section/companies/index',
		},
	];
	
	
	protected CSVDownloadCompanies = CSVDownloadCompanies;
	protected PermCompaniesCanPush = Company.PermCompaniesCanPush;
	protected PermCRMReportCompaniesPDF = Company.PermCRMReportCompaniesPDF;
	protected PermCRMExportCompaniesCSV = Company.PermCRMExportCompaniesCSV;
	
	protected loadingData = false;
	
	
	public get IsLoadingData(): boolean {
		
		
		if (this.$refs.companyList && this.$refs.companyList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.companyList) {
			this.$refs.companyList.LoadData();
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
			name: 'CompanyReportDialogue', 
			state: {
				allLoadedCompanies: true,
			},
		});
		
	}
	
	protected AddCompany(): void {
		Dialogues.Open({ name: 'ModifyCompanyDialogue', state: null});
	}
	
}
</script>
