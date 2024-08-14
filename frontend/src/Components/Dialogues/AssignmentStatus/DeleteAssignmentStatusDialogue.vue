<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Assignment Status</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="AssignmentStatus">{{AssignmentStatus.json.name}}</span>"?
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-assignment-status-confirm-button"
					>
					Confirm
				</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import { DeleteAssignmentStatusDialogueModelState, IDeleteAssignmentStatusDialogueModelState } from '@/Data/Models/DeleteAssignmentStatusDialogueModelState/DeleteAssignmentStatusDialogueModelState';
import { AssignmentStatus, IAssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {
		
	},
})
export default class DeleteAssignmentStatusDialogue extends DialogueBase {
	
	
	public static GenerateEmpty(): IDeleteAssignmentStatusDialogueModelState {
		return DeleteAssignmentStatusDialogueModelState.GetEmpty();
	}
	
	constructor() {
		super();
		this.ModelState = DeleteAssignmentStatusDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteAssignmentStatusDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAssignmentStatusDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		const state = this.ModelState as IDeleteAssignmentStatusDialogueModelState;
		const redirect = state.redirectToIndex;

		

		console.log('confirm', this.ModelState);
		
		if (state.id) {
			AssignmentStatus.DeleteIds([state.id]);
		}
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAssignmentStatusDialogueModelState.GetEmpty();
		
		if (redirect) {
			this.$router.push(`/section/assignment-status-definitions`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}
	
	
	protected get AssignmentStatus(): IAssignmentStatus | null {
		
		const state = this.ModelState as IDeleteAssignmentStatusDialogueModelState;
		//const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return AssignmentStatus.ForId(state.id);
	}
	
}
</script>