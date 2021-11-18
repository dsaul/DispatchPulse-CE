<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>Merge Projects</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<v-alert
					outlined
					type="warning"
					elevation="0"
					style="margin-top: 20px;"
					>
					It would not be a bad idea to take a backup in Settings before merging projects.
				</v-alert>
				
				<v-stepper
					v-model="currentStep"
					vertical
					style="-webkit-box-shadow: inherit; box-shadow: inherit;"
					>
					<v-stepper-step
						:complete="ProjectIDsValidates"
						editable
						step="1"
						class="e2e-merge-dialogue-step-1"
						>
						Select the projects to merge.
						<small>Select as many as required.</small>
					</v-stepper-step>

					<v-stepper-content step="1">
						<ProjectSelectFieldArrayAdapter 
							v-model="ProjectIDs"
							:showDetails="false"
							:isDialogue="true"
							class="e2e-merge-projects-select-field-adapter"
							/>
					</v-stepper-content>
					
					<v-stepper-step
						:complete="SelectedName != NULL_SENTINEL"
						editable
						step="2"
						class="e2e-merge-dialogue-step-2"
						>
						Choose a project name.
					</v-stepper-step>

					<v-stepper-content step="2">
						
						<v-alert
							outlined
							type="info"
							elevation="0"
							style=""
							v-if="!AreThereProjectsSelected"
							>
							No projects selected above.
						</v-alert>
						<v-alert
							outlined
							type="warning"
							elevation="0"
							style=""
							v-if="AreThereProjectsSelected && ProjectNames.length === 0"
							>
							No names on any of the selected projects.
						</v-alert>
						
						<v-radio-group
							v-if="AreThereProjectsSelected"
							v-model="SelectedName"
							>
							<v-radio
								label="No name selected."
								:value="NULL_SENTINEL"
								>
							</v-radio>
							<v-radio
								v-for="(name, index) of ProjectNames"
								:key="index"
								
								:label="name"
								:value="name">
							</v-radio>
						</v-radio-group>
						
						
					</v-stepper-content>

					<v-stepper-step
						:complete="SelectedParent != NULL_SENTINEL"
						editable
						step="3"
						class="e2e-merge-dialogue-step-3"
						>
						Choose a parent project.
					</v-stepper-step>

					<v-stepper-content step="3">
						
						<v-alert
							outlined
							type="info"
							elevation="0"
							style=""
							v-if="!AreThereProjectsSelected"
							>
							No projects selected above.
						</v-alert>
						
						<v-radio-group
							v-if="AreThereProjectsSelected"
							v-model="SelectedParent"
							>
							<v-radio
								label="No parent project selected."
								:value="NULL_SENTINEL"
								>
							</v-radio>
							<v-radio
								v-for="(project, index) of ParentProjects"
								:key="index"
								
								:label="ProjectCombinedDescriptionForId(project.id)"
								:value="project.id">
							</v-radio>
						</v-radio-group>
						
						<v-alert
							outlined
							type="warning"
							elevation="0"
							style=""
							v-if="AreThereProjectsSelected && ParentProjects.length === 0"
							>
							No parent projects on any of the selected projects.
						</v-alert>
						
						
					</v-stepper-content>

					<v-stepper-step
						:complete="SelectedStatus != NULL_SENTINEL"
						editable
						step="4"
						class="e2e-merge-dialogue-step-4"
						>
						Choose a status.
					</v-stepper-step>

					<v-stepper-content step="4">
						
						<v-alert
							outlined
							type="info"
							elevation="0"
							style=""
							v-if="!AreThereProjectsSelected"
							>
							No projects selected above.
						</v-alert>
						
						<v-radio-group
							v-if="AreThereProjectsSelected"
							v-model="SelectedStatus"
							>
							<v-radio
								label="No status selected."
								:value="NULL_SENTINEL"
								>
							</v-radio>
							<v-radio
								v-for="(status, index) of Statuses"
								:key="index"
								
								:label="status.name"
								:value="status.id">
							</v-radio>
						</v-radio-group>
						
						<v-alert
							outlined
							type="warning"
							elevation="0"
							style=""
							v-if="AreThereProjectsSelected && Statuses.length === 0"
							>
							No status on any of the selected projects.
						</v-alert>
						
						
						
						
						
						
						
						
					</v-stepper-content>

					<v-stepper-step
						step="5"
						editable
						:complete="SelectedSchedule != NULL_SENTINEL"
						class="e2e-merge-dialogue-step-5"
						>
						Choose a schedule.
					</v-stepper-step>
					<v-stepper-content step="5">
						
						<v-alert
							outlined
							type="info"
							elevation="0"
							style=""
							v-if="!AreThereProjectsSelected"
							>
							No projects selected above.
						</v-alert>
						
						
						<v-radio-group
							v-if="AreThereProjectsSelected"
							v-model="SelectedSchedule"
							>
							<v-radio
								label="No schedule selected."
								:value="NULL_SENTINEL"
								>
							</v-radio>
							<v-radio
								v-for="(schedule, index) of Schedules"
								:key="index"
								
								:label="`Start: ${schedule.startDescription} End: ${schedule.endDescription}`"
								:value="schedule.fromProject">
							</v-radio>
						</v-radio-group>
					</v-stepper-content>
					
					<v-stepper-step
						step="6"
						editable
						:complete="currentStep > 6"
						class="e2e-merge-dialogue-step-6"
						>
						Project Settings
					</v-stepper-step>
					<v-stepper-content step="6">
						
						<p>
							These settings are decided for you so that they don't change anything. 
							If you want to change them, you can do so in the project after it is merged.
						</p>
						
						<v-switch
							v-model="ComputedForceLabourAsExtra"
							:label="`Force labour to be extra.`"
							:disabled="true"
							>
						</v-switch>
						
						<v-switch
							v-model="ComputedForceAssignmentsToUseProjectSchedule"
							:label="`Force assignments to use project schedule.`"
							:disabled="true"
							>
						</v-switch>
						
						<!-- <v-alert
							outlined
							type="info"
							elevation="0"
							style=""
							v-if="!AreThereProjectsSelected"
							>
							No projects selected above.
						</v-alert>
						
						
						<v-radio-group
							v-if="AreThereProjectsSelected"
							v-model="SelectedSchedule"
							>
							<v-radio
								label="No schedule selected."
								:value="NULL_SENTINEL"
								>
							</v-radio>
							<v-radio
								v-for="(schedule, index) of Schedules"
								:key="index"
								
								:label="`Start: ${schedule.startDescription} End: ${schedule.endDescription}`"
								:value="schedule.fromProject">
							</v-radio>
						</v-radio-group> -->
					</v-stepper-content>
					
				</v-stepper>
				

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn
					color="blue darken-1"
					text
					@click="ResetAndClose()"
					
					>
					Close
				</v-btn>
				<v-btn
					color="blue darken-1"
					text
					@click="Next()"
					class="e2e-merge-projects-dialogue-next-button"
					>
					Next
				</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import AssistantDialogueBase from '@/Components/Dialogues/AssistantDialogueBase';
import ProjectSelectFieldArrayAdapter from '@/Components/Rows/ProjectSelectFieldArrayAdapter.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { IMaterial, Material } from '@/Data/CRM/Material/Material';
import { ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

interface MergeProjectsDialogueModelState {
	projectIds: Array<string | null>;
	selectedName: string | null; // just the name
	selectedParentProject: string | null; // parent project id
	selectedStatus: string | null; // status id
	selectedSchedule: string | null; // project id the schedule is on
}

interface IScheduleIntermediary {
	fromProject: string;
	hasStartISO8601: boolean;
	startISO8601: string | null;
	startTimeMode: 'none' | 'morning-first-thing' | 
			'morning-second-thing' | 'afternoon-first-thing' | 
			'afternoon-second-thing' | 'time';
	hasEndISO8601: boolean;
	endTimeMode: 'none' | 'time';
	endISO8601: string | null;
	startDescription: string;
	endDescription: string;
}

@Component({
	components: {
		ProjectSelectFieldArrayAdapter,
	},
})
export default class MergeProjectsDialogue extends AssistantDialogueBase {
	
	public static GenerateEmpty(): MergeProjectsDialogueModelState {
		return {
			projectIds: [null, null],
			selectedName: MergeProjectsDialogue.NULL_SENTINEL,
			selectedParentProject: MergeProjectsDialogue.NULL_SENTINEL,
			selectedStatus: MergeProjectsDialogue.NULL_SENTINEL,
			selectedSchedule: MergeProjectsDialogue.NULL_SENTINEL,
		};
	}
	
	protected static NULL_SENTINEL = `__NULL_SENTINEL_${GenerateID()}__`;
	protected get NULL_SENTINEL(): string {
		return MergeProjectsDialogue.NULL_SENTINEL;
	}
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	protected ProjectCombinedDescriptionForId = Project.CombinedDescriptionForId;
	protected ProjectStartScheduleDescriptionForId = Project.StartScheduleDescriptionForId;
	protected ProjectEndScheduleDescriptionForId = Project.EndScheduleDescriptionForId;
	
	constructor() {
		super();
		this.ModelState = MergeProjectsDialogue.GenerateEmpty();
	}
	
	get MaxSteps(): number {
		return 6;
	}
	
	get DialogueName(): string {
		return 'MergeProjectsDialogue';
	}
	
	
	
	protected ResetAndClose(): void {
		this.ModelState = MergeProjectsDialogue.GenerateEmpty();
		Dialogues.Close(this.DialogueName);
		this.currentStep = 1;
	}
	
	protected Finalize(): void {
		console.log('Finalize()');
		
		const state = this.ModelState as MergeProjectsDialogueModelState;
		
		const filteredProjectIds = _.filter(state.projectIds, (o) => o != null) as string[];
		
		console.debug('filteredProjectIds', filteredProjectIds);
		
		// Validate
		if (filteredProjectIds.length < 2) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Not enough projects have been selected, we must merge at least 2.',
				autoClearInSeconds: 10,
			});
			this.currentStep = 1;
			return;
		}
		
		// Only allow null name if none of the projects had a name.
		if (state.selectedName === this.NULL_SENTINEL && this.ProjectNames.length > 0) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'You must select a name.',
				autoClearInSeconds: 10,
			});
			this.currentStep = 2;
			return;
		}
		
		if (state.selectedName === this.NULL_SENTINEL) {
			// Blank names really aren't allowed anymore, put in a default.
			state.selectedName = 'No project name.';
			this.ModelState = state;
		}
		
		if (state.selectedParentProject === this.NULL_SENTINEL && this.ParentProjects.length > 0) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'You must select a parent project.',
				autoClearInSeconds: 10,
			});
			this.currentStep = 3;
			return;
		}
		
		if (state.selectedStatus === this.NULL_SENTINEL && this.Statuses.length > 0) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'You must select a status.',
				autoClearInSeconds: 10,
			});
			this.currentStep = 4;
			return;
		}
		
		if (state.selectedSchedule === this.NULL_SENTINEL && this.Schedules.length > 0) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'You must select a schedule.',
				autoClearInSeconds: 10,
			});
			this.currentStep = 5;
			return;
		}
		
		
		const newProject: IProject = Project.GetEmpty();
		if (!newProject.id) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Error during merge.',
				autoClearInSeconds: 10,
			});
			return;
		}
		newProject.json.parentId = state.selectedParentProject === this.NULL_SENTINEL ? null : state.selectedParentProject;
		newProject.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		newProject.json.name = state.selectedName === this.NULL_SENTINEL ? null : state.selectedName;
		newProject.json.statusId = state.selectedStatus === this.NULL_SENTINEL ? null : state.selectedStatus;
		newProject.json.forceLabourAsExtra = this.ComputedForceLabourAsExtra;
		newProject.json.forceAssignmentsToUseProjectSchedule = this.ComputedForceAssignmentsToUseProjectSchedule;
		
		// Mergable data
		for (const projectId of state.projectIds) {
			const project = Project.ForId(projectId);
			if (!project) {
				continue;
			}
			
			for (const o of project.json.addresses) {
				newProject.json.addresses.push(o);
			}
			for (const o of project.json.contacts) {
				newProject.json.contacts.push(o);
			}
			for (const o of project.json.companies) {
				newProject.json.companies.push(o);
			}
		}
		
		// Schedule
		if (state.selectedSchedule !== this.NULL_SENTINEL) {
			
			const scheduleProject = Project.ForId(state.selectedSchedule);
			if (scheduleProject) {
				
				newProject.json.hasStartISO8601 = scheduleProject.json.hasStartISO8601;
				newProject.json.startTimeMode = scheduleProject.json.startTimeMode;
				newProject.json.startISO8601 = scheduleProject.json.startISO8601;
				
				newProject.json.hasEndISO8601 = scheduleProject.json.hasEndISO8601;
				newProject.json.endTimeMode = scheduleProject.json.endTimeMode;
				newProject.json.endISO8601 = scheduleProject.json.endISO8601;
			}
			
		}
		console.debug('newProject', newProject);
		
		
		// Request all of the project's data.
		
		const promises: Array<Promise<any>> = [];
		
		// Project Notes
		const rtrProjectNotes = ProjectNote.RequestProjectNotes.Send({
			sessionId: BillingSessions.CurrentSessionId(),
		});
		if (rtrProjectNotes.completeRequestPromise) {
			promises.push(rtrProjectNotes.completeRequestPromise);
		}
		
		// Assignments
		for (const projectId of state.projectIds) {
			const project = Project.ForId(projectId);
			if (!project) {
				continue;
			}
			
			const rtrAssignments = Assignment.RequestAssignments.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				limitToProjectId: projectId,
			});
			if (rtrAssignments.completeRequestPromise) {
				promises.push(rtrAssignments.completeRequestPromise);
			}
		}
		
		// Materials
		const rtrMaterials = Material.RequestMaterials.Send({
			sessionId: BillingSessions.CurrentSessionId(),
		});
		if (rtrMaterials.completeRequestPromise) {
			promises.push(rtrMaterials.completeRequestPromise);
		}
		
		// Labour
		for (const projectId of state.projectIds) {
			const project = Project.ForId(projectId);
			if (!project) {
				continue;
			}
			
			const rtrLabour = Labour.RequestLabour.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				limitToProjectId: projectId,
			});
			if (rtrLabour.completeRequestPromise) {
				promises.push(rtrLabour.completeRequestPromise);
			}
		}
		
		
		if (promises.length > 0) {
			
			const groupPromise = Promise.all(promises);
			
			groupPromise.catch((err) => {
				Notifications.AddNotification({
					severity: 'error',
					message: `Unable to request all information to start merge, we got the following error: ${err.message}`,
					autoClearInSeconds: 10,
				});
			});
			
			groupPromise.then(() => {
				
				if (!newProject.id) {
					Notifications.AddNotification({
						severity: 'error',
						message: 'Error during merge.',
						autoClearInSeconds: 10,
					});
					return;
				}
				
				const notesToModify = ProjectNote.ForProjectIds(filteredProjectIds);
				console.debug('notesToModify', notesToModify);
				
				const assignmentsToModify = Assignment.ForProjectIds(filteredProjectIds);
				console.debug('assignmentsToModify', assignmentsToModify);
				
				const materialsToModify = Material.ForProjectIds(filteredProjectIds);
				console.debug('materialsToModify', materialsToModify);
				
				const labourEntriesToModify = Labour.ForProjectIds(filteredProjectIds);
				console.debug('labourEntriesToModify', labourEntriesToModify);
				
				const payload: Record<string, IProject> = {};
				payload[newProject.id] = newProject;
				Project.UpdateIds(payload);
				
				// Update all notes.
				const notesPayload: Record<string, IProjectNote> = {};
				for (const o of notesToModify) {
					const clone = _.cloneDeep(o);
					if (!clone.id) {
						console.error('!clone.id');
						continue;
					}
					clone.json.projectId = newProject.id;
					clone.lastModifiedISO8601 = DateTime.utc().toISO();
					clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
					notesPayload[clone.id] = clone;
				}
				ProjectNote.UpdateIds(notesPayload);
				
				// Update all assignments.
				const assignmentsPayload: Record<string, IAssignment> = {};
				for (const o of assignmentsToModify) {
					const clone = _.cloneDeep(o);
					if (!clone.id) {
						console.error('!clone.id');
						continue;
					}
					clone.json.projectId = newProject.id;
					clone.lastModifiedISO8601 = DateTime.utc().toISO();
					clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
					assignmentsPayload[clone.id] = clone;
				}
				
				Assignment.UpdateIds(assignmentsPayload);
				
				// Update all materials.
				const materialsPayload: Record<string, IMaterial> = {};
				for (const o of materialsToModify) {
					const clone = _.cloneDeep(o);
					if (!clone.id) {
						console.error('!clone.id');
						continue;
					}
					clone.json.projectId = newProject.id;
					clone.lastModifiedISO8601 = DateTime.utc().toISO();
					clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
					materialsPayload[clone.id] = clone;
				}
				Material.UpdateIds(materialsPayload);
				
				// Update all labour.
				const labourPayload: Record<string, ILabour> = {};
				for (const o of labourEntriesToModify) {
					const clone = _.cloneDeep(o);
					if (!clone.id) {
						console.error('!clone.id');
						continue;
					}
					clone.json.projectId = newProject.id;
					clone.lastModifiedISO8601 = DateTime.utc().toISO();
					clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
					labourPayload[clone.id] = clone;
				}
				Labour.UpdateIds(labourPayload);
				
				
				
				
				
				// Delete old projects.
				Project.DeleteIds(filteredProjectIds);

				
				this.ResetAndClose();
				
				
				
				
			});
		}
		
		
		
		
		
		
		
		
		
	}
	
	
	
	
	get ProjectIDs(): Array<string | null> {
		
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to get on null state, returning [].');
			return [];
		}
		
		if (!state.projectIds || state.projectIds.length === 0) {
			return [];
		}
		
		return state.projectIds;
		
	}
	
	set ProjectIDs(val: Array<string | null>) {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to set on null state, returning.');
			return;
		}
		
		state.projectIds = val;
		
		state.selectedName = this.NULL_SENTINEL;
		state.selectedStatus = this.NULL_SENTINEL;
		state.selectedParentProject = this.NULL_SENTINEL;
		
		this.ModelState = state;
	}
	
	
	get ProjectIDsValidates(): boolean {
		
		const state = this.ModelState as MergeProjectsDialogueModelState;
		
		const filtered = _.filter(state.projectIds, (o) => o != null) as string[];
		
		return filtered.length >= 2;
		
	}
	
	
	get SelectedName(): string | null {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to get on null state, returning null.');
			return null;
		}
		
		if (IsNullOrEmpty(state.selectedName)) {
			return null;
		}
		
		
		return state.selectedName;
	}
	
	set SelectedName(val: string | null) {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to set on null state, returning.');
			return;
		}
		
		state.selectedName = val;
		this.ModelState = state;
	}
	
	get SelectedParent(): string | null {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to get on null state, returning.');
			return null;
		}
		
		if (IsNullOrEmpty(state.selectedParentProject)) {
			return null;
		}
		
		return state.selectedParentProject;
	}
	
	set SelectedParent(val: string | null) {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to set on null state, returning null.');
			return;
		}
		
		state.selectedParentProject = val;
		this.ModelState = state;
	}
	
	
	get SelectedStatus(): string | null {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to get on null state, returning.');
			return null;
		}
		
		if (IsNullOrEmpty(state.selectedStatus)) {
			return null;
		}
		
		return state.selectedStatus;
	}
	
	set SelectedStatus(val: string | null) {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to set on null state, returning.');
			return;
		}
		
		state.selectedStatus = val;
		this.ModelState = state;
	}
	
	get SelectedSchedule(): string | null {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to get on null state, returning.');
			return null;
		}
		
		if (!state.selectedSchedule) {
			return null;
		}
		
		return state.selectedSchedule;
	}
	
	set SelectedSchedule(val: string | null) {
		const state = this.ModelState as MergeProjectsDialogueModelState;
		if (!state) {
			console.warn('Attempted to set on null state, returning.');
			return;
		}
		
		state.selectedSchedule = val;
		this.ModelState = state;
	}
	
	get ComputedForceAssignmentsToUseProjectSchedule(): boolean {
		
		const state = this.ModelState as MergeProjectsDialogueModelState;
		
		const projectsIds = state.projectIds;
		if (!projectsIds || projectsIds.length === 0) {
			return true;
		}
		
		for (const projectId of projectsIds) {
			
			const project = Project.ForId(projectId);
			if (!project) {
				continue;
			}
			
			if (!project.json.forceAssignmentsToUseProjectSchedule) {
				return false;
			}
			
			
		}
		
		return true;
	}
	
	get ComputedForceLabourAsExtra(): boolean {
		
		const state = this.ModelState as MergeProjectsDialogueModelState;
		
		const projectsIds = state.projectIds;
		if (!projectsIds || projectsIds.length === 0) {
			return true;
		}
		
		for (const projectId of projectsIds) {
			
			const project = Project.ForId(projectId);
			if (!project) {
				continue;
			}
			
			if (!project.json.forceLabourAsExtra) {
				return false;
			}
			
			
		}
		
		return true;
		
	}
	
	
	
	
	
	
	
	
	get ProjectNames(): string[] {
		
		if (!this.ModelState) {
			console.warn('Attempted to get on null state, returning [].');
			return [];
		}
		
		console.log('this.ModelState.projectIds', this.ModelState.projectIds);
		
		const ret = [];
		
		for (const id of this.ModelState.projectIds) {
			if (!id) {
				continue;
			}
			
			const name = Project.NameForId(id);
			if (!name || IsNullOrEmpty(name)) {
				continue;
			}
			
			ret.push(name);
		}
		
		return ret;
	}
	
	get ParentProjects(): any[] {
		
		const state = this.ModelState as MergeProjectsDialogueModelState;
		
		if (!state) {
			console.warn('Attempted to get on null state, returning [].');
			return [];
		}
		
		const ret = [];
		
		for (const id of state.projectIds) {
			if (!id) {
				continue;
			}
			
			const project = Project.ForId(id);
			if (!project || !project.json) {
				continue;
			}
			
			const parentId = project.json.parentId;
			if (!parentId || IsNullOrEmpty(parentId)) {
				continue;
			}
			
			// If one of the parent projects is selected, we don't want recursive parents.
			if (_.find(state.projectIds, (o) => o === parentId)) {
				continue;
			}
			
			const parentProject = Project.ForId(parentId);
			if (!parentProject || !parentProject.json) {
				continue;
			}
			
			let parentAddress = '';
			if (parentProject && parentProject.json &&
				parentProject.json.addresses) {
				
				for (let i = 0; i < parentProject.json.addresses.length; i++) {
					const address = parentProject.json.addresses[i];
					
					parentAddress += address.value;
					if (i !== parentProject.json.addresses.length - 1) {
						parentAddress += ', ';
					}
				}
				
					
			}
			
			ret.push({
				id: parentId,
				parentName: parentProject.json.name,
				parentAddresses: parentAddress,
			});
		}
		
		return ret;
		
		
	}
	
	get Statuses(): any[] {
		
		if (!this.ModelState) {
			console.warn('Attempted to get on null state, returning [].');
			return [];
		}
		
		const ret = [];
		
		for (const id of this.ModelState.projectIds) {
			if (!id) {
				continue;
			}
			
			const project = Project.ForId(id);
			if (!project || !project.json) {
				continue;
			}
			
			const statusId = project.json.statusId;
			if (!statusId || IsNullOrEmpty(statusId)) {
				continue;
			}
			
			const status = ProjectStatus.ForId(statusId);
			if (!status) {
				continue;
			}
			
			
			if (!_.find(ret, (o) => o.id === statusId)) {
				ret.push({
					id: statusId,
					name: status.json.name,
				});
			}
			
			
		}
		
		return ret;
		
		
	}
	
	
	
	
	get Schedules(): IScheduleIntermediary[] {
		
		if (!this.ModelState) {
			console.warn('Attempted to get on null state, returning [].');
			return [];
		}
		
		const ret = [] as IScheduleIntermediary[];
		
		for (const id of this.ModelState.projectIds) {
			
			if (!id) {
				continue;
			}
			
			const project = Project.ForId(id);
			if (!project || !project.json) {
				continue;
			}
			
			
			
			const schedule: IScheduleIntermediary = {
				fromProject: id,
				startDescription: Project.StartScheduleDescriptionForId(id),
				endDescription: Project.EndScheduleDescriptionForId(id),
				hasStartISO8601: project.json.hasStartISO8601,
				startISO8601: project.json.startISO8601,
				startTimeMode: project.json.startTimeMode,
				hasEndISO8601: project.json.hasEndISO8601,
				endTimeMode: project.json.endTimeMode,
				endISO8601: project.json.endISO8601,
			};
			
			ret.push(schedule);
		}
		
		return ret;
	}
	
	
	get AreThereProjectsSelected(): boolean {
		
		if (!this.ModelState) {
			console.warn('Attempted to get on null state, returning false.');
			return false;
		}
		
		
		for (const id of this.ModelState.projectIds) {
			if (id) {
				return true;
			}
		}
		
		return false;
	}
	
	
	
}
</script>