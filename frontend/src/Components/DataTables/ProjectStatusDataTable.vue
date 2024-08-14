<template>
	<v-card flat>

		<v-text-field autocomplete="newpassword" class="mx-4 e2e-project-status-data-table-filter"
			v-model="searchString" hide-details label="Filter" prepend-inner-icon="search" solo
			style="margin-bottom: 10px;"></v-text-field>


		<v-data-table v-if="PermProjectStatusCanRequest()" :headers="headers" :items="AllProjectStatuses"
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
			<template v-slot:[`item.json.isAwaitingPayment`]="{ item }">
				<div v-if="item.json.isAwaitingPayment">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isClosed`]="{ item }">
				<div v-if="item.json.isClosed">&#x2714;</div>
				<div v-else>&#x274C;</div>
			</template>
			<template v-slot:[`item.json.isNewProjectStatus`]="{ item }">
				<div v-if="item.json.isNewProjectStatus">&#x2714;</div>
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
						<v-list-item @click="EditEntry(item)" :disabled="disabled || !PermProjectStatusCanPush()">
							<v-list-item-icon>
								<v-icon>edit</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Edit…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item @click="DeleteEntry(item)" :disabled="disabled || !PermProjectStatusCanDelete()">
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
import { IProjectStatus, ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({
	components: {
		PermissionsDeniedAlert,
	},
})
export default class ProjectStatusDataTable extends DataTableBase {

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
			sort: (a: boolean, b: boolean): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Awaiting Payment',
			value: 'json.isAwaitingPayment',
			align: 'center',
			sort: (a: boolean, b: boolean): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is Closed',
			value: 'json.isClosed',
			align: 'center',
			sort: (a: boolean, b: boolean): number => {
				const descA = a ? 'true' : 'false';
				const descB = b ? 'true' : 'false';
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Is New Project Status',
			value: 'json.isNewProjectStatus',
			align: 'center',
			sort: (a: boolean, b: boolean): number => {
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
	protected PermProjectStatusCanRequest = ProjectStatus.PermProjectStatusCanRequest;
	protected PermProjectStatusCanPush = ProjectStatus.PermProjectStatusCanPush;
	protected PermProjectStatusCanDelete = ProjectStatus.PermProjectStatusCanDelete;
	protected loadingData = false;
	protected resizeObserver: ResizeObserver | null = null;
	protected closestMain: Element | null = null;

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

				if (ProjectStatus.PermProjectStatusCanRequest()) {
					const rtr = ProjectStatus.RequestProjectStatus.Send({
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

	protected get AllProjectStatuses(): IProjectStatus[] {
		return Object.values<IProjectStatus>(this.$store.state.Database.projectStatus);
	}


	// eslint-disable-next-line 
	protected CustomDataTableFilter(value: any, search: string | null, item: IProjectStatus): boolean {
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

		// isAwaitingPayment
		haystack = '' + item.json.isAwaitingPayment ? 'awaiting payment' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		// isClosed
		haystack = '' + item.json.isClosed ? 'closed' : '';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}

		return false;
	}




	protected EditEntry(val: IProjectStatus): void {
		//console.log('EditEntry', id);

		Dialogues.Open({
			name: 'ModifyProjectStatusDialogue',
			state: val,
		});
	}

	protected DeleteEntry(val: IProjectStatus): void {
		//console.log('DeleteEntry', id);

		Dialogues.Open({
			name: 'DeleteProjectStatusDialogue',
			state: {
				redirectToIndex: true,
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
