<template>
	<v-dialog v-model="IsOpen" persistent scrollable>
		<v-card>
			<v-card-title>Delete Project Status</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">

				Are you sure you want to delete "<span v-if="ProjectStatus">{{ ProjectStatus.json.name }}</span>"?

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer />
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="red darken-1" text @click="Confirm()" class="e2e-delete-project-status-confirm-button">
					Confirm
				</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import { DeleteAssignmentDialogueModelState, IDeleteAssignmentDialogueModelState } from '@/Data/Models/DeleteAssignmentDialogueModelState/DeleteAssignmentDialogueModelState';
import { IProjectStatus, ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import Dialogues from '@/Utility/Dialogues';
import { IDeleteContactDialogueModelState } from '@/Data/Models/DeleteContactDialogueModelState/DeleteContactDialogueModelState';

@Component({
	components: {

	},
})
export default class DeleteProjectStatusDialogue extends DialogueBase {


	public static GenerateEmpty(): IDeleteAssignmentDialogueModelState {
		return DeleteAssignmentDialogueModelState.GetEmpty();
	}

	constructor() {
		super();
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();
	}

	get DialogueName(): string {
		return 'DeleteProjectStatusDialogue';
	}

	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();
	}

	protected Confirm(): void {

		const state = this.ModelState as IDeleteContactDialogueModelState;
		const redirect = state.redirectToIndex;

		console.log('confirm', this.ModelState);

		if (state.id) {
			ProjectStatus.DeleteIds([state.id]);
		}

		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();

		if (redirect) {
			this.$router.push(`/section/project-status-definitions`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}

	protected get ProjectStatus(): IProjectStatus | null {

		const state = this.ModelState as IDeleteContactDialogueModelState;
		//const id = state.id;

		if (!state.id) {
			return null;
		}

		return ProjectStatus.ForId(state.id);
	}

}
</script>