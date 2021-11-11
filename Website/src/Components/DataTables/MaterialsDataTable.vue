<template>
	<v-card flat>
				
		<v-text-field
			autocomplete="newpassword"
			class="mx-4 e2e-materials-data-table-filter"
			v-model="searchString"
			hide-details
			label="Filter"
			prepend-inner-icon="search"
			solo
			style="margin-bottom: 10px;"
		></v-text-field>
		
		
		<v-data-table
			v-if="PermMaterialsCanRequest()"
			:headers="headers"
			:items="AllMaterials"
			:search="searchString"
			item-key="name"
			:custom-filter="CustomDataTableFilter"
			ref="dataTable"
			:footer-props="{
			showFirstLastPage: true,
			firstIcon: 'mdi-arrow-collapse-left',
			lastIcon: 'mdi-arrow-collapse-right',
			prevIcon: 'chevron_left',
			nextIcon: 'chevron_right',
			'items-per-page-options': [5,10,15,50],
			}"
			>
			<template v-slot:[`item.json.quantity`]="{ item }">
				<span>{{ item.json.quantity }} {{item.json.quantityUnit || '&#x274C;'}}</span>
			</template>
			<template v-slot:[`item.json.projectId`]="{ item }">
				<DataLoaderProject :projectId="item.json.projectId">
					<router-link
						v-if="item && item.json && item.json.projectId"
						:to="`/section/projects/${item.json.projectId}?tab=Materials`"
						>
						{{ProjectCombinedDescriptionForId(item.json.projectId)}}
					</router-link>
				</DataLoaderProject>
			</template>
			<template v-slot:[`item.json.dateUsedISO8601`]="{ item }">
				{{ ISO8601ToLocalDateOnly(item.json.dateUsedISO8601) }}
			</template>
			<template v-slot:[`item.json.productId`]="{ item }">
				<DataLoaderProduct :productId="item.json.productId">
					{{ ProductNameForId(item.json.productId) }}
				</DataLoaderProduct>
			</template>
			<template v-slot:[`item.json.isBilled`]="{ item }">
				<span v-if="item.json.isBilled">&#x2714;</span>
				<span v-else>&#x274C;</span>
			</template>
			<template v-slot:[`item.json.isExtra`]="{ item }">
				<div v-if="item.json.isExtra">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.action`]="{ item }">
				<v-menu bottom left>
					<template v-slot:activator="{ on }">
						<v-btn
							icon
							v-on="on"
							:disabled="disabled"
							>
							<v-icon>more_vert</v-icon>
						</v-btn>
					</template>
					<v-list dense>
						<v-list-item
							@click="EditEntry(item)"
							:disabled="disabled || !PermMaterialsCanPush()"
							>
							<v-list-item-icon>
								<v-icon>edit</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Edit…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item
							@click="DeleteEntry(item)"
							:disabled="disabled || !PermMaterialsCanDelete()"
							>
							<v-list-item-icon>
								<v-icon>delete</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Delete…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
					</v-list>
				</v-menu>
			</template>
		</v-data-table>
		<PermissionsDeniedAlert v-else />
		<v-spacer style="height:40px;"></v-spacer>
	</v-card>
	
</template>
<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import { Component, Vue } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import DataTableBase from './DataTableBase';
import ResizeObserver from 'resize-observer-polyfill';
import { Project } from '@/Data/CRM/Project/Project';
import DataLoaderProject from '@/Components/DataLoaders/DataLoaderProject.vue';
import DataLoaderProduct from '@/Components/DataLoaders/DataLoaderProduct.vue';
import SignalRConnection from '@/RPC/SignalRConnection';
import { IMaterial, Material } from '@/Data/CRM/Material/Material';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({
	components: {
		DataLoaderProject,
		DataLoaderProduct,
		PermissionsDeniedAlert,
	},
})
export default class MaterialsDataTable extends DataTableBase {
	
	public $refs!: {
		dataTable: Vue,
	};
	
	protected headers = [
		{
			text: 'Date Used',
			value: 'json.dateUsedISO8601',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = '' + a;
				const descB = '' + b;
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Project',
			value: 'json.projectId',
			align: 'left',
			sort: (a: string, b: string): number => {
				const descA = Project.CombinedDescriptionForId(a);
				const descB = Project.CombinedDescriptionForId(b);
				//console.log('sort', descA, descB);
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Quantity',
			value: 'json.quantity',
			align: 'left',
			sortable: false,
		},
		{
			text: 'Product',
			value: 'json.productId',
			align: 'left',
			sort: (a: string, b: string): number => {
				const descA = this.ProductNameForId(a) || '';
				const descB = this.ProductNameForId(b) || '';
				//console.log('sort', descA, descB);
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Extra',
			value: 'json.isExtra',
			align: 'center',
			sort: (a: boolean | null, b: boolean | null): number => {
				const descA = a ? 'yes' : 'no';
				const descB = b ? 'yes' : 'no';
				//console.log('sort', descA, descB);
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Billed',
			value: 'json.isBilled',
			align: 'center',
			sort: (a: boolean | null, b: boolean | null): number => {
				const descA = a ? 'yes' : 'no';
				const descB = b ? 'yes' : 'no';
				//console.log('sort', descA, descB);
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Location',
			value: 'json.location',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = '' + a;
				const descB = '' + b;
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Actions',
			value: 'action',
			align: 'right',
			sortable: false,
			filterable: false,
		},
	];
	protected searchString = '';
	protected PermMaterialsCanRequest = Material.PermMaterialsCanRequest;
	protected PermMaterialsCanDelete = Material.PermMaterialsCanDelete;
	protected PermMaterialsCanPush = Material.PermMaterialsCanPush;
	protected get AllMaterials(): IMaterial[] {
		return Object.values<IMaterial>(this.$store.state.Database.materials);
	}
	
	protected resizeObserver: ResizeObserver | null = null;
	protected closestMain: Element | null = null;
	
	protected loadingData = false;
	
	public beforeDestroy(): void {
		if (this.resizeObserver && this.closestMain) {
			this.resizeObserver.unobserve(this.closestMain);
		}
	}
	
	public mounted(): void {
		this.resizeObserver = new ResizeObserver((entries, observer) => {
			this.ResizeTriggered(entries, observer);
		});
		
		if (this.$el) {
			this.closestMain = this.$el.closest('.v-content__wrap');
			
			if (this.closestMain) {
				this.resizeObserver.observe(this.closestMain);
			}
			
		}
		
		this.LoadData();
	}
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				const promises: Array<Promise<any>> = [];
				
				if (Material.PermMaterialsCanRequest()) {
					
					const rtr = Material.RequestMaterials.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
					if (rtr.completeRequestPromise) {
						promises.push(rtr.completeRequestPromise);
					}
					
				}
				
				if (promises.length > 0) {
					
					this.loadingData = true;
					
					Promise.all(promises).finally(() => {
						this.loadingData = false;
					});
				}
				
				
			});
		});
		
	}
	
	// eslint-disable-next-line 
	protected CustomDataTableFilter(value: any, search: string | null, item: IMaterial): boolean {
		//console.log(`CustomDataTableFilter '${search}'`);
		
		if (search == null || IsNullOrEmpty(search)) {
			return true;
		}
		
		let haystack: string | null = null;
		let index: number | null = null;
		
		// Date
		if (item.json.dateUsedISO8601 && !IsNullOrEmpty(item.json.dateUsedISO8601)) {
			haystack = '' + this.ISO8601ToLocalDateOnly(item.json.dateUsedISO8601);
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}
		
		// Project
		if (item.json.projectId && !IsNullOrEmpty(item.json.projectId)) {
			haystack = '' + Project.CombinedDescriptionForId(item.json.projectId);
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}
		
		// Quantity
		haystack = '' + item.json.quantity + ' ' + item.json.quantityUnit;
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Product
		haystack = '' + this.ProductNameForId(item.json.productId);
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Location
		haystack = '' + item.json.location;
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Extra
		haystack = '' + (item.json.isExtra ? 'extra' : '');
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Billed
		haystack = '' + (item.json.isBilled ? 'extra' : 'billed');
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		return false;
	}
	
	
	
	
	protected EditEntry(val: IMaterial): void {
		//console.log('EditEntry', id);
		
		Dialogues.Open({ 
			name: 'ModifyMaterialDialogue', 
			state: val,
		});
	}
	
	protected DeleteEntry(val: IMaterial): void {
		//console.log('DeleteEntry', id);
		
		Dialogues.Open({ 
			name: 'DeleteMaterialDialogue', 
			state: {
				redirectToIndex: false,
				id: val.id,
			},
		});
	}
	
	
	
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
protected ResizeTriggered(entries: ResizeObserverEntry[], observer: ResizeObserver): void {
		//console.debug('ResizeTriggered()');
		
		if (!this.closestMain) {
			return;
		}
		
		const rect: DOMRect = this.closestMain.getBoundingClientRect();
		const width: number = Math.floor(rect.width);
		
		if (this.$refs.dataTable && this.$refs.dataTable.$el) {
			const e = this.$refs.dataTable.$el as HTMLElement;
			e.style.width = `${width || 0}px`;
		}
		
		
		
	}
	
	
	
}

</script>
