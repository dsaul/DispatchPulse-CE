<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Project</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				Are you sure you want to delete "<span v-if="ProjectDescription">{{ProjectDescription}}</span>"?
			</v-card-text>
			<v-card-text style="color: black;">
				<v-switch
					style="margin-top: 0px;"
					label="Delete this Project's Materials"
					v-model="DeleteProjectMaterialsAsWell"
					>
				</v-switch>
				<v-switch
					style="margin-top: 0px;"
					label="Delete this Project's Notes"
					v-model="DeleteProjectNotesAsWell"
					>
				</v-switch>
				<v-switch
					style="margin-top: 0px;"
					label="Delete this Project's Labour"
					v-model="DeleteProjectLabourAsWell"
					>
				</v-switch>
				<v-switch
					style="margin-top: 0px;"
					label="Delete this Project's Assignments"
					v-model="DeleteProjectAssignmentsAsWell"
					>
				</v-switch>
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-project-dialogue-confirm"
					>Confirm</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import { DeleteProjectDialogueModelState, IDeleteProjectDialogueModelState } from '@/Data/Models/DeleteProjectDialogueModelState/DeleteProjectDialogueModelState'; //tslint:disable-line
import { IProject, Project } from '@/Data/CRM/Project/Project';
import Dialogues from '@/Utility/Dialogues';
import { Material } from '@/Data/CRM/Material/Material';
import { ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { IDeleteContactDialogueModelState } from '@/Data/Models/DeleteContactDialogueModelState/DeleteContactDialogueModelState';

@Component({
	components: {
		
	},
})
export default class DeleteProjectDialogue extends DialogueBase {
	
	public static GenerateEmpty(): IDeleteProjectDialogueModelState {
		return DeleteProjectDialogueModelState.GetEmpty();
	}
	
	constructor() {
		super();
		this.ModelState = DeleteProjectDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteProjectDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteProjectDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		
		const redirect = state.redirectToIndex;
		
		console.log('confirm', this.ModelState);
		
		if (state.id) {
			
			// Delete this project's materials.
			if (state.deleteProjectMaterialsAsWell) {
				
				const ids = [];
				
				const entries = Material.ForProjectIds([state.id]);
				for (const entry of entries) {
					if (entry.id && !IsNullOrEmpty(entry.id)) {
						ids.push(entry.id);
					}
				}
				
				Material.DeleteIds(ids);
			}
			
			// Delete this project's notes.
			if (state.deleteProjectNotesAsWell) {
				
				const ids = [];
				
				const entries = ProjectNote.ForProjectIds([state.id]);
				for (const entry of entries) {
					if (entry.id && !IsNullOrEmpty(entry.id)) {
						ids.push(entry.id);
					}
				}
				
				ProjectNote.DeleteIds(ids);
				
			}
			
			// Delete this project's labours.
			if (state.deleteProjectLabourAsWell) {
				
				const ids = [];
				
				const entries = Labour.ForProjectIds([state.id]);
				for (const entry of entries) {
					if (entry.id && !IsNullOrEmpty(entry.id)) {
						ids.push(entry.id);
					}
				}
				
				Labour.DeleteIds(ids);
				
			}
			
			
			// Delete this project's assignments.
			if (state.deleteProjectAssignmentsAsWell) {
				
				const ids = [];
				
				const entries = Assignment.ForProjectIds([state.id]);
				for (const entry of entries) {
					if (entry.id && !IsNullOrEmpty(entry.id)) {
						ids.push(entry.id);
					}
				}
				
				Assignment.DeleteIds(ids);
			}
			
			
			
			
			Project.DeleteIds([state.id]);
		}
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteProjectDialogueModelState.GetEmpty();
		
		if (redirect) {
			this.$router.push(`/section/projects`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}
	
	
	
	
	protected get DeleteProjectMaterialsAsWell(): boolean {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		
		return state.deleteProjectMaterialsAsWell;
	}
	
	protected set DeleteProjectMaterialsAsWell(flag: boolean) {
		
		//console.debug('set DialogueBase.ModelState', val);
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		state.deleteProjectMaterialsAsWell = flag;
		this.ModelState = state;
	}
	
	protected get DeleteProjectNotesAsWell(): boolean {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		
		return state.deleteProjectNotesAsWell;
	}
	
	protected set DeleteProjectNotesAsWell(flag: boolean) {
		
		//console.debug('set DialogueBase.ModelState', val);
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		state.deleteProjectNotesAsWell = flag;
		this.ModelState = state;
	}
	
	protected get DeleteProjectLabourAsWell(): boolean {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		
		return state.deleteProjectLabourAsWell;
	}
	
	protected set DeleteProjectLabourAsWell(flag: boolean) {
		
		//console.debug('set DialogueBase.ModelState', val);
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		state.deleteProjectLabourAsWell = flag;
		this.ModelState = state;
	}
	
	protected get DeleteProjectAssignmentsAsWell(): boolean {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		
		return state.deleteProjectAssignmentsAsWell;
	}
	
	protected set DeleteProjectAssignmentsAsWell(flag: boolean) {
		
		//console.debug('set DialogueBase.ModelState', val);
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		state.deleteProjectAssignmentsAsWell = flag;
		this.ModelState = state;
	}
	
	
	
	protected get Project(): IProject | null {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		//const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return Project.ForId(state.id);
	}
	
	protected get ProjectDescription(): string | null {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		//const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return Project.CombinedDescriptionForId(state.id);
		
	}
	
}
</script>