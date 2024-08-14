<template>
	<v-dialog v-model="IsOpen" persistent scrollable>
		<v-card>
			<v-card-title>Confim Deleting Project Note</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">

				Are you sure you want to delete this note?

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer />
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="red darken-1" text @click="Confirm()">Confirm</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import { DeleteProjectNoteDialogueModelState, IDeleteProjectNoteDialogueModelState } from '@/Data/Models/DeleteProjectNoteDialogueModelState/DeleteProjectNoteDialogueModelState';
import Dialogues from '@/Utility/Dialogues';
import { ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { IDeleteContactDialogueModelState } from '@/Data/Models/DeleteContactDialogueModelState/DeleteContactDialogueModelState';

@Component({
	components: {

	},
})
export default class DeleteProjectNoteDialogue extends DialogueBase {

	public static GenerateEmpty(): IDeleteProjectNoteDialogueModelState {
		return DeleteProjectNoteDialogueModelState.GetEmpty();
	}

	constructor() {
		super();
		this.ModelState = DeleteProjectNoteDialogueModelState.GetEmpty();
	}

	get DialogueName(): string {
		return 'DeleteProjectNoteDialogue';
	}

	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteProjectNoteDialogueModelState.GetEmpty();
	}

	protected Confirm(): void {

		const state = this.ModelState as IDeleteContactDialogueModelState;
		const redirect = state.redirectToIndex;

		console.log('confirm', this.ModelState);

		if (state.id) {
			ProjectNote.DeleteIds([state.id]);
		}

		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteProjectNoteDialogueModelState.GetEmpty();

		if (redirect) {
			this.$router.push(`/section/projects`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}

}
</script>