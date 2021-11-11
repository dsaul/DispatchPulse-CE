<template>
	<div>
		<v-list
			v-if="PermAssignmentCanRequest()"
			>
			<v-text-field 
				v-if="showFilter"
				autocomplete="newpassword"
				class="mx-4"
				v-model="filter"
				hide-details
				label="Filter"
				prepend-inner-icon="search"
				solo
				style="margin-bottom: 10px;"
				ref="filterField"
				>
			</v-text-field>
			
			<div v-if="PageRows.length != 0">
				<template>
					<div class="text-center" v-if="showTopPagination === true">
						<v-pagination
							v-model="CurrentPage"
							:length="PageCount"
							:total-visible="breadcrumbsVisibleCount"
							>
						</v-pagination>
					</div>
				</template>
					
				<v-list-item-group color="primary">
					<AssignmentListItem
						v-for="(row, index) in PageRows" 
						:key="row.id" 
						v-model="PageRows[index]" 
						:showMenuButton="showMenuButton"
						:isDialogue="isDialogue"
						@ClickEntry="ClickEntry"
						@OpenEntry="OpenEntry"
						@DeleteEntry="DeleteEntry"
						:focusIsProject="focusIsProject"
						:focusIsAgent="focusIsAgent"
						:rootProject="rootProject"
						:disabled="disabled"
						/>
				</v-list-item-group>
				
				<template>
					<div class="text-center">
						<v-pagination
							v-model="CurrentPage"
							:length="PageCount"
							:total-visible="breadcrumbsVisibleCount"
							>
						</v-pagination>
					</div>
				</template>
			</div>
			<div v-else>
				<div v-if="loadingData" style="margin-left: 20px; margin-right: 20px;">
					<content-placeholders>
						<content-placeholders-heading :img="true" />
						<!-- <content-placeholders-text :lines="3" /> -->
					</content-placeholders>
				</div>
				<v-alert
					v-else
					outlined
					type="info"
					elevation="0"
					style="margin-left: 15px; margin-right: 15px; margin-bottom: 0px;"
					>
					{{emptyMessage}}
				</v-alert>
			</div>
		</v-list>
		<PermissionsDeniedAlert v-else />
	</div>
</template>

<script lang="ts">

import AssignmentListItem from '@/Components/ListItems/AssignmentListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import _ from 'lodash';
import { DateTime } from 'luxon';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ListBase from './ListBase';
import { mapGetters } from 'vuex';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { AssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';
import { IProjectNoteStyledText } from '@/Data/CRM/ProjectNoteStyledText/ProjectNoteStyledText';
import { IProjectNoteCheckbox } from '@/Data/CRM/ProjectNoteCheckbox/ProjectNoteCheckbox';
import { IProjectNoteImage } from '@/Data/CRM/ProjectNoteImage/ProjectNoteImage';
import { IProjectNoteVideo } from '@/Data/CRM/ProjectNoteVideo/ProjectNoteVideo';

@Component({
	components: {
		AssignmentListItem,
		PermissionsDeniedAlert,
	},
	computed: {
		...mapGetters([
			'SessionId',
		]),
	},
})
export default class AssignmentsList extends ListBase {
	
	@Prop({ default: false }) public readonly focusIsProject!: boolean;
	@Prop({ default: false }) public readonly focusIsAgent!: boolean;
	
	@Prop({ default: false }) public readonly showOnlyForBillingUsersAgent!: boolean;
	@Prop({ default: false }) public readonly showOnlyOpenAssignments!: boolean;
	@Prop({ default: false }) public readonly showOnlyClosedAssignments!: boolean;
	@Prop({ default: false }) public readonly showOnlyTodayOrEarlier!: boolean;
	@Prop({ default: false }) public readonly showOnlyTasksWithNoStartTime!: boolean;
	@Prop({ default: null })  public readonly showOnlyProjectId!: string;
	@Prop({ default: null })  public readonly showOnlyAgentId!: string;
	@Prop({ default: false }) public readonly showOnlyPastDue!: boolean;
	@Prop({ default: false }) public readonly showOnlyUnassigned!: boolean;
	@Prop({ default: false }) public readonly showOnlyDueWithNoLabour!: boolean;
	@Prop({ default: false }) public readonly showOnlyBilliableReview!: boolean;
	@Prop({ default: false }) public readonly hideAssignmentsWithNoStartTime!: boolean;
	@Prop({ default: false }) public readonly showChildrenOfProjectIdAsWell!: boolean;
	@Prop({ default: null }) public readonly rootProject!: IProject;
	
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: false }) public readonly isReverseSort!: boolean;
	
	@Prop({ default: 'There are no assignments to show.' }) declare public readonly emptyMessage: string;
	
	public $refs!: {
		filterField: Vue,
	};
	
	public SessionId!: string;
	
	
	protected PermAssignmentCanRequest = Assignment.PermAssignmentCanRequest;
	
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		//console.debug('AssignmentsList LoadData()');
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
					
					
					const agentId = Agent.LoggedInAgentId();
					
					// Make sure we have the logged in agent loaded.
					if (null != agentId &&
						!IsNullOrEmpty(agentId) &&
						null == Agent.ForLoggedInAgent()) {
							
						const rtrAgents = Agent.FetchForId(agentId);
						if (rtrAgents.completeRequestPromise) {
							promises.push(rtrAgents.completeRequestPromise);
						}
						
					}
					
					
					
					if (Assignment.PermAssignmentCanRequest()) {
						const rtrAssignments = Assignment.RequestAssignments.Send({
							sessionId: BillingSessions.CurrentSessionId(),
							limitToSessionAgent: this.showOnlyForBillingUsersAgent,
							limitToOpen: this.showOnlyOpenAssignments,
							limitToClosed: this.showOnlyClosedAssignments,
							limitToTodayOrEarlier: this.showOnlyTodayOrEarlier,
							limitToTasksWithNoStartTime: this.showOnlyTasksWithNoStartTime,
							limitToProjectId: this.showOnlyProjectId,
							limitToAgentId: this.showOnlyAgentId,
							limitToPastDue: this.showOnlyPastDue,
							limitToUnassigned: this.showOnlyUnassigned,
							limitToDueWithNoLabour: this.showOnlyDueWithNoLabour,
							limitToBillableReview: this.showOnlyBilliableReview,
							filterAssignmentsWithNoStartTime: this.hideAssignmentsWithNoStartTime,
							showChildrenOfProjectIdAsWell: this.showChildrenOfProjectIdAsWell,
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
	
	public SelectFilterField(): void {
		//console.log('SelectFilterField()', this.$refs.filterField);
		if (this.$refs.filterField) {
			const input = this.$refs.filterField.$el.querySelector('input');
			if (input) {
				input.focus();
			}
		}
	}
	
	protected GetEntryRouteForId(id: string): string {
		return `/section/assignments/${id}?tab=General`;
	}
	
	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteAssignmentDialogue';
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
	
	protected GetRawRows(): Record<string, IAssignment> {
		
		// We need to go through all of these and determine if we need to fetch the projects.
		const assignments = this.$store.state.Database.assignments as Record<string, IAssignment>;
		
		const projectIDsToRequest: string[] = [];
		
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		for (const [key, value] of Object.entries(assignments)) {
			
			const projectId = value.json.projectId;
			if (null != projectId && !IsNullOrEmpty(projectId)) {
				
				if (null == Project.ForId(projectId)) {
					
					projectIDsToRequest.push(projectId);
				}
				
			}
			
		}
		
		if (projectIDsToRequest.length > 0) {
			Project.RequestProjects.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				limitToIds: projectIDsToRequest,
			});
		}
		
		
		
		
		return assignments;
	}
	
	protected RowFilter(o: IAssignment): boolean {
				
		let result = true;
		
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
			
			if (this.showOnlyAgentId) {
				
				const found = _.find(o.json.agentIds, (value) => {
					return value?.trim() === this.showOnlyAgentId.trim();
				});
				
				if (!found) {
					result = false;
					break;
				}
			}
			
			if (this.showOnlyBilliableReview) {
				
				const statusId = o.json.statusId;
				
				if (!statusId || IsNullOrEmpty(statusId)) {
					result = false;
					break;
				}
				
				const status = AssignmentStatus.ForId(statusId);
				if (!status) {
					result = false;
					break;
				}
				
				if (!status.json.isBillableReview) {
					result = false;
					break;
				}
				
			}
			
			
			if (this.showOnlyProjectId) {
				
				let projects = [];
				
				const assignmentEntryProject = Project.ForId(o.json.projectId);
				const showOnlyProject = Project.ForId(this.showOnlyProjectId);
				
				
				if (this.showChildrenOfProjectIdAsWell) {
					
					projects = Project.RecursiveChildProjectsOfId(this.showOnlyProjectId);
				} else {
					
					
					if (showOnlyProject) {
						projects.push(showOnlyProject);
					}
					
				}
				
				//console.log('this.showOnlyProjectId', this.showOnlyProjectId);
				
				
				
				const found = !!_.find(projects, (value) => {
					return assignmentEntryProject?.id === value.id;
				});
				
				if (!found) {
					result = false;
					break;
				}
				
			}
			
			if (this.showOnlyForBillingUsersAgent) {
				// Only show assignments for this billing user's agent.
				
				
				const found = _.find(o.json.agentIds, (value) => {
					return value?.trim() === Agent.LoggedInAgentId()?.trim();
				});
				
				
				if (!found) {
					result = false;
					break;
				}
			}
			
			if (this.showOnlyOpenAssignments) {
				
				
				
				
				if (o.json.statusId == null) {
					
					console.debug('AssignmentList: o.json.statusId == null, showing anyway to be safe', o);
					
				} else {
					
					const assignmentStatus = AssignmentStatus.ForId(o.json.statusId || null);
					
					if (!assignmentStatus) {
						
						console.debug(`!AssignmentList: cannot find status id '${o.json.statusId}' in Database.assignmentStatus` +
							' showing anyway to be safe', o);
						
					} else {						
						if (!assignmentStatus.json.isOpen) {
							result = false;
							break;
						}
					}
				} 
				
			}
			
			if (this.showOnlyClosedAssignments) {
				
				if (o.json.statusId == null) {
					
					console.debug('assignment status id is null, showing anyway to be safe', o);
					
				} else {
					
					const assignmentStatus = AssignmentStatus.ForId(o.json.statusId);
					
					if (!assignmentStatus) {
						
						console.debug(`!AssignmentList: cannot find status id '${o.json.statusId}' in Database.assignmentStatus` +
							' showing anyway to be safe', o);
						
					} else {
						
						if (assignmentStatus.json.isOpen) {
							result = false;
							break;
						}
					}
				}
				
			}
			
			if (this.showOnlyPastDue) {
				
				// Takes into account the project as under get raw rows, we requested all projects for this list.
				
				const endISO8601 = Assignment.EndISO8601ForId(o.id);
				if (!endISO8601) {
					result = false;
					break;
				}
				
				let time = DateTime.fromISO(endISO8601).toLocal();
				if (Assignment.EndTimeModeForId(o.id) === 'none') {
					time = time.set({
						hour: 23,
						minute: 59,
						second: 59,
					});
				}
				
				
				
				const now = DateTime.local();
				
				//if (o.id == "4ed37a9a-2827-4d5b-857e-a7394404ce66") {
				//	console.log('time, now', time.c, now.c);
				//}
				
				
				if (time > now) {
					result = false;
					break;
				}
				
			}
			
			if (this.showOnlyDueWithNoLabour) {
				
				// Takes into account the project as under get raw rows, we requested all projects for this list.
				
				const startISO8601 = Assignment.StartISO8601ForId(o.id);
				if (!startISO8601 || IsNullOrEmpty(startISO8601)) {
					result = false;
					break;
				}
				
				const dbTime = DateTime.fromISO(startISO8601).toLocal();
				const now = DateTime.local();
				
				
				if (dbTime > now) { // not due
					result = false;
					break;
				}
				
				const labourEntries = Labour.ForAssignmentIds([o.id]);
				if (labourEntries.length > 0) {
					result = false;
					break;
				}
				
				
				
			}
			
			if (this.showOnlyUnassigned) {
				
				const noEmpty = _.compact(o.json.agentIds);
				
				
				if (noEmpty.length !== 0) {
					result = false;
					break;
				}
				
				
			}
			
			
			if (this.hideAssignmentsWithNoStartTime) {
				
				// Takes into account the project as under get raw rows, we requested all projects for this list.
				
				const hasStartISO = Assignment.HasStartISO8601ForId(o.id);
				const startISO = Assignment.StartISO8601ForId(o.id);
				
				
				
				if (startISO === null ||
					IsNullOrEmpty(startISO) ||
					hasStartISO === false) {
					result = false;
					break;
				}
				
				
			}
			
			
			
			if (this.showOnlyTodayOrEarlier) {
				
				const hasStartISO = Assignment.HasStartISO8601ForId(o.id);
				const startISO = Assignment.StartISO8601ForId(o.id);
				
				
				if (false === hasStartISO || 
					null == startISO || 
					IsNullOrEmpty(o.json.startISO8601)) {
						
					// Unscheduled could be today
					
				} else {
					// Only show tasks from today or earlier.
					const dbDesiredStart = DateTime.fromISO(startISO);
					const localDesiredStart = dbDesiredStart.toLocal();
					const localNow = DateTime.local();
					const localEndOfDay = localNow.endOf('day');
					if (localDesiredStart > localEndOfDay) {
						result = false;
						break;
					}
				}
				
				
			}
			
			if (this.showOnlyTasksWithNoStartTime) {
				
				const hasStartISO = Assignment.HasStartISO8601ForId(o.id);
				//const startISO = Assignment.StartISO8601ForId(o.id);
				// TODO: Confirm this is correct
				if (hasStartISO === true) {
					result = false;
					break;
				}
				
			}
			
			if (this.showFilter) {
				let haystack = '';
				
				for (const id of o.json.agentIds) {
					
					const agent = Agent.ForId(id);
					
					if (agent) {
						haystack += agent.json.name;
						haystack += ' ';
					}
					
				}
				
				haystack += o.json.workRequested;
				
				if (o.json.projectId != null && !IsNullOrEmpty(o.json.projectId)) {
					
					const project = Project.ForId(o.json.projectId);
					
					if (project) {
						haystack += project.json.name;
						
						for (const addr of project.json.addresses) {
							haystack += addr.value;
							haystack += ' ';
						}
						
						
					}
					
				}
				
				const notes = ProjectNote.ForAssignmentIds([o.id]);
				for (const note of notes) {
					
					switch (note.json.contentType) {
						case 'styled-text':
						{
							const content = note.json.content as IProjectNoteStyledText;
							haystack += content.html;
						}
						break;
						case 'checkbox':
						{
							const content = note.json.content as IProjectNoteCheckbox;
							haystack += content.text;
						}
						break;
						case 'image':
						{
							const content = note.json.content as IProjectNoteImage;
							haystack += content.uri;
						}
						break;
						case 'video':
						{
							const content = note.json.content as IProjectNoteVideo;
							haystack += content.uri;
						}
						break;
						
					}
					
				}
				
				haystack += Assignment.StartISO8601ForId(o.id);
				haystack += Assignment.EndISO8601ForId(o.id);
				
				const status = AssignmentStatus.ForId(o.json.statusId);
				if (status) {
					haystack += status.json.name;
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
	
	protected RowSortBy(o: IAssignment): string {
		return (Assignment.StartISO8601ForId(o.id || null) || '1').toLowerCase();
	}
	
	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
}

</script>