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
			
			<v-toolbar-title class="white--text">Product Definitions</v-toolbar-title>

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
					<v-list-item
						@click="CSVDownloadProducts()"
						:disabled="!PermCRMExportProductsCSV()"
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

					<v-tab @click="$router.replace({query: { ...$route.query, tab: 'Products'}}).catch(((e) => {}));">
						Products
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
				
				
				
				
				<ProductsDataTable
					ref="dataTable"
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
						@click="AddProduct()"
						class="e2e-products-definitons-add-product"
						:disabled="connectionStatus != 'Connected' || !PermProductsCanPush()"
						>
						<v-list-item-icon>
							<v-icon>local_grocery_store</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Product</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>
				
			</v-row>
		</v-footer>
		
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
import ProductsDataTable from '@/Components/DataTables/ProductsDataTable.vue';
import CSVDownloadProducts from '@/Data/CRM/Product/CSVDownloadProducts';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { Product } from '@/Data/CRM/Product/Product';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		AddMenuButton,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ProductsDataTable,
		ReloadButton,
		NotificationBellButton,
	},
})
export default class MaterialTypesSettings extends ViewBase {
	
	public $refs!: {
		dataTable: ProductsDataTable,
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
			text: 'Product Definitions',
			disabled: true,
			to: '/section/product-definitions',
		},
	];
	
	protected PermProductsCanPush = Product.PermProductsCanPush;
	protected CSVDownloadProducts = CSVDownloadProducts;
	protected PermCRMExportProductsCSV = Product.PermCRMExportProductsCSV;
	protected loadingData = false;
	
	constructor() {
		super();
		
	}
	
	

	public get IsLoadingData(): boolean {
		
		if (this.$refs.dataTable && this.$refs.dataTable.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}

	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.dataTable) {
			this.$refs.dataTable.LoadData();
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
	
	protected AddProduct(): void {
		Dialogues.Open({ name: 'ModifyProductDialogue', state: null});
	}
	
}
</script>
