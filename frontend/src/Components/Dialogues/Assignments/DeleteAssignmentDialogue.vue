<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Assignment</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="Assignment">{{AssignmentDescription}}</span>"?

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-assignment-dialogue-confirm"
					>Confirm</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import { DeleteAssignmentDialogueModelState, IDeleteAssignmentDialogueModelState } from '@/Data/Models/DeleteAssignmentDialogueModelState/DeleteAssignmentDialogueModelState';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {
		
	},
})
export default class DeleteAssignmentDialogue extends DialogueBase {
	
	public static GenerateEmpty(): IDeleteAssignmentDialogueModelState {
		return DeleteAssignmentDialogueModelState.GetEmpty();
	}
	
	constructor() {
		super();
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteAssignmentDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		const state = this.ModelState as IDeleteAssignmentDialogueModelState;
		const redirect = state.redirectToIndex;
		
		//console.log('confirm', JSON.stringify(this.ModelState));
		
		if (state.id) {
			Assignment.DeleteIds([state.id]);
		}
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();
		
		if (redirect) {
			this.$router.push(`/section/assignments/index?tab=Open%20%26%20Assigned`).catch(((e: Error) => { }));// eslint-disable-line
		}
		
	}
	
	
	protected get Assignment(): IAssignment | null {
		
		const state = this.ModelState as IDeleteAssignmentDialogueModelState;
		const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		const contact = Assignment.ForId(id);
		return contact;
	}
	
	
	protected get AssignmentDescription(): string | null {
		
		
		
		const state = this.ModelState as IDeleteAssignmentDialogueModelState;
		const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return Assignment.LocationDescriptionForId(id);
	}
	
	
	
}
</script>