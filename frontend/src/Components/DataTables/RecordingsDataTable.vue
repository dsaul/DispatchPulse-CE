<template>
	<v-card flat>
				
		<v-text-field
			autocomplete="newpassword"
			class="mx-4"
			v-model="searchString"
			hide-details
			label="Filter"
			prepend-inner-icon="search"
			solo
			style="margin-bottom: 10px;"
		></v-text-field>
		
		
		<v-data-table
			v-if="PermRecordingsCanRequest()"
			:headers="headers"
			:items="AllRecordings"
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
			<template v-slot:[`item.json.name`]="{ item }">
				<span>{{ item.json.name }}</span>
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
							:disabled="disabled || !PermRecordingsCanPush()"
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
							:disabled="disabled || !PermRecordingsCanDelete()"
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
import { Component, Vue } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import DataTableBase from './DataTableBase';
import ResizeObserver from 'resize-observer-polyfill';
import DataLoaderProject from '@/Components/DataLoaders/DataLoaderProject.vue';
import DataLoaderProduct from '@/Components/DataLoaders/DataLoaderProduct.vue';
import SignalRConnection from '@/RPC/SignalRConnection';
import { IRecording, Recording } from '@/Data/CRM/Recording/Recording';
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
export default class RecordingsDataTable extends DataTableBase {
	
	public $refs!: {
		dataTable: Vue,
	};
	
	protected headers = [
		{
			text: 'Name',
			value: 'json.name',
			align: 'left',
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
	protected PermRecordingsCanRequest = Recording.PermRecordingsCanRequest;
	protected PermRecordingsCanDelete = Recording.PermRecordingsCanDelete;
	protected PermRecordingsCanPush = Recording.PermRecordingsCanPush;
	protected get AllRecordings(): IRecording[] {
		return Object.values<IRecording>(this.$store.state.Database.recordings);
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
				
				if (Recording.PermRecordingsCanRequest()) {
					
					const rtr = Recording.RequestRecordings.Send({
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
	protected CustomDataTableFilter(value: any, search: string | null, item: IRecording): boolean {
		//console.log(`CustomDataTableFilter '${search}'`);
		
		if (search == null || IsNullOrEmpty(search)) {
			return true;
		}
		
		let haystack: string | null = null;
		let index: number | null = null;
		
		// Date
		if (item.json.name && !IsNullOrEmpty(item.json.name)) {
			haystack = '' + item.json.name;
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}
		
		// // Project
		// if (item.json.projectId && !IsNullOrEmpty(item.json.projectId)) {
		// 	haystack = '' + Project.CombinedDescriptionForId(item.json.projectId);
		// 	index = haystack.toLowerCase().indexOf(search.toLowerCase());
		// 	if (index !== -1) {
		// 		return true;
		// 	}
		// }
		
		// // Quantity
		// haystack = '' + item.json.quantity + ' ' + item.json.quantityUnit;
		// index = haystack.toLowerCase().indexOf(search.toLowerCase());
		// if (index !== -1) {
		// 	return true;
		// }
		
		// // Product
		// haystack = '' + this.ProductNameForId(item.json.productId);
		// index = haystack.toLowerCase().indexOf(search.toLowerCase());
		// if (index !== -1) {
		// 	return true;
		// }
		
		// // Location
		// haystack = '' + item.json.location;
		// index = haystack.toLowerCase().indexOf(search.toLowerCase());
		// if (index !== -1) {
		// 	return true;
		// }
		
		// // Extra
		// haystack = '' + (item.json.isExtra ? 'extra' : '');
		// index = haystack.toLowerCase().indexOf(search.toLowerCase());
		// if (index !== -1) {
		// 	return true;
		// }
		
		// // Billed
		// haystack = '' + (item.json.isBilled ? 'extra' : 'billed');
		// index = haystack.toLowerCase().indexOf(search.toLowerCase());
		// if (index !== -1) {
		// 	return true;
		// }
		
		return false;
	}
	
	
	
	
	protected EditEntry(val: IRecording): void {
		// console.log('EditEntry', val);
		
		this.$emit('edit-entry', val);

	}
	
	protected DeleteEntry(val: IRecording): void {
		// console.log('DeleteEntry', val);
		
		this.$emit('delete-entry', val);
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
