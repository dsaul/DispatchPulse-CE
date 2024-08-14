<template>
	<div style="padding: 15px; padding-top: 5px; background: white;">
		
		<div :class="{
			'grid-container': true,
			'grid-container-with-minimized-elements': HasMinimizedColumns,
		}">
			<div class="options" style="display: flex; justify-content: center;">
				
				<v-btn icon x-large @click="GoToPreviousDate">
					<v-icon>navigate_before</v-icon>
				</v-btn>
				
				<v-menu
					style="flex: 1;"
					v-model="showDateSelect"
					:close-on-content-click="false"
					:nudge-right="30"
					:nudge-top="25"
					transition="scale-transition"

					offset-y
					min-width="290px"
				>
					<template v-slot:activator="{ on }">
					<v-text-field
						v-model="date"
						label="Date"
						prepend-icon="event"
						readonly
						v-on="on"
						style="max-width:300px;"
					></v-text-field>
					</template>
					<v-date-picker
						v-model="date"
						@input="showDateSelect = false"
						:elevation="1"
						></v-date-picker>
				</v-menu>
				
				<v-btn icon x-large @click="GoToNextDate">
					<v-icon>navigate_next</v-icon>
				</v-btn>
				
			</div>
			<scroll-sync class="header-name">
				Agent
			</scroll-sync>
			<div class="header-unassigned">
				Unassigned
			</div>
			<scroll-sync group="scheduler" horizontal class="header-content">
				<SchedulerHeader 
					v-for="(column, index) in columns"
					:key="index"
					:cellWidthPx="cellWidthPx"
					:cellWidthMinimizedPx="cellWidthMinimizedPx"
					:column="column"
					:minimized="MinimizedColumns[index]"
					/>
				<div style="min-width: 5000px;height: 1px;"></div> <!-- Padding to ensure the two scroll areas keep being aligned. -->
			</scroll-sync>
			<scroll-sync group="scheduler" vertical class="main-name">
				<v-list-item-group color="primary">
					<!-- <AgentsListItem 
						:class="`agent-row _unassigned`" 
						:showMenuButton="false" 
						:isPlaceholder="true" 
						:reducePadding="true" 
						:placeholderValues="{ name: 'Unassigned' }" 
						/> -->
					<div v-if="Rows">
						<AgentsListItem 
							:class="`agent-row _${row.id}`" 
							:showMenuButton="false" 
							:reducePadding="true" 
							v-for="(row, index) in Rows" 
							:key="row.id" 
							v-model="Rows[index]" 
							:showEmploymentStatus="false"
							/>
					</div>
				</v-list-item-group>
				<div style="height: 5000px;"></div> <!-- Padding to ensure the two scroll areas keep being aligned. -->
			</scroll-sync>
			<scroll-sync group="scheduler" vertical horizontal class="main-content">
				<!-- <SchedulerRow
					:date="date"
					:cellWidthPx="cellWidthPx"
					:cellWidthMinimizedPx="cellWidthMinimizedPx"
					class="schedule-row"
					:pushHeightTargetSelector="`.agent-row._unassigned`"
					:agentId="null"
					:columns="columns"
					:minimizedColumns="MinimizedColumns"
					/> -->
				<div v-if="Rows">
					<SchedulerRow
						:date="date"
						:cellWidthPx="cellWidthPx"
						:cellWidthMinimizedPx="cellWidthMinimizedPx"
						class="schedule-row"
						v-for="row in Rows"
						:key="row.id"
						:pushHeightTargetSelector="`.agent-row._${row.id}`"
						:agentId="row.id"
						:columns="columns"
						:minimizedColumns="MinimizedColumns"
						/>
				</div>
			</scroll-sync>
			<div class="main-unassigned">
				<SchedulerCell
					:date="date"
					:agentId="null"
					:cellWidthPx="cellWidthPx"
					:cellWidthMinimizedPx="cellWidthMinimizedPx"
					:isUnscheduled="true"
					:isFirstAM="false"
					:isSecondAM="false"
					:isFirstPM="false"
					:isSecondPM="false"
					:localTimeStart="null"
					:localTimeEnd="null"
					:minimized="false"
					style="min-height: 100%;"
					/>
				
			</div>
			
			
		</div>
	</div>
	
</template>
<script lang="ts">

import ScrollSync from 'vue-scroll-sync';
import { Component, Vue, Prop } from 'vue-property-decorator';
import AgentsListItem from '@/Components/ListItems/AgentsListItem.vue';
import _ from 'lodash';
import SchedulerRow from '@/Components/Scheduler/SchedulerRow.vue';
import SchedulerHeader from '@/Components/Scheduler/ScheduleHeader.vue';
import Draggable from 'vuedraggable';
import { DateTime } from 'luxon';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import SchedulerCell from '@/Components/Scheduler/SchedulerCell.vue';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { EmploymentStatus } from '@/Data/CRM/EmploymentStatus/EmploymentStatus';

export interface SchedulerColumn {
	type: 'unscheduled' | 'am1' | 'am2' | 'pm1' | 'pm2' | 'timespan';
	columnHeader: string;
	columnHeaderMinimized: string;
	localTimeStart: { hour: number, minute: number, second: number } | null;
	localTimeEnd: { hour: number, minute: number, second: number } | null;
}


@Component({
	components: {
		ScrollSync,
		AgentsListItem,
		SchedulerRow,
		SchedulerHeader,
		SchedulerCell,
		Draggable,
	},
	props: {
		
	},
})
export default class Scheduler extends Vue {
	
	@Prop({ default: 150 }) public readonly cellWidthPx!: number;
	@Prop({ default: 40 }) public readonly cellWidthMinimizedPx!: number;
	
	@Prop({ default: () => DateTime.local().toFormat('yyyy-MM-dd') }) public readonly date!: string;
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	protected showDateSelect = false;
	
	protected columns: SchedulerColumn[] = [
		{
			type: 'unscheduled',
			columnHeader: 'Unscheduled',
			columnHeaderMinimized: 'Unscheduled',
			localTimeStart: null,
			localTimeEnd: null,
		},
		{
			type: 'am1',
			columnHeader: '1st AM',
			columnHeaderMinimized: '1st AM',
			localTimeStart: null,
			localTimeEnd: null,
		},
		{
			type: 'am2',
			columnHeader: '2nd AM',
			columnHeaderMinimized: '2nd AM',
			localTimeStart: null,
			localTimeEnd: null,
		},
		{
			type: 'pm1',
			columnHeader: '1st PM',
			columnHeaderMinimized: '1st PM',
			localTimeStart: null,
			localTimeEnd: null,
		},
		{
			type: 'pm2',
			columnHeader: '2nd PM',
			columnHeaderMinimized: '2nd PM',
			localTimeStart: null,
			localTimeEnd: null,
		},
		{
			type: 'timespan',
			columnHeader: 'Before Hours',
			columnHeaderMinimized: 'Before',
			localTimeStart: { hour: 0, minute: 0, second: 0 },
			localTimeEnd: { hour: 7, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '8:00 AM',
			columnHeaderMinimized: '8 AM',
			localTimeStart: { hour: 8, minute: 0, second: 0 },
			localTimeEnd: { hour: 8, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '9:00 AM',
			columnHeaderMinimized: '9 AM',
			localTimeStart: { hour: 9, minute: 0, second: 0 },
			localTimeEnd: { hour: 9, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '10:00 AM',
			columnHeaderMinimized: '10 AM',
			localTimeStart: { hour: 10, minute: 0, second: 0 },
			localTimeEnd: { hour: 10, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '11:00 AM',
			columnHeaderMinimized: '11 AM',
			localTimeStart: { hour: 11, minute: 0, second: 0 },
			localTimeEnd: { hour: 11, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '12:00 PM',
			columnHeaderMinimized: '12 PM',
			localTimeStart: { hour: 12, minute: 0, second: 0 },
			localTimeEnd: { hour: 12, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '1:00 PM',
			columnHeaderMinimized: '1 PM',
			localTimeStart: { hour: 13, minute: 0, second: 0 },
			localTimeEnd: { hour: 13, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '2:00 PM',
			columnHeaderMinimized: '2 PM',
			localTimeStart: { hour: 14, minute: 0, second: 0 },
			localTimeEnd: { hour: 14, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '3:00 PM',
			columnHeaderMinimized: '3 PM',
			localTimeStart: { hour: 15, minute: 0, second: 0 },
			localTimeEnd: { hour: 15, minute: 59, second: 59 },
			
		},
		{
			type: 'timespan',
			columnHeader: '4:00 PM',
			columnHeaderMinimized: '4 PM',
			localTimeStart: { hour: 16, minute: 0, second: 0 },
			localTimeEnd: { hour: 16, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: '5:00 PM',
			columnHeaderMinimized: '5 PM',
			localTimeStart: { hour: 17, minute: 0, second: 0 },
			localTimeEnd: { hour: 17, minute: 59, second: 59 },
		},
		{
			type: 'timespan',
			columnHeader: 'After Hours',
			columnHeaderMinimized: 'After',
			localTimeStart: { hour: 18, minute: 0, second: 0 },
			localTimeEnd: { hour: 23, minute: 59, second: 59 },
		},
	];
	
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
					
					if (Agent.PermAgentsCanRequest()) {
						const rtrAgents = Agent.RequestAgents.Send({
							sessionId: BillingSessions.CurrentSessionId(),
						});
						if (rtrAgents.completeRequestPromise) {
							promises.push(rtrAgents.completeRequestPromise);
						}
					}
					
					if (Assignment.PermAssignmentCanRequest()) {
						const rtrAssignments = Assignment.RequestAssignments.Send({
							sessionId: BillingSessions.CurrentSessionId(),
						});
						if (rtrAssignments.completeRequestPromise) {
							promises.push(rtrAssignments.completeRequestPromise);
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
	
	public get HasMinimizedColumns(): boolean {
		
		for (const b of this.MinimizedColumns) {
			if (b) {
				return true;
			}
		}
		
		return false;
	}
	
	public get MinimizedColumns(): boolean[] {
		
		const ret: boolean[] = [];
		
		for (const column of this.columns) {
			
			switch (column.type) {
				case 'unscheduled':
				{
					const hasItems = Assignment.ForSchedulerMinimization({
						date: this.date,
						isUnscheduled: true,
						isFirstAM: false,
						isSecondAM: false,
						isFirstPM: false,
						isSecondPM: false,
						localTimeStart: null,
						localTimeEnd: null,
					});
					ret.push(!hasItems);
				}
				break;
				case 'am1':
				{
					const hasItems = Assignment.ForSchedulerMinimization({
						date: this.date,
						isUnscheduled: false,
						isFirstAM: true,
						isSecondAM: false,
						isFirstPM: false,
						isSecondPM: false,
						localTimeStart: null,
						localTimeEnd: null,
					});
					ret.push(!hasItems);
				}
				break;
				case 'am2':
				{
					const hasItems = Assignment.ForSchedulerMinimization({
						date: this.date,
						isUnscheduled: false,
						isFirstAM: false,
						isSecondAM: true,
						isFirstPM: false,
						isSecondPM: false,
						localTimeStart: null,
						localTimeEnd: null,
					});
					ret.push(!hasItems);
				}
				break;
				case 'pm1':
				{
					const hasItems = Assignment.ForSchedulerMinimization({
						date: this.date,
						isUnscheduled: false,
						isFirstAM: false,
						isSecondAM: false,
						isFirstPM: true,
						isSecondPM: false,
						localTimeStart: null,
						localTimeEnd: null,
					});
					ret.push(!hasItems);
				}
				break;
				case 'pm2':
				{
					const hasItems = Assignment.ForSchedulerMinimization({
						date: this.date,
						isUnscheduled: false,
						isFirstAM: false,
						isSecondAM: false,
						isFirstPM: false,
						isSecondPM: true,
						localTimeStart: null,
						localTimeEnd: null,
					});
					ret.push(!hasItems);
				}
				break;
				case 'timespan':
				{
					const hasItems = Assignment.ForSchedulerMinimization({
						date: this.date,
						isUnscheduled: false,
						isFirstAM: false,
						isSecondAM: false,
						isFirstPM: false,
						isSecondPM: false,
						localTimeStart: column.localTimeStart,
						localTimeEnd: column.localTimeEnd,
					});
					ret.push(!hasItems);
				}
				break;
			}
		}
		
		return ret;
	}
	
	public get Rows(): IAgent[] {
		
		
		//console.log('rows',this);
		
		const filtered: IAgent[] = _.filter(
			this.$store.state.Database.agents,
			(o: IAgent) => {
				
				
				let result = true;
				
				do {
					
					const employmentStatusId = o.json.employmentStatusId;
					if (null == employmentStatusId || IsNullOrEmpty(employmentStatusId)) {
						result = false;
						break;
					}
					
					const employmentStatus = EmploymentStatus.ForId(employmentStatusId);
					if (null == employmentStatus) {
						result = false;
						break;
					}
					
					if (false === employmentStatus.json.shouldBeListedInScheduler) {
						result = false;
						break;
					}
					
					
					
					
					let haystack = '';
					haystack += o.json.name;
					haystack = haystack.replace(/\W/g, '');
					haystack = haystack.toLowerCase();
					
					
					let needle = this.filter.toLowerCase();
					needle = needle.replace(/\W/g, '');
					
					//console.log('haystack:',haystack,'needle:',needle);
					
					if (haystack.indexOf(needle) === -1) {
						result = false;
						break;
					}
					
				} while (false);
				
				return result;
			},
		);
		
		//console.log(filtered);
		
		const sorted: IAgent[] = _.sortBy(filtered, (o: IAgent) => {
			return o.json.name;
		});
		
		
		return sorted;
	}
	
	protected GoToPreviousDate(): void {
		
		//console.debug('GoToPreviousDate', this.date);
		
		const dateLocal = DateTime.fromFormat(this.date, 'yyyy-MM-dd', {
			zone: 'local',
			setZone: true,
		});
		
		const yesterdayDateLocal = dateLocal.minus({ days: 1 });
		const newDate = yesterdayDateLocal.toFormat('yyyy-MM-dd');
		
		console.debug('newDate', newDate);
		//this.date = newDate;
		
		this.$emit('OnDateChanged', newDate);
		
		
	}
	
	protected GoToNextDate(): void {
		//console.debug('GoToNextDate', this.date);
		
		const dateLocal = DateTime.fromFormat(this.date, 'yyyy-MM-dd', {
			zone: 'local',
			setZone: true,
		});
		
		const tomorrowDateLocal = dateLocal.plus({ days: 1 });
		const newDate = tomorrowDateLocal.toFormat('yyyy-MM-dd');
		
		//this.date = newDate;
		this.$emit('OnDateChanged', newDate);
		
		//console.debug('newDate', newDate);
	}
	
	protected mounted(): void {
		//console.log(this);
		
		this.LoadData();
	}
	
	
	
}
</script>
<style scoped>

.grid-container {
	display: grid;
	grid-template-columns: 200px auto 168px;
	grid-template-rows: 70px 45px auto 50px;
	height: 100vh;
	margin-top: calc(-112px + -48px + -41px + -5px + -15px);
	padding-top: calc(112px + 48px + 41px + 5px + 15px); 
	-moz-box-sizing: border-box;
	box-sizing: border-box;
}

.grid-container-with-minimized-elements {
	grid-template-rows: 70px 80px auto 50px;
}

@media only screen and (max-width: 279px) {
	.grid-container  {
		margin-top: calc(-104px + -48px + -61px + -5px + -15px);
		padding-top: calc(104px + 48px + 61px + 5px + 15px);
	}
}
@media only screen and (max-width: 959px) {
	.grid-container  {
		margin-top: calc(-104px + -48px + -41px + -5px + -15px);
		padding-top: calc(104px + 48px + 41px + 5px + 15px);
	}
}

.header-name {
	grid-column: 1;
	grid-row: 2;
	overflow: hidden;
	color: rgba(0, 0, 0, 0.87);
	padding: 10px;
	font-weight: bold;
	border-bottom: 1px solid #111;
	border-right: 1px solid #111;
	display: flex;
	align-items: flex-end;
	/*margin-top: 20px;*/
}

.header-unassigned {
	grid-column: 3;
	grid-row: 2;
	overflow: hidden;
	color: rgba(0, 0, 0, 0.87);
	padding: 10px;
	font-weight: bold;
	border-bottom: 1px solid #111;
	border-right: 1px solid #111;
	border-left: 1px solid #111;
	display: flex;
	align-items: flex-end;
	/*margin-top: 20px;*/
}

.header-content {
	grid-column: 2;
	grid-row: 2;
	overflow: hidden;
	border-bottom: 1px solid #111;
	display:flex;
	/*margin-top: 20px;*/
}

.main-name {
	grid-column: 1;
	grid-row: 3 / span 4;
	overflow: hidden;
	border-right: 1px solid #111;
}

.main-content {
	
	grid-column: 2;
	grid-row: 3 / span 4;
	overflow: auto;
	/* background: url('https://static.dispatchpulse.com/subtle-patterns/so-white.png'); */
	
}

.main-unassigned {
	grid-column: 3;
	grid-row: 3 / span 4;
	overflow: auto;
	border-left: 1px solid #111;
	overflow-y: scroll;
	height: 100%;
}

.options {

	grid-row: 1;
	grid-column: 1 / span 3;
}

</style>