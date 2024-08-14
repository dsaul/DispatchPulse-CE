<template>
	<div>
		<div v-if="PermLabourCanRequest()">
			<v-text-field v-if="showFilter" autocomplete="newpassword" class="mx-4" v-model="filter" hide-details
				label="Filter" prepend-inner-icon="search" solo style="margin-bottom: 10px;" ref="filterField">
			</v-text-field>

			<v-row v-if="showDateFilters" no-gutters style="padding-left: 10px;padding-right: 10px;">
				<v-col style="padding: 5px; max-width: calc(50% - 37px)">
					<v-menu v-model="filterDateStartMenu" :close-on-content-click="false" :nudge-right="40"
						transition="scale-transition" offset-y min-width="290px">
						<template v-slot:activator="{ on }">
							<v-text-field v-model="filterDateStart" label="From" prepend-inner-icon="event" readonly
								hide-details solo v-on="on"></v-text-field>
						</template>
						<v-date-picker v-model="filterDateStart" @input="filterDateStartMenu = false"
							:allowed-dates="FilterDateStartAllowedDates" :elevation="1">
							<div style="text-align: center; width: 100%;">From Date</div>
						</v-date-picker>
					</v-menu>
				</v-col>
				<v-col style="padding: 5px; max-width: calc(50% - 37px)">
					<v-menu v-model="filterDateEndMenu" :close-on-content-click="false" :nudge-right="40"
						transition="scale-transition" offset-y min-width="290px">
						<template v-slot:activator="{ on }">
							<v-text-field v-model="filterDateEnd" label="Filter End Date" prepend-inner-icon="event"
								readonly hide-details solo v-on="on"></v-text-field>
						</template>
						<v-date-picker v-model="filterDateEnd" @input="filterDateEndMenu = false"
							:allowed-dates="FilterDateEndAllowedDates" :elevation="1">
							<div style="text-align: center; width: 100%;">To Date</div>
						</v-date-picker>
					</v-menu>
				</v-col>
				<v-col style="padding: 5px; max-width: 64px;">
					<v-menu offset-y>
						<template v-slot:activator="{ on }">
							<v-btn color="primary" dark v-on="on" style="width: 100%; height: 100%;">
								<v-icon>date_range</v-icon>
							</v-btn>
						</template>
						<v-list>
							<v-list-item
								@click="SetFilterDateRange(payPeriods.lastPeriodStart, payPeriods.lastPeriodEnd)">
								<v-list-item-title>Last Pay Period</v-list-item-title>
							</v-list-item>
							<v-list-item
								@click="SetFilterDateRange(payPeriods.currentPeriodStart, payPeriods.currentPeriodEnd)">
								<v-list-item-title>Current Pay Period</v-list-item-title>
							</v-list-item>
							<v-list-item
								@click="SetFilterDateRange(payPeriods.nextPeriodStart, payPeriods.nextPeriodEnd)">
								<v-list-item-title>Next Pay Period</v-list-item-title>
							</v-list-item>

							<div v-if="payPeriods">
								<v-divider />
								<v-list-item v-for="(item, index) in payPeriods.historicPeriods" :key="index"
									@click="SetFilterDateRange(item.start, item.end)">
									<v-list-item-title>{{ DateTimeToDateShortPrefixed(item.start) }} to
										{{ DateTimeToDateShortPrefixed(item.end) }}</v-list-item-title>
								</v-list-item>
							</div>
						</v-list>
					</v-menu>
				</v-col>
			</v-row>

			<div v-if="PageRows.length != 0">
				<template>
					<div class="text-center" v-if="showTopPagination === true">
						<v-pagination v-model="CurrentPage" :length="PageCount" :total-visible="breadcrumbsVisibleCount"
							style="margin-bottom: 10px; margin-top: 20px;">
						</v-pagination>
					</div>
				</template>

				<v-list-item-group color="primary">
					<div v-for="(row, index) in PageRows" :key="row.id">

						<v-toolbar v-if="row.isHeader" dense
							style="margin-left: 20px; margin-right: 20px;margin-top: 10px;margin-bottom: 10px;"
							elevation="1" color="rgb(116, 115, 137)" short dark>
							<v-toolbar-title dense v-text="row.title"></v-toolbar-title>
							<v-spacer></v-spacer>
							<v-btn v-if="row.trailingButton" icon @click="row.trailingButtonCallback"
								:disabled="disabled || !PermCRMLabourManualEntries()">
								<v-icon v-text="row.trailingButtonIcon"></v-icon>
							</v-btn>
						</v-toolbar>
						<LabourListItem v-else v-model="PageRows[index]" :showMenuButton="showMenuButton"
							:isDialogue="isDialogue" @ClickEntry="ClickEntry" @OpenEntry="OpenEntry"
							@DeleteEntry="DeleteEntry" :focusIsProject="focusIsProject" :focusIsAgent="focusIsAgent"
							:rootProject="rootProject" :disabled="disabled" />
					</div>
				</v-list-item-group>

				<template>
					<div class="text-center">
						<v-pagination v-model="CurrentPage" :length="PageCount" :total-visible="breadcrumbsVisibleCount"
							style="margin-bottom: 20px; margin-top: 20px;">
						</v-pagination>
					</div>
				</template>

				<v-toolbar dense style="margin-left: 20px; margin-right: 20px;margin-top: 10px;margin-bottom: 10px;"
					elevation="1" color="rgb(116, 115, 137)" short dark>
					<v-toolbar-title dense>Summary</v-toolbar-title>
					<v-spacer></v-spacer>
					<!--<v-btn v-if="row.trailingButton" icon @click="row.trailingButtonCallback">
						<v-icon v-text="row.trailingButtonIcon"></v-icon>
					</v-btn>-->
				</v-toolbar>

				<v-container>
					<v-row>
						<v-col cols="6" sm="2" offset-sm="1" style="text-align: right; font-weight: bold;">
							Total This Page:
						</v-col>
						<v-col>
							{{ ('' + HoursTotalPage.hours).padStart(2, '0') }}h <!--
						-->{{ ('' + HoursTotalPage.minutes).padStart(2, '0') }}m <!--
						-->{{ ('' + HoursTotalPage.seconds.toFixed(0)).padStart(2, '0') }}s
						</v-col>
					</v-row>
					<v-row>
						<v-col cols="6" sm="2" offset-sm="1" style="text-align: right; font-weight: bold;">
							Total All Pages:
						</v-col>
						<v-col>
							{{ ('' + HoursTotalAll.hours).padStart(2, '0') }}h <!--
						-->{{ ('' + HoursTotalAll.minutes).padStart(2, '0') }}m <!--
						-->{{ ('' + HoursTotalAll.seconds.toFixed(0)).padStart(2, '0') }}s
						</v-col>
					</v-row>
				</v-container>
			</div>
			<div v-else>
				<div v-if="loadingData" style="margin-left: 20px; margin-right: 20px;">
					<content-placeholders>
						<content-placeholders-heading :img="true" />
						<!-- <content-placeholders-text :lines="3" /> -->
					</content-placeholders>
				</div>
				<v-alert v-else outlined type="info" elevation="0"
					style="margin-left: 15px; margin-right: 15px; margin-bottom: 0px;">
					{{ emptyMessage }}
				</v-alert>
			</div>


		</div>
		<PermissionsDeniedAlert v-else />
	</div>
</template>

<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import LabourListItem from '@/Components/ListItems/LabourListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import _ from 'lodash';
import { mapGetters } from 'vuex';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ListBase, { IListHeader } from './ListBase';
import { DateTime } from 'luxon';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { LabourType } from '@/Data/CRM/LabourType/LabourType';
import { LabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/LabourSubtypeException';
import { LabourSubtypeHoliday } from '@/Data/CRM/LabourSubtypeHoliday/LabourSubtypeHoliday';
import { LabourSubtypeNonBillable } from '@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';
import GetPayPeriodsNextToDate from '@/Utility/GetPayPeriodsNextToDate';
import { IGetPayPeriodsNextToDate } from '@/Utility/GetPayPeriodsNextToDate';
import DateTimeToDateShortPrefixed from '@/Utility/Formatters/DateTime/DateTimeToDateShortPrefixed';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		LabourListItem,
		PermissionsDeniedAlert,
	},
	computed: {
		...mapGetters([
			'SessionId',
		]),
	},
})
export default class LabourList extends ListBase {

	@Prop({ default: null }) public readonly rootProject!: IProject;
	@Prop({ default: null }) public readonly showOnlyProjectId!: string;
	@Prop({ default: false }) public readonly showChildrenOfProjectIdAsWell!: boolean;
	@Prop({ default: null }) public readonly showOnlyAgentId!: string;
	@Prop({ default: null }) public readonly showOnlyAssignmentId!: string;
	@Prop({ default: 'There are no labour entries to show.' }) declare public readonly emptyMessage: string;
	@Prop({ default: false }) public readonly focusIsProject!: boolean;
	@Prop({ default: false }) public readonly focusIsAgent!: boolean;
	@Prop({ default: false }) public readonly showOnlyForBillingUsersAgent!: boolean;
	@Prop({ default: false }) public readonly showOnlyActiveAndToday!: boolean;
	@Prop({ default: null }) public readonly prefillProjectId!: string | null;
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: false }) public readonly isReverseSort!: boolean;
	@Prop({ default: true }) public readonly showDateFilters!: boolean;

	public $refs!: {
		filterField: Vue,
	};

	public SessionId!: string;

	protected DateTimeToDateShortPrefixed = DateTimeToDateShortPrefixed;
	protected MobileDeviceWidth = MobileDeviceWidth;
	protected PermLabourCanRequest = Labour.PermLabourCanRequest;
	protected PermCRMLabourManualEntries = Labour.PermCRMLabourManualEntries;

	protected filter = '';
	protected filterDateStart: string = DateTime.local().minus({ years: 50 }).toFormat('yyyy-LL-dd');
	protected filterDateEnd: string = DateTime.local().plus({ years: 50 }).toFormat('yyyy-LL-dd');
	protected filterDateStartMenu = false;
	protected filterDateEndMenu = false;

	protected payPeriods: IGetPayPeriodsNextToDate | null = null;

	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;





	public MountedAfter(): void {
		this.payPeriods = GetPayPeriodsNextToDate();
	}

	public get IsLoadingData(): boolean {

		return this.loadingData;
	}

	public LoadData(): void {

		//console.debug('LabourList LoadData()');

		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}

		this._LoadDataTimeout = setTimeout(() => {

			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {

					const promises: Array<Promise<any>> = [];

					if (Labour.PermLabourCanRequest()) {
						const rtr = Labour.RequestLabour.Send({
							sessionId: BillingSessions.CurrentSessionId(),
							limitToSessionAgent: this.showOnlyForBillingUsersAgent,
							limitToProjectId: this.showOnlyProjectId,
							limitToAgentId: this.showOnlyAgentId,
							limitToAssignmentId: this.showOnlyAssignmentId,
							limitToActiveAndToday: this.showOnlyActiveAndToday,
							showChildrenOfProjectIdAsWell: this.showChildrenOfProjectIdAsWell,
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

		}, 250);

	}

	public InsertRowHeaders(rows: ILabour[] | IListHeader[]): void {

		let lastDate: string | null = null;

		for (let i = 0; i < rows.length; i++) {

			const row = rows[i] as ILabour;

			if (!row.json.startISO8601) {
				continue;
			}

			const startIsoStr = row.json.startISO8601;
			const startIso = DateTime.fromISO(startIsoStr);
			const startLocal = startIso.toLocal();
			const date = startLocal.toLocaleString(DateTime.DATE_MED);

			//console.log('##', lastDate , startIso);

			if (lastDate !== date) {

				// Insert

				rows.splice(i, 0, {
					isHeader: true,
					title: date,
					trailingButton: true,
					trailingButtonIcon: 'add',
					trailingButtonCallback: () => {

						Dialogues.Open({
							name: 'ModifyLabourDialogue', state: {
								json: {
									agentId: Agent.LoggedInAgentId(),
									projectId: this.prefillProjectId,
									startISO8601: row.json.startISO8601,
								},
							}
						});

					},
				} as IListHeader);

				lastDate = date;
				i++;
			}

			//console.debug('obj', rows[i]);

		}



	}

	public SelectFilterField(): void {
		//console.log('SelectFilterField()', this.$refs.filterField);
		if (this.$refs.filterField) {
			const input = this.$refs.filterField.$el.querySelector('input');
			if (input) {
				input.focus();
			}
		}
	}

	public get FilterDateStart(): string | null {
		return this.filterDateStart;
	}

	public get FilterDateEnd(): string | null {
		return this.filterDateEnd;
	}

	protected SetFilterDateRange(from: DateTime, to: DateTime): void {

		//console.debug('SetFilterDateRange()', from, to);

		this.filterDateStart = from.toFormat('yyyy-LL-dd');
		this.filterDateEnd = to.toFormat('yyyy-LL-dd');
	}


	protected FilterDateStartAllowedDates(val: string): boolean { // 1970-05-31
		//console.log('FilterDateStartAllowedDates', val);

		const validateValue = DateTime.fromFormat(val, 'yyyy-LL-dd', {
			zone: 'local',
		});


		const filterEndLocal = DateTime.fromFormat(this.filterDateEnd, 'yyyy-LL-dd', {
			zone: 'local',
		});


		return validateValue <= filterEndLocal;
	}
	protected FilterDateEndAllowedDates(val: string): boolean { // 1970-05-31

		const validateValue = DateTime.fromFormat(val, 'yyyy-LL-dd', {
			zone: 'local',
		});

		const filterStartLocal = DateTime.fromFormat(this.filterDateStart, 'yyyy-LL-dd', {
			zone: 'local',
		});

		//console.log('FilterDateEndAllowedDates', val);
		return validateValue >= filterStartLocal;
	}


	protected get HoursTotalPage(): { hours: number, minutes: number, seconds: number } {

		const ret = {
			hours: 0,
			minutes: 0,
			seconds: 0,
		};
		const rows = this.PageRows as ILabour[];

		for (const row of rows) {

			if (!row || !row.json) {
				continue;
			}

			if (row.json.timeMode === 'date-and-hours') {

				if (row.json.hours !== null && row.json.hours !== undefined) {

					const hours = Math.floor(+row.json.hours);
					const minutes = (60 * ((+row.json.hours) % 1));

					ret.hours += hours;
					ret.minutes += minutes;
				}

			} else if (row.json.timeMode === 'start-stop-timestamp') {

				if (row.json.startISO8601 &&
					!IsNullOrEmpty(row.json.startISO8601) &&
					row.json.endISO8601 &&
					!IsNullOrEmpty(row.json.endISO8601)
				) {
					const startISO = DateTime.fromISO(row.json.startISO8601);
					const startLocal = startISO.toLocal();
					const endISO = DateTime.fromISO(row.json.endISO8601);
					const endLocal = endISO.toLocal();

					const diff = endLocal.diff(startLocal, ['hours', 'minutes', 'seconds'])
						.toObject() as { hours: number, minutes: number, seconds: number };

					ret.hours += diff.hours;
					ret.minutes += diff.minutes;
					ret.seconds += diff.seconds;
				}

			} else {
				console.error(`unknown timeMode ${row.json.timeMode}`);
			}

		}


		// Reduce results to something more readable.

		// seconds
		{
			const whole = Math.floor(ret.seconds / 60);
			const remainder = ret.seconds % 60;

			ret.minutes += whole;
			ret.seconds = remainder;
		}

		// minutes
		{
			const whole = Math.floor(ret.minutes / 60);
			const remainder = ret.minutes % 60;


			ret.hours += whole;
			ret.minutes = remainder;
		}

		ret.hours = Math.floor(ret.hours);
		ret.minutes = Math.floor(ret.minutes);
		ret.seconds = Math.floor(ret.seconds);
		return ret;
	}

	protected get HoursTotalAll(): { hours: number, minutes: number, seconds: number } {

		const ret = {
			hours: 0,
			minutes: 0,
			seconds: 0,
		};
		const rows = this.Rows as ILabour[];

		for (const row of rows) {

			if (!row || !row.json) {
				continue;
			}

			if (row.json.timeMode === 'date-and-hours') {

				if (row.json.hours !== null && row.json.hours !== undefined) {

					const hours = Math.floor(+row.json.hours);
					const minutes = (60 * ((+row.json.hours) % 1));

					ret.hours += hours;
					ret.minutes += minutes;
				}

			} else if (row.json.timeMode === 'start-stop-timestamp') {

				if (row.json.startISO8601 &&
					!IsNullOrEmpty(row.json.startISO8601) &&
					row.json.endISO8601 &&
					!IsNullOrEmpty(row.json.endISO8601)
				) {
					const startISO = DateTime.fromISO(row.json.startISO8601);
					const startLocal = startISO.toLocal();
					const endISO = DateTime.fromISO(row.json.endISO8601);
					const endLocal = endISO.toLocal();

					const diff = endLocal.diff(startLocal, ['hours', 'minutes', 'seconds'])
						.toObject() as { hours: number, minutes: number, seconds: number };

					ret.hours += diff.hours;
					ret.minutes += diff.minutes;
					ret.seconds += diff.seconds;
				}

			} else if (row.json.timeMode === 'none') {
				// do nothing
			} else {
				console.error(`unknown timeMode ${row.json.timeMode}`);
			}

		}

		// Reduce results to something more readable.

		// seconds
		{
			const whole = Math.floor(ret.seconds / 60);
			const remainder = ret.seconds % 60;

			ret.minutes += whole;
			ret.seconds = remainder;
		}

		// minutes
		{
			const whole = Math.floor(ret.minutes / 60);
			const remainder = ret.minutes % 60;


			ret.hours += whole;
			ret.minutes = remainder;
		}

		ret.hours = Math.floor(ret.hours);
		ret.minutes = Math.floor(ret.minutes);
		ret.seconds = Math.floor(ret.seconds);
		return ret;
	}


	protected GetOpenAsDialogue(): boolean {
		return true;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected GetEntryRouteForId(id: string): string {
		throw Error('not implemented');
		//return `/section/projects/${id}?tab=General`;
	}

	protected GetOpenDialogueName(): string {
		return 'ModifyLabourDialogue';
	}

	protected GetOpenDialogueModelState(id: string): ILabour | null {

		console.log('GetOpenDialogueModelState', id);

		const labour = Labour.ForId(id);

		const clone = _.cloneDeep(labour);
		return clone;
	}

	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteLabourDialogue';
	}

	protected GetDeleteDialogueModelState(id: string): {
		redirectToIndex: boolean;
		id: guid;
	} | null {

		return {
			redirectToIndex: false,
			id,
		};
	}

	protected GetRawRows(): Record<string, ILabour> {
		return this.$store.state.Database.labour;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: ILabour, key: string): boolean {
		let result = true;

		//console.log('RowFilter', o);

		//return result;
		do {
			if (!o || !o.id || !o.json) {
				result = false;
				break;
			}

			if (this.excludeIds && this.excludeIds.length > 0) {

				let isInExcludeList = false;

				for (const id of this.excludeIds) {
					if (null === id) {
						continue;
					}

					if (o.id === id) {
						isInExcludeList = true;
						break;
					}
				}

				if (isInExcludeList) {
					result = false;
					break;
				}

			}

			// Date Filter
			if (this.showDateFilters && o.json.startISO8601) {

				const entryStart = DateTime.fromISO(o.json.startISO8601);
				//const entryLocal = entryStart.toLocal();

				const filterStartLocal = DateTime.fromFormat(this.filterDateStart, 'yyyy-LL-dd', {
					zone: 'local',
				});
				let filterEndLocal = DateTime.fromFormat(this.filterDateEnd, 'yyyy-LL-dd', {
					zone: 'local',
				});
				filterEndLocal = filterEndLocal.set({
					hour: 23,
					minute: 59,
					second: 59,
				});

				if (entryStart < filterStartLocal || entryStart > filterEndLocal) {
					result = false;
					break;
				}





			}







			if (this.showOnlyActiveAndToday) {

				if (!o.json.startISO8601) {
					result = false;
					break;
				}

				const dbStart = DateTime.fromISO(o.json.startISO8601);
				const localStart = dbStart.toLocal();

				const localNow = DateTime.local();
				const localStartOfDay = localNow.startOf('day');
				const localEndOfDay = localNow.endOf('day');


				if (o.json.isActive === false && (localStart < localStartOfDay || localStart > localEndOfDay)) {
					result = false;
					break;
				}




			}



			if (this.showOnlyForBillingUsersAgent) {
				// Only show assignments for this billing user's agent.
				if (o.json.agentId !== Agent.LoggedInAgentId()) {
					result = false;
					break;
				}
			}

			if (this.showOnlyAssignmentId) {
				if (o.json.assignmentId !== this.showOnlyAssignmentId) {
					result = false;
					break;
				}
			}

			if (this.showOnlyAgentId) {
				if (o.json.agentId !== this.showOnlyAgentId) {
					result = false;
					break;
				}
			}

			if (this.showOnlyProjectId) {

				let projects = [];

				const labourEntryProject = Project.ForId(o.json.projectId);
				const showOnlyProject = Project.ForId(this.showOnlyProjectId);


				if (this.showChildrenOfProjectIdAsWell) {

					projects = Project.RecursiveChildProjectsOfId(this.showOnlyProjectId);
				} else {


					if (showOnlyProject) {
						projects.push(showOnlyProject);
					}

				}

				const found = !!_.find(projects, (value) => {
					return labourEntryProject?.id === value.id;
				});

				if (!found) {
					result = false;
					break;
				}

			}

			if (this.showFilter) {
				let haystack = '';

				if (o.json.projectId) {
					const project = Project.ForId(o.json.projectId);

					if (project) {
						if (!IsNullOrEmpty(project.json.name)) {
							haystack += ` ${project.json.name} `;
						}

						for (const addr of project.json.addresses) {
							if (!IsNullOrEmpty(addr.value)) {
								haystack += ` ${addr.value} `;
							}
						}

					}
				}


				if (o.json.agentId) {
					const agent = Agent.ForId(o.json.agentId);
					if (agent) {
						haystack += ` ${agent.json.name} `;
					}
				}

				//if (o.json.assignmentId) {
				//	
				//}

				if (o.json.typeId) {
					const labourType = LabourType.ForId(o.json.typeId);

					if (labourType) {
						haystack += ` ${labourType.json.name} `;
					}
				}

				//if (o.json.timeMode) {
				//	
				//}

				if (o.json.hours) {

					const h = `${Math.floor(+o.json.hours)}`.padStart(2, '0');
					const m = (60 * ((+o.json.hours) % 1)).toFixed(0).padStart(2, '0');

					haystack += ` ${h}h ${m}m 00s `;
				}

				if (o.json.startISO8601 && o.json.endISO8601) {


					const startISO = DateTime.fromISO(o.json.startISO8601);
					const startLocal = startISO.toLocal();
					const endISO = DateTime.fromISO(o.json.endISO8601);
					const endLocal = endISO.toLocal();
					const diff = endLocal.diff(startLocal, ['hours', 'minutes', 'seconds']).toObject();


					const h = ('' + diff.hours).padStart(2, '0');
					const m = ('' + diff.minutes).padStart(2, '0');
					const s = ('' + (diff.seconds as number).toFixed(0)).padStart(2, '0');

					haystack += ` ${h}h ${m}m ${s}s `;
				}


				if (o.json.startISO8601) {
					haystack += DateTime.fromISO(o.json.startISO8601).toLocaleString(DateTime.DATETIME_SHORT);
				}
				if (o.json.endISO8601) {
					haystack += DateTime.fromISO(o.json.endISO8601).toLocaleString(DateTime.DATETIME_SHORT);
				}


				haystack += o.json.isActive ? ' Active ' : '';

				if (o.json.locationType) {
					switch (o.json.locationType.trim()) {
						case 'travel':
							haystack += ' Travel ';
							break;
						case 'on-site':
							haystack += ' On Site ';
							break;
						case 'remote':
							haystack += ' Remote ';
							break;
					}
				}





				haystack += Labour.IsExtraForId(o.id) ? ' Extra ' : '';
				haystack += o.json.isBilled ? ' Billed ' : '';
				haystack += o.json.isPaidOut ? ' Paid Out ' : '';

				if (o.json.exceptionTypeId) {
					const type = LabourSubtypeException.ForId(o.json.exceptionTypeId);

					if (type) {
						haystack += ` ${type.json.name} `;
					}

				}

				if (o.json.holidayTypeId) {

					const type = LabourSubtypeHoliday.ForId(o.json.holidayTypeId);

					if (type) {
						haystack += ` ${type.json.name} `;
					}

				}

				if (o.json.nonBillableTypeId) {

					const type = LabourSubtypeNonBillable.ForId(o.json.nonBillableTypeId);

					if (type) {
						haystack += ` ${type.json.name} `;
					}

				}

				if (o.json.notes) {
					haystack += o.json.notes;
				}

				haystack = haystack.replace(/\W/g, '');
				haystack = haystack.toLowerCase();


				let needle = this.filter.toLowerCase();
				needle = needle.replace(/\W/g, '');

				//console.log('haystack:',haystack,'needle:',needle);

				if (haystack.indexOf(needle) === -1) {
					result = false;
					break;
				}
			}

		} while (false);

		return result;
	}






	protected RowSortBy(o: ILabour): string {

		return `${o.json.startISO8601}${o.id}`;

	}

	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
}

</script>