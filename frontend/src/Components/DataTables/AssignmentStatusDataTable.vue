<template>
	<v-card flat>

		<v-text-field autocomplete="newpassword" class="mx-4 e2e-assignment-status-data-table-filter"
			v-model="searchString" hide-details label="Filter" prepend-inner-icon="search" solo
			style="margin-bottom: 10px;"></v-text-field>


		<v-data-table v-if="PermAssignmentStatusCanRequest()" :headers="headers" :items="AllAssignmentStatuses"
			:search="searchString" item-key="name" :custom-filter="CustomDataTableFilter" ref="dataTable" :footer-props="{
				showFirstLastPage: true,
				firstIcon: 'mdi-arrow-collapse-left',
				lastIcon: 'mdi-arrow-collapse-right',
				prevIcon: 'chevron_left',
				nextIcon: 'chevron_right',
				'items-per-page-options': [5, 10, 15, 50],
			}">
			<template v-slot:[`item.json.name`]="{ item }">
				<span>{{ item.json.name }}</span>
			</template>
			<template v-slot:[`item.json.isOpen`]="{ item }">
				<div v-if="item.json.isOpen">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isReOpened`]="{ item }">
				<div v-if="item.json.isReOpened">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isAssigned`]="{ item }">
				<div v-if="item.json.isAssigned">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isWaitingOnClient`]="{ item }">
				<div v-if="item.json.isWaitingOnClient">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isWaitingOnVendor`]="{ item }">
				<div v-if="item.json.isWaitingOnVendor">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isBillable`]="{ item }">
				<div v-if="item.json.isBillable">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isBillableReview`]="{ item }">
				<div v-if="item.json.isBillableReview">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isInProgress`]="{ item }">
				<div v-if="item.json.isInProgress">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isNonBillable`]="{ item }">
				<div v-if="item.json.isNonBillable">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isScheduled`]="{ item }">
				<div v-if="item.json.isScheduled">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isDefault`]="{ item }">
				<div v-if="item.json.isDefault">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.action`]="{ item }">
				<v-menu bottom left>
					<template v-slot:activator="{ on }">
						<v-btn icon v-on="on" :disabled="disabled">
							<v-icon>more_vert</v-icon>
						</v-btn>
					</template>

					<v-list dense>
						<v-list-item @click="EditEntry(item)" :disabled="disabled || !PermAssignmentStatusCanPush()">
							<v-list-item-icon>
								<v-icon>edit</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Edit…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item @click="DeleteEntry(item)"
							:disabled="disabled || !PermAssignmentStatusCanDelete()">
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
import Dialogues from '@/Utility/Dialogues';
import SignalRConnection from '@/RPC/SignalRConnection';
import { AssignmentStatus, IAssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({
	components: {
		PermissionsDeniedAlert,
	},
})
export default class AssignmentStatusDataTable extends DataTableBase {

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
			text: 'Is Open',
			value: 'json.isOpen',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Re-Opened',
			value: 'json.isReOpened',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Assigned',
			value: 'json.isAssigned',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Waiting on Client',
			value: 'json.isWaitingOnClient',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Waiting on Vendor',
			value: 'json.isWaitingOnVendor',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Billable',
			value: 'json.isBillable',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Billable Review',
			value: 'json.isBillableReview',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is In Progress',
			value: 'json.isInProgress',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Non Billable',
			value: 'json.isNonBillable',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Scheduled',
			value: 'json.isScheduled',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Default',
			value: 'json.isDefault',
			align: 'center',
			sort: (a: string, b: string): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
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
	protected PermAssignmentStatusCanRequest = AssignmentStatus.PermAssignmentStatusCanRequest;
	protected PermAssignmentStatusCanPush = AssignmentStatus.PermAssignmentStatusCanPush;
	protected PermAssignmentStatusCanDelete = AssignmentStatus.PermAssignmentStatusCanDelete;
	protected get AllAssignmentStatuses(): IAssignmentStatus[] {
		return Object.values<IAssignmentStatus>(this.$store.state.Database.assignmentStatus);
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

				if (AssignmentStatus.PermAssignmentStatusCanRequest()) {
					const rtr = AssignmentStatus.RequestAssignmentStatus.Send({
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
	protected CustomDataTableFilter(value: any, search: string | null, item: IAssignmentStatus): boolean {
		//console.log(`CustomDataTableFilter '${search}'`);

		if (search == null || IsNullOrEmpty(search)) {
			return true;
		}

		let haystack: string | null = null;
		let index: number | null = null;

		// Name
		if (item.json.name && !IsNullOrEmpty(item.json.name)) {
			haystack = '' + item.json.name;
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}


		// isOpen
		haystack = '' + item.json.isOpen ? 'open' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		// isReOpened
		haystack = '' + item.json.isReOpened ? 'reopened' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		// isAssigned
		haystack = '' + item.json.isAssigned ? 'assigned' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		// isWaitingOnClient
		haystack = '' + item.json.isWaitingOnClient ? 'waiting on client' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		// isWaitingOnVendor
		haystack = '' + item.json.isWaitingOnVendor ? 'waiting on vendor' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		// isBillable
		haystack = '' + item.json.isBillable ? 'billable' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		// isBillableReview
		haystack = '' + item.json.isBillable ? 'billable review' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		// isDefault
		haystack = '' + item.json.isBillable ? 'default' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		return false;
	}




	protected EditEntry(val: IAssignmentStatus): void {
		//console.log('EditEntry', id);

		Dialogues.Open({
			name: 'ModifyAssignmentStatusDialogue',
			state: val,
		});
	}

	protected DeleteEntry(val: IAssignmentStatus): void {
		//console.log('DeleteEntry', id);

		Dialogues.Open({
			name: 'DeleteAssignmentStatusDialogue',
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
