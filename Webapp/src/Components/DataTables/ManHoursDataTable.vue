<template>
	<v-card flat>
				
		<v-text-field
			autocomplete="newpassword"
			class="mx-4 e2e-man-hours-data-table-filter"
			v-model="searchString"
			hide-details
			label="Filter"
			prepend-inner-icon="search"
			solo
			style="margin-bottom: 10px;"
		></v-text-field>
		
		
		<v-data-table
			v-if="PermEstimatingManHoursCanRequest()"
			:headers="headers"
			:items="AllManHours"
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
				<router-link
					v-if="item && item.json && item.json.projectId"
					:to="`/section/projects/${item.json.projectId}?tab=Materials`"
					>
					
					{{ProjectCombinedDescriptionForId(item.json.projectId)}}
				</router-link>
			</template>
			<template v-slot:[`item.json.dateUsedISO8601`]="{ item }">
				{{ ISO8601ToLocalDateOnly(item.json.dateUsedISO8601) }}
			</template>
			<template v-slot:[`item.json.productId`]="{ item }">
				{{ ProductNameForId(item.json.productId) }}
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
							:disabled="disabled || !PermEstimatingManHoursCanPush()"
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
							:disabled="disabled || !PermEstimatingManHoursCanDelete()"
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
import SignalRConnection from '@/RPC/SignalRConnection';
import { EstimatingManHours, IEstimatingManHours } from '@/Data/CRM/EstimatingManHours/EstimatingManHours';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({
	components: {
		PermissionsDeniedAlert,
	},
})
export default class ManHoursDataTable extends DataTableBase {
	
	public $refs!: {
		dataTable: Vue,
	};
	
	protected headers = [
		{
			text: 'Task',
			value: 'json.item',
			align: 'left',
			sort: (a: string, b: string): number => {
				const descA = '' + a;
				const descB = '' + b;
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Hours',
			value: 'json.manHours',
			align: 'right',
			sort: (a: number | null, b: number | null): number => {
				const descA = '' + a;
				const descB = '' + b;
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Measurement',
			value: 'json.measurement',
			align: 'left',
			sort: (a: string | null, b: string | null): number => {
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
	protected PermEstimatingManHoursCanRequest = EstimatingManHours.PermEstimatingManHoursCanRequest;
	protected PermEstimatingManHoursCanPush = EstimatingManHours.PermEstimatingManHoursCanPush;
	protected PermEstimatingManHoursCanDelete = EstimatingManHours.PermEstimatingManHoursCanDelete;
	protected get AllManHours(): IEstimatingManHours[] {
		return Object.values<IEstimatingManHours>(this.$store.state.Database.estimatingManHours);
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
				
				if (EstimatingManHours.PermEstimatingManHoursCanRequest()) {
					
					const rtr = EstimatingManHours.RequestEstimatingManHours.Send({
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
	protected CustomDataTableFilter(value: any, search: string | null, item: IEstimatingManHours): boolean {
		//console.log(`CustomDataTableFilter '${search}'`);
		
		if (search == null || IsNullOrEmpty(search)) {
			return true;
		}
		
		let haystack: string | null = null;
		let index: number | null = null;
		
		// item
		if (item.json.item && !IsNullOrEmpty(item.json.item)) {
			haystack = '' + item.json.item;
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}
		
		// manHours
		haystack = '' + item.json.manHours;
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// measurement
		if (item.json.measurement && !IsNullOrEmpty(item.json.measurement)) {
			haystack = '' + item.json.measurement;
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}
		
		return false;
	}
	
	
	
	
	protected EditEntry(val: IEstimatingManHours): void {
		//console.log('EditEntry', id);
		
		Dialogues.Open({ 
			name: 'ModifyManHourDialogue', 
			state: val,
		});
	}
	
	protected DeleteEntry(val: IEstimatingManHours): void {
		//console.log('DeleteEntry', id);
		
		Dialogues.Open({ 
			name: 'DeleteManHourDialogue', 
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
