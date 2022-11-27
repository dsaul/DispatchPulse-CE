<template>
	<v-card flat>
				
		<v-text-field
			autocomplete="newpassword"
			class="mx-4 e2e-labour-data-table-filter"
			v-model="SearchString"
			hide-details
			label="Filter"
			prepend-inner-icon="search"
			solo
			style="margin-bottom: 10px;"
		></v-text-field>
		
		
		<v-data-table
			v-if="PermLabourCanRequest()"
			:headers="headers"
			:items="AllLabour"
			:search="SearchString"
			item-key="name"
			:custom-filter="CustomDataTableFilter"
			must-sort
			sort-by="json.startISO8601"
			:sort-desc="true"
			ref="dataTable"
			:footer-props="{
			showFirstLastPage: true,
			firstIcon: 'mdi-arrow-collapse-left',
			lastIcon: 'mdi-arrow-collapse-right',
			prevIcon: 'chevron_left',
			nextIcon: 'chevron_right',
			'items-per-page-options': [5,10,15,50],
			}"
			class="data-table-labour"
			>
			<template v-slot:[`item.json.startISO8601`]="{ item }">
				
				<v-tooltip
					v-if="item.json.timeMode == 'date-and-hours'"
					top
					>
					<template v-slot:activator="{ on }">
					
						<v-chip label outlined style="margin: 4px;" v-on="on">
							<v-avatar left>
								<v-icon small>calendar_today</v-icon>
							</v-avatar>
							{{ ISO8601ToLocalDateOnly(item.json.startISO8601) }}
						</v-chip>
						
					</template>
					<span>Date</span>
				</v-tooltip>
				<v-tooltip
					v-else-if="item.json.timeMode == 'start-stop-timestamp'"
					top
					>
					<template v-slot:activator="{ on }">
					
						<v-chip label outlined style="margin: 4px;" v-on="on">
							<v-avatar left>
								<v-icon small>calendar_today</v-icon>
							</v-avatar>
							{{ ISO8601LocalDateOnlyTimeSpan(item.json.startISO8601, item.json.endISO8601) }}
						</v-chip>
					
					</template>
					<span>Date Timespan</span>
				</v-tooltip>
				<span v-else>&mdash;</span>
				
				
				
			</template>
			<template v-slot:[`item.json.agentId`]="{ item }">
				
				<DataLoaderAgent v-if="item.json.agentId" :agentId="item.json.agentId">
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								label
								outlined
								style="margin: 4px; color: rgba(0, 0, 0, 0.87) !important;"
								class="e2e-agent-chip"
								:to="`/section/agents/${item.json.agentId}?tab=Labour`"
								color="primary"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>directions_walk</v-icon>
								</v-avatar>
								{{ AgentNameForId(item.json.agentId) }}
							</v-chip>
						</template>
						<span>Agent</span>
					</v-tooltip>
				</DataLoaderAgent>
				
			</template>
			<template v-slot:[`item.json.projectId`]="{ item }">
				
				<!-- Project -->
				<DataLoaderProject v-if="item.json.projectId" :projectId="item.json.projectId">
					<!-- Project Address Chip -->
					<v-tooltip
						v-if="ProjectAddressForId(item.json.projectId)"
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								label
								outlined
								:to="`/section/projects/${item.json.projectId}?tab=Labour`"
								color="primary"
								style="margin: 4px; color: rgba(0, 0, 0, 0.87) !important;"
								v-on="on"
								class="e2e-address-chip"
								><!-- Billable -->
								<v-avatar left>
									<v-icon small>fa-home</v-icon>
								</v-avatar>
								{{ ProjectAddressForId(item.json.projectId) }}
							</v-chip>
						</template>
						<span>Project Address</span>
					</v-tooltip>
					
					
					<!-- Project Name Chip -->
					<v-tooltip
						v-if="ProjectNameForId(item.json.projectId)"
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								label
								outlined
								:to="`/section/projects/${item.json.projectId}?tab=Labour`"
								color="primary"
								style="margin: 4px; color: rgba(0, 0, 0, 0.87) !important;"
								v-on="on"
								class="e2e-project-name-chip"
								><!-- Billable -->
								<v-avatar left>
									<v-icon small>assignment</v-icon>
								</v-avatar>
								{{ ProjectNameForId(item.json.projectId) }}
							</v-chip>
						</template>
						<span>Project Name</span>
					</v-tooltip>
				</DataLoaderProject>
				
				
				<!-- Holiday -->
				<v-tooltip
					v-if="LabourTypeIsHolidayForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on"
							>
							<v-avatar left>
								<v-icon small v-text="LabourSubtypeHolidayIconForId(item.json.holidayTypeId)"></v-icon>
							</v-avatar>
							{{LabourSubtypeHolidayNameForId(item.json.holidayTypeId)}}
						</v-chip>
					</template>
					<span>Observed Holiday</span>
				</v-tooltip>
				
				<!-- Non Billable -->
				<v-tooltip
					v-if="LabourTypeIsNonBillableForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on">
							<v-avatar left>
								<v-icon small v-text="LabourSubtypeNonBillableIconForId(item.json.nonBillableTypeId)"></v-icon>
							</v-avatar>
							{{LabourSubtypeNonBillableNameForId(item.json.nonBillableTypeId)}}
						</v-chip>
					</template>
					<span>Non Billable Type</span>
				</v-tooltip>
				
				<!-- Exception -->
				<v-tooltip
					v-if="LabourTypeIsExceptionForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on">
							<v-avatar left>
								<v-icon small v-text="LabourSubtypeExceptionIconForId(item.json.exceptionTypeId)"></v-icon>
							</v-avatar>
							{{LabourSubtypeExceptionNameForId(item.json.exceptionTypeId)}}
						</v-chip>
					</template>
					<span>Exception Type</span>
				</v-tooltip>
				
				<!-- Banked Payout -->
				<v-tooltip
					v-if="LabourTypeIsPayOutBankedForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on">
							Payout Banked
						</v-chip>
					</template>
					<span>Banked Payout</span>
				</v-tooltip>
				
				<!-- Assignment -->
				<DataLoaderAssignment
					v-if="item.json.assignmentId"
					:assignmentId="item.json.assignmentId">
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								:to="`/section/assignments/${item.json.assignmentId}?tab=Labour`"
								color="primary"
								label
								outlined
								style="margin: 4px; color: rgba(0, 0, 0, 0.87) !important;"
								v-on="on"
								class="e2e-assignment-chip"
								>
								<v-avatar left>
									<v-icon small>fa-tasks</v-icon>
								</v-avatar>
								{{ AssignmentWorkDescriptionForId(item.json.assignmentId) || 'No Work Specified' }}
							</v-chip>
						</template>
						<span>Assignment</span>
					</v-tooltip>
				</DataLoaderAssignment>
				
			</template>
			<template v-slot:[`item.json.typeId`]="{ item }">
				
				<!-- If a name was provided. -->
				<v-tooltip
					v-if="LabourTypeNameForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on">
							<v-avatar left>
								<v-icon small v-text="LabourTypeIconForId(item.json.typeId)"></v-icon>
							</v-avatar>
							{{LabourTypeNameForId(item.json.typeId)}}
						</v-chip>
					</template>
					<span>Entry Type</span>
				</v-tooltip>
				<!-- Otherwise... -->
				<v-tooltip
					v-else-if="LabourTypeIsBillableForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on">
							Is Billable
						</v-chip>
					</template>
					<span>Entry Type</span>
				</v-tooltip>
				<v-tooltip
					v-else-if="LabourTypeIsHolidayForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on">
							Is Holiday
						</v-chip>
					</template>
					<span>Entry Type</span>
				</v-tooltip>
				
				<v-tooltip
					v-else-if="LabourTypeIsNonBillableForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on">
							Is Non-Billable
						</v-chip>
					</template>
					<span>Entry Type</span>
				</v-tooltip>
				
				<v-tooltip
					v-else-if="LabourTypeIsExceptionForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip
							label
							outlined
							style="margin: 4px;"
							v-on="on">
							Is Exception
						</v-chip>
					</template>
					<span>Entry Type</span>
				</v-tooltip>
				
				<v-tooltip
					v-else-if="LabourTypeIsPayOutBankedForId(item.json.typeId)"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip
							outlined
							label
							style="margin: 4px;"
							v-on="on">
							Is Banked Payout
						</v-chip>
					</template>
					<span>Entry Type</span>
				</v-tooltip>
				
				
			</template>
			<template v-slot:[`item.time`]="{ item }">
				
				<div v-if="item.json.timeMode == 'date-and-hours'">
					
					
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip label outlined style="margin: 4px;" v-on="on">
								<v-avatar left>
									<v-icon small>timelapse</v-icon>
								</v-avatar>
								{{ Math.floor(+item.json.hours) }}h {{ (60 * ((+item.json.hours) % 1)).toFixed(0).padStart(2, '0') }}m
							</v-chip>
						</template>
					<span>Duration</span>
				</v-tooltip>
					
				</div>
				<div v-if="item.json.timeMode == 'start-stop-timestamp'">
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip label outlined style="margin: 4px;" v-on="on">
								<v-avatar left>
									<v-icon small>timelapse</v-icon>
								</v-avatar>
								<span v-if="item.json.isActive">In Progress</span>
								<span v-else>
									{{ (ISO8601Difference(item.json.startISO8601,item.json.endISO8601).hours).toFixed(0) }}h 
									{{ (ISO8601Difference(item.json.startISO8601,item.json.endISO8601).minutes).toFixed(0).padStart(2, '0') }}m
								</span>
								
							</v-chip>
						</template>
						<span>Duration</span>
					</v-tooltip>
					
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip label outlined style="margin: 4px;"  v-on="on">
								<v-avatar left>
									<v-icon small>fa-hourglass-start</v-icon>
								</v-avatar>
								{{ ISO8601ToLocalDatetime(item.json.startISO8601) }}
							</v-chip>
						</template>
						<span>Start Time</span>
					</v-tooltip>
					
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip label outlined style="margin: 4px;" v-on="on">
								<v-avatar left>
									<v-icon small>fa-hourglass-end</v-icon>
								</v-avatar>
								<span v-if="item.json.isActive">In Progress</span>
								<span v-else>
									{{ ISO8601ToLocalDatetime(item.json.endISO8601) }}
								</span>
							</v-chip>
						</template>
						<span>End Time</span>
					</v-tooltip>
					
					
					
				</div>
			</template>
			<template v-slot:[`item.details`]="{ item }">
				<v-tooltip
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip v-if="item.json.isActive" label outlined style="margin: 4px;" v-on="on">
							Active
						</v-chip>
					</template>
					<span>This entry is currently in progress.</span>
				</v-tooltip>
				<v-tooltip
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip v-if="item.json.locationType && item.json.locationType != 'none'" label outlined style="margin: 4px;" v-on="on">
							<span v-if="item.json.locationType == 'travel'">Travel</span>
							<span v-else-if="item.json.locationType == 'on-site'">On Site</span>
							<span v-else-if="item.json.locationType == 'remote'">Remote</span>
							<span v-else v-text="item.json.locationType"></span>
						</v-chip>
					</template>
					<span>Where this entry takes place.</span>
				</v-tooltip>
				<v-tooltip
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip v-if="LabourIsExtraForId(item.id)" label outlined style="margin: 4px;" v-on="on">
							Extra
						</v-chip>
					</template>
					<span>This entry is not included in the client's contract.</span>
				</v-tooltip>
				<v-tooltip
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip v-if="item.json.isBilled" label outlined style="margin: 4px;" v-on="on">
							Billed
						</v-chip>
					</template>
					<span>This entry has been billed to the client.</span>
				</v-tooltip>
				<v-tooltip
					top
					>
					<template v-slot:activator="{ on }">
						<v-chip v-if="item.json.isPaidOut" label outlined style="margin: 4px;" v-on="on">
							Paid Out
						</v-chip>
					</template>
					<span>This entry has been paid out to the agent.</span>
				</v-tooltip>
				<PopUpChip
					v-if="item.json.notes"
					tooltipText="This entry has notes."
					>
					<v-card-text>
						{{item.json.notes}}
					</v-card-text>
				</PopUpChip>
				
				
				
			</template>
			<template v-slot:[`item.action`]="{ item }">
				<v-menu bottom left>
					<template v-slot:activator="{ on }">
						<v-btn
						icon
						v-on="on"
						>
							<v-icon small>more_vert</v-icon>
						</v-btn>
					</template>

					<v-list dense>
						<v-list-item
							class="e2e-labour-data-table-item-edit"
							@click="EditEntry(item)"
							:disabled="disabled || !PermLabourCanPush() || !PermCRMLabourManualEntries()"
							>
							<v-list-item-icon>
								<v-icon small>edit</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Edit…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item
							@click="DeleteEntry(item)"
							class="e2e-labour-data-table-item-delete"
							:disabled="disabled || !PermLabourCanDelete()"
							>
							<v-list-item-icon>
								<v-icon small>delete</v-icon>
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
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import PopUpChip from '@/Components/Chips/PopUpChip.vue';
import SignalRConnection from '@/RPC/SignalRConnection';
import DataLoaderProject from '@/Components/DataLoaders/DataLoaderProject.vue';
import DataLoaderProduct from '@/Components/DataLoaders/DataLoaderProduct.vue';
import DataLoaderAgent from '@/Components/DataLoaders/DataLoaderAgent.vue';
import DataLoaderAssignment from '@/Components/DataLoaders/DataLoaderAssignment.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({
	components: {
		PopUpChip,
		DataLoaderProject,
		DataLoaderProduct,
		DataLoaderAgent,
		DataLoaderAssignment,
		PermissionsDeniedAlert,
	},
})
export default class LabourDataTable extends DataTableBase {
	
	
	public $refs!: {
		dataTable: Vue,
	};
	
	protected headers = [
		{
			text: 'Date',
			value: 'json.startISO8601',
			align: 'left',
			sort: (a: string, b: string): number => {
				//console.log(a, b);
				const descA = '' + a;
				const descB = '' + b;
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Agent',
			value: 'json.agentId',
			align: 'left',
			sort: (a: string, b: string): number => {
				const descA = '' + a;
				const descB = '' + b;
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Description',
			value: 'json.projectId',
			align: 'left',
			sort: (a: string, b: string): number => { // eslint-disable-line @typescript-eslint/no-unused-vars
				const descA = '' + Project.CombinedDescriptionForId(a);
				const descB = '' + Project.CombinedDescriptionForId(a);
				//console.log(descA, descB);
				return descA.localeCompare(descB);
			},
		},
		{
			text: 'Type',
			value: 'json.typeId',
			align: 'left',
			sortable: false,
		},
		{
			text: 'Time',
			value: 'time',
			align: 'left',
			sortable: false,
		},
		{
			text: 'Details',
			value: 'details',
			align: 'left',
			sortable: false,
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
	protected searchStringTimeout: ReturnType<typeof setTimeout> | null = null;
	protected PermLabourCanRequest = Labour.PermLabourCanRequest;
	protected PermLabourCanPush = Labour.PermLabourCanPush;
	protected PermLabourCanDelete = Labour.PermLabourCanDelete;
	protected PermCRMLabourManualEntries = Labour.PermCRMLabourManualEntries;
	protected LabourIsExtraForId = Labour.IsExtraForId;
	
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
				
				if (Labour.PermLabourCanRequest()) {
					const rtr = Labour.RequestLabour.Send({
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
	
	
	
	protected get SearchString(): string {
		return this.searchString;
	}
	
	protected set SearchString(val: string) {
		
		if (this.searchStringTimeout) {
			clearTimeout(this.searchStringTimeout);
			this.searchStringTimeout = null;
		}
		
		this.searchStringTimeout = setTimeout(() => {
			Vue.set(this, 'searchString', val);
		}, 500);
		
		//this.searchString = val;
	}
	
	
	
	
	protected get AllLabour(): ILabour[] {
		const ret = Object.values<ILabour>(this.$store.state.Database.labour);
		//console.log(ret);
		return ret;
	}
	
	
	
	
	
	// eslint-disable-next-line 
	protected CustomDataTableFilter(value: any, search: string | null, item: ILabour): boolean {
		
		if (!item || !item.id) {
			return false;
		}
		
		//console.log(`CustomDataTableFilter '${search}'`);
		
		if (search == null || IsNullOrEmpty(search)) {
			return true;
		}
		
		let haystack: string | null = null;
		let index: number | null = null;
		
		// Agent
		haystack = '' + this.AgentNameForId(item.json.agentId);
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Project Address
		haystack = '' + Project.AddressForId(item.json.projectId);
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		
		
		// Project Name
		haystack = '' + Project.NameForId(item.json.projectId);
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Holiday
		haystack = '' + this.LabourSubtypeHolidayNameForId(item.json.holidayTypeId);
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// NonBillable
		haystack = '' + this.LabourSubtypeNonBillableNameForId(item.json.nonBillableTypeId);
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Exception
		haystack = '' + this.LabourSubtypeExceptionNameForId(item.json.exceptionTypeId);
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Banked Payout
		haystack = 'banked payout';
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// Assignment Description
		haystack = '' + (this.AssignmentWorkDescriptionForId(item.json.assignmentId) || 'No Work Specified');
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		// LabourTypeName
		const customTypeName = this.LabourTypeNameForId(item.json.typeId);
		if (null != customTypeName && !IsNullOrEmpty(customTypeName)) {
			haystack = '' + this.LabourTypeNameForId(item.json.typeId);
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		} else if (this.LabourTypeIsBillableForId(item.json.typeId)) {
			haystack = 'Is Billable';
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		} else if (this.LabourTypeIsHolidayForId(item.json.typeId)) {
			haystack = 'Is Holiday';
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		} else if (this.LabourTypeIsNonBillableForId(item.json.typeId)) {
			haystack = 'Is Non-Billable';
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		} else if (this.LabourTypeIsExceptionForId(item.json.typeId)) {
			haystack = 'Is Exception';
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		} else if (this.LabourTypeIsPayOutBankedForId(item.json.typeId)) {
			haystack = 'Is Banked Payout';
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}
		
		
		
		
		// Details
		haystack = '';
		
		if (item.json.isActive) {
			haystack += 'active ';
		}
		if (item.json.locationType && !IsNullOrEmpty(item.json.locationType) && item.json.locationType !== 'none') {
			if (item.json.locationType === 'travel') {
				haystack += 'travel ';
			} else if (item.json.locationType === 'on-site') {
				haystack += 'on site on-site ';
			} else if (item.json.locationType === 'remote') {
				haystack += 'remote ';
			} else {
				haystack += '' + item.json.locationType + ' ';
			}
		}
		if (Labour.IsExtraForId(item.id)) {
			haystack += 'extra ';
		}
		if (item.json.isBilled) {
			haystack += 'billed ';
		}
		if (item.json.isPaidOut) {
			haystack += 'paid out paid-out';
		}
		if (item.json.notes && !IsNullOrEmpty(item.json.notes)) {
			haystack += item.json.notes;
		}
		index = haystack.toLowerCase().indexOf(search.toLowerCase());
		if (index !== -1) {
			return true;
		}
		
		
		// Date Start
		if (item.json.startISO8601 && !IsNullOrEmpty(item.json.startISO8601)) {
			
			if (item.json.timeMode === 'date-and-hours') {
				haystack = `${this.ISO8601ToLocalDateOnly(item.json.startISO8601)}${item.json.endISO8601}`;
			} else if (item.json.timeMode === 'start-stop-timestamp') {
				haystack = `${this.ISO8601LocalDateOnlyTimeSpan(item.json.startISO8601, item.json.endISO8601)}`;
			} else {
				haystack = '';
			}
			
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}
		
		// Time
		if (item.json.hours && item.json.timeMode === 'date-and-hours') {
			haystack = `${Math.floor(+item.json.hours)}h ${(60 * ((+item.json.hours) % 1)).toFixed(0).padStart(2, '0')}m`;
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		} else if (item.json.startISO8601 && item.json.endISO8601 && item.json.timeMode === 'start-stop-timestamp') {
			
			const diff = this.ISO8601Difference(item.json.startISO8601, item.json.endISO8601);
			haystack = '';
			
			if (diff && diff.hours && diff.minutes) {
				haystack += `${diff.hours.toFixed(0).padStart(2, '0')}h ${diff.minutes.toFixed(0).padStart(2, '0')}m `;
			}
			haystack += this.ISO8601ToLocalDatetime(item.json.startISO8601);
			haystack += ' ';
			
			haystack += this.ISO8601ToLocalDatetime(item.json.endISO8601);
			haystack += ' ';
			index = haystack.toLowerCase().indexOf(search.toLowerCase());
			if (index !== -1) {
				return true;
			}
		}
		
		return false;
		
		
	}
	
	
	
	
	protected EditEntry(val: ILabour): void {
		//console.log('EditEntry', id);
		
		Dialogues.Open({ 
			name: 'ModifyLabourDialogue', 
			state: val,
		});
	}
	
	protected DeleteEntry(val: ILabour): void {
		//console.log('DeleteEntry', id);
		
		Dialogues.Open({ 
			name: 'DeleteLabourDialogue', 
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
<style>
	.data-table-labour td {
		padding: 0px !important;
	}
</style>