<template>
	<div class="base" :style="{ 
		minWidth: `${minimized ? cellWidthMinimizedPx : cellWidthPx}px`,
		width: `${minimized ? cellWidthMinimizedPx+'px' : 'initial'}`,
		}">
		<v-dialog
			v-model="ModifyOthersDialogue"
			:fullscreen="MobileDeviceWidth()"
			scrollable
			persistent
			>
			<v-card>
				<v-card-title class="headline">Change Other Assignments?</v-card-title>

				<v-card-text>
					<p>You moved an assignment whose project requires that all assignments share the same schedule.</p>
					
					<v-radio-group v-model="ModifyOthersDialogueChoice">
						<v-radio
							label="Move just this assignment and turn off project schedule syncing?"
							value="MoveJustThisAssignment"
							>
						</v-radio>
						<v-radio
							label="Change the other assignments on this project to match this schedule as well."
							value="ChangeOtherAssignments"
							>
						</v-radio>
					</v-radio-group>
				</v-card-text>

				<v-card-actions>
					<v-btn
						color="green darken-1"
						text
						@click="ModifyOthersDialogue = false"
						>
						Cancel
					</v-btn>
					<v-spacer></v-spacer>

					<v-btn
						color="red darken-1"
						text
						@click="ModifyOthersDialogueCallback"
						>
						Save
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>
		<!---->
		<draggable
			v-if="PermAssignmentCanPush()"
			class="list-group"
			scroll-sensitivity="100"
			:force-fallback="true"
			:value="Events"
			group="people"
			@change="OnChange"
			:move="move1"
			:style="{ 
				//width: `${minimized ? cellWidthMinimizedPx : cellWidthPx}px`,
				flex: 1,
				}"
			>
			<div
				class="list-group-item"
				v-for="(element, index) in Events"
				:key="element.id"
				>
				<SchedulerEvent
					v-model="Events[index]"
					:cellWidthPx="cellWidthPx"
					:cellWidthMinimizedPx="cellWidthMinimizedPx"
					
					:agentId="agentId"
					/>
			</div>
		</draggable>
		<div v-else style="flex: 1;">
			<div
				class="list-group-item"
				v-for="(element, index) in Events"
				:key="element.id"
				>
				<SchedulerEvent
					v-model="Events[index]"
					:cellWidthPx="cellWidthPx"
					:cellWidthMinimizedPx="cellWidthMinimizedPx"
					
					:agentId="agentId"
					/>
			</div>
		</div>
		
		
		
	</div>
</template>
<script lang="ts">



import { Component, Vue, Prop } from 'vue-property-decorator';
import Draggable from 'vuedraggable';
import SchedulerEvent from '@/Components/Scheduler/SchedulerEvent.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { DateTime } from 'luxon';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { Project } from '@/Data/CRM/Project/Project';
import { Agent } from '@/Data/CRM/Agent/Agent';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { ISchedulerLocalTime } from '@/Data/Models/SchedulerLocalTime/SchedulerLocalTime';
import { ISchedulerCellDragData } from '@/Data/Models/SchedulerCellDragData/SchedulerCellDragData';

@Component({
	components: {
		Draggable,
		SchedulerEvent,
	},
})
export default class SchedulerCell extends Vue {
	
	
	@Prop({ default: false }) public readonly minimized!: boolean;
	@Prop({ default: 150 }) public readonly cellWidthPx!: number;
	@Prop({ default: 50 }) public readonly cellWidthMinimizedPx!: number;
	
	@Prop({ default: null }) public readonly agentId!: string | null;
	@Prop({ default: false }) public readonly isUnscheduled!: boolean;
	@Prop({ default: false }) public readonly isFirstAM!: boolean;
	@Prop({ default: false }) public readonly isSecondAM!: boolean;
	@Prop({ default: false }) public readonly isFirstPM!: boolean;
	@Prop({ default: false }) public readonly isSecondPM!: boolean;
	@Prop({ default: null }) public readonly localTimeStart!: ISchedulerLocalTime | null;
	@Prop({ default: null }) public readonly localTimeEnd!: ISchedulerLocalTime | null;
	@Prop({ default: null }) public readonly date!: string;
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	
	protected ModifyOthersDialogue = false;
	protected ModifyOthersDialogueChoice: 'MoveJustThisAssignment' | 'ChangeOtherAssignments' | null = null;
	protected ModifyOthersChangeData: {
		assignmentId: string | null;
		startTimeMode: 'none' | 'morning-first-thing' | 
		'morning-second-thing' | 'afternoon-first-thing' | 
		'afternoon-second-thing' | 'time';
		hasStartISO8601: boolean,
		startISO8601: string | null,
	} = {
		assignmentId: null,
		startTimeMode: 'none',
		hasStartISO8601: false,
		startISO8601: null,
	};
	
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	
	protected get Events(): ISchedulerCellDragData[] {
		
		return Assignment.ForSchedulerCell(
			this.agentId, 
			this.date, 
			this.isUnscheduled, 
			this.isFirstAM,
			this.isSecondAM,
			this.isFirstPM,
			this.isSecondPM,
			this.localTimeStart,
			this.localTimeEnd,
			);
		
		
	}
	
	public mounted(): void {
		this.LoadData();
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
					
					if (null != this.agentId && null == Agent.ForId(this.agentId)) {
						const rtrAgents = Agent.FetchForId(this.agentId);
						if (rtrAgents.completeRequestPromise) {
							promises.push(rtrAgents.completeRequestPromise);
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
	
	// eslint-disable-next-line
	protected move1(evt: any, originalEvent: any): boolean {
		//console.log('move1', evt, originalEvent);
		
		return true;
	}
	
	// eslint-disable-next-line
	protected OnChange(evt: any): void {
		
		console.log('OnChange', evt);
		
		if (evt.hasOwnProperty('added')) {
			this.OnAdded(evt.added);
		} else if (evt.hasOwnProperty('removed')) {
			this.OnRemoved(evt.removed);
		} else {
			console.debug('unhandled event', evt);
		}
		
		
	}
	
	protected OnAdded(evt: {
		element: ISchedulerCellDragData;
		newIndex: number;
	}): void {
		
		console.log('OnAdded', evt);
		
		const eventAssignment = evt.element.assignment as IAssignment;
		if (!eventAssignment ||
			!eventAssignment.id || 
			IsNullOrEmpty(eventAssignment.id)
			) {
			console.error('SchedulerCell.OnAdded added invalid object', evt);
			return;
		}
		
		// Get latest version of dragged assignment.
		const assignment = Assignment.ForId(eventAssignment.id);
		if (!assignment ||
			!assignment.id ||
			!assignment.json) {
			return;
		}
		
		// Always remove the old assignment.
		if (evt.element.forAgentId !== undefined) {
			Assignment.RemoveAgentIdFromId(assignment.id, evt.element.forAgentId);
		}
		
		Assignment.AddAgentIdsToId(assignment.id, this.agentId);
		
		
		
		// Compute New Values
		const dateParsed = DateTime.fromFormat(this.date, 'yyyy-MM-dd', {
			zone: 'local',
			setZone: true,
		});
		
		let startTimeMode: 'none' | 'morning-first-thing' | 
			'morning-second-thing' | 'afternoon-first-thing' | 
			'afternoon-second-thing' | 'time' = 'none';
		let hasStartISO8601 = false;
		let startISO8601: string | null = null;
		
		if (this.isUnscheduled) {
			startTimeMode = 'none';
			hasStartISO8601 = false;
			startISO8601 = null;
		} else if (this.isFirstAM) {
			const dateSelected = DateTime.fromObject(
				{
					year: dateParsed.year,
					month: dateParsed.month,
					day: dateParsed.day,
				}, 
				{
					zone: 'local',
				},
			);
			
			startTimeMode = 'morning-first-thing';
			hasStartISO8601 = true;
			startISO8601 = dateSelected.toUTC().toISO();
		} else if (this.isSecondAM) {
			const dateSelected = DateTime.fromObject({
					year: dateParsed.year,
					month: dateParsed.month,
					day: dateParsed.day,
				}, 
				{
					zone: 'local',
				},
			);
			
			startTimeMode = 'morning-second-thing';
			hasStartISO8601 = true;
			startISO8601 = dateSelected.toUTC().toISO();
		} else if (this.isFirstPM) {
			const dateSelected = DateTime.fromObject({
				year: dateParsed.year,
				month: dateParsed.month,
				day: dateParsed.day,
				}, 
				{
					zone: 'local',
				},
			);
			
			startTimeMode = 'afternoon-first-thing';
			hasStartISO8601 = true;
			startISO8601 = dateSelected.toUTC().toISO();
		} else if (this.isSecondPM) {
			const dateSelected = DateTime.fromObject({
				year: dateParsed.year,
				month: dateParsed.month,
				day: dateParsed.day,
				}, 
				{
					zone: 'local',
				},
			);
			
			startTimeMode = 'afternoon-second-thing';
			hasStartISO8601 = true;
			startISO8601 = dateSelected.toUTC().toISO();
		} else if (this.localTimeStart && this.localTimeEnd) {
			const dateSelected = DateTime.fromObject({
				year: dateParsed.year,
				month: dateParsed.month,
				day: dateParsed.day,
				hour: this.localTimeStart.hour || 0,
				minute: this.localTimeStart.minute || 0,
				second: this.localTimeStart.second || 0,
				}, 
				{
					zone: 'local',
				},
			);
			
			startTimeMode = 'time';
			hasStartISO8601 = true;
			startISO8601 = dateSelected.toUTC().toISO();
		}
		
		
		
		
		const project = Project.ForId(assignment.json.projectId);
		
		
		if (!project || !project.id) {
			
			// If there is no project, just update the time.
			console.debug('no project just update time');
			
			Assignment.UpdateStartTimeForId(eventAssignment.id, {
				startTimeMode,
				hasStartISO8601,
				startISO8601,
			});
			
		} else {
			
			// Get count of assignments on this project.
			const assignments = Assignment.ForProjectIds([project.id]);
			
			
			if (project.json.forceAssignmentsToUseProjectSchedule) {
				
				if (assignments.length > 1) {
					
					// If the project is set that all assignments must share 
					// the same schedule, we need confirmation on what to do.
					
					console.debug('set to force others confirm');
					
					this.ModifyOthersDialogueChoice = null;
					this.ModifyOthersDialogue = true;
					this.ModifyOthersChangeData = {
						assignmentId: eventAssignment.id,
						startTimeMode,
						hasStartISO8601,
						startISO8601,
					};
					
					
				} else {
					
					// 1 assignment, just update the time, but also update the project time.
					
					Project.UpdateStartTimeForId(project.id, {
						startTimeMode,
						hasStartISO8601,
						startISO8601,
					});
					
					Assignment.UpdateStartTimeForId(eventAssignment.id, {
						startTimeMode,
						hasStartISO8601,
						startISO8601,
					});
						
					
				}
				
				
				
				
				
			} else {
				
				// If it isn't set to force others, just update the time.
				console.debug('not set to force others, just update time');
				
				Assignment.UpdateStartTimeForId(eventAssignment.id, {
					startTimeMode,
					hasStartISO8601,
					startISO8601,
				});
				
			}
			
			
			
			
			
			
			
		}
		
		
		
	}
	
	protected ModifyOthersDialogueCallback(): void {
		
		console.log('ModifyOthersDialogueCallback', this.ModifyOthersDialogueChoice, this.ModifyOthersChangeData);
		
		switch (this.ModifyOthersDialogueChoice) {
			case 'MoveJustThisAssignment':
				
				do {
					
					const assignment = Assignment.ForId(this.ModifyOthersChangeData.assignmentId);
					if (!assignment || !assignment.id) {
						break;
					}
					
					const projectId = assignment.json.projectId;
					if (!projectId) {
						break;
					}
					
					Project.UpdateForceAssignmentsToUseProjectScheduleForId(projectId, false);
					
					Assignment.UpdateStartTimeForId(assignment.id, this.ModifyOthersChangeData);
					
					
					
					this.ModifyOthersDialogueChoice = null;
					this.ModifyOthersDialogue = false;
					
				} while (false);
				
				break;
			case 'ChangeOtherAssignments':
				
				do {
					
					const assignment = Assignment.ForId(this.ModifyOthersChangeData.assignmentId);
					if (!assignment || !assignment.id) {
						break;
					}
					
					const projectId = assignment.json.projectId;
					if (!projectId) {
						break;
					}
					
					Project.UpdateStartTimeForId(projectId, this.ModifyOthersChangeData);
					
					
					const allAssignments = Assignment.ForProjectIds([projectId]);
					for (const obj of allAssignments) {
						if (!obj.id) {
							continue;
						}
						Assignment.UpdateStartTimeForId(obj.id, this.ModifyOthersChangeData);
						
					}
					
					this.ModifyOthersDialogueChoice = null;
					this.ModifyOthersDialogue = false;
					
				} while (false);
				
				break;
			default:
				console.error('no option selected in dialogue');
				
				Notifications.AddNotification({
					severity: 'error',
					message: 'You must select an option below, or cancel.',
					autoClearInSeconds: 10,
				});
				
				break;
		}
		
	}
	
	
	
	
	
	// We're not going to do anything for the removed event.
	// eslint-disable-next-line
	protected OnRemoved(evt: any): void {
		//console.log('OnRemoved', evt);
	}
	
	
	
	
	
}
</script>
<style scoped>

.base {
	border-right: 1px solid #ccc;
	border-bottom: 1px dashed #ccc;
	display: flex;
	
}

</style>