<template>
	<v-dialog v-model="IsOpen" persistent scrollable>
		<v-card>
			<v-card-title>Delete Employment Status</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">

				Are you sure you want to delete "<span v-if="EmploymentStatus">{{ EmploymentStatus.json.name }}</span>"?

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer />
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="red darken-1" text @click="Confirm()" class="e2e-delete-employment-status-confirm-button">
					Confirm
				</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import { DeleteEmploymentStatusDialogueModelState, IDeleteEmploymentStatusDialogueModelState } from '@/Data/Models/DeleteEmploymentStatusDialogueModelState/DeleteEmploymentStatusDialogueModelState';
import { EmploymentStatus, IEmploymentStatus } from '@/Data/CRM/EmploymentStatus/EmploymentStatus';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {

	},
})
export default class DeleteEmploymentStatusDialogue extends DialogueBase {


	public static GenerateEmpty(): IDeleteEmploymentStatusDialogueModelState {
		return DeleteEmploymentStatusDialogueModelState.GetEmpty();
	}

	constructor() {
		super();
		this.ModelState = DeleteEmploymentStatusDialogueModelState.GetEmpty();
	}

	get DialogueName(): string {
		return 'DeleteEmploymentStatusDialogue';
	}

	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteEmploymentStatusDialogueModelState.GetEmpty();
	}

	protected Confirm(): void {

		const state = this.ModelState as IDeleteEmploymentStatusDialogueModelState;
		const redirect = state.redirectToIndex;

		console.log('confirm', this.ModelState);

		if (state.id) {
			EmploymentStatus.DeleteIds([state.id]);
		}

		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteEmploymentStatusDialogueModelState.GetEmpty();

		if (redirect) {
			this.$router.push(`/section/employment-status-definitions`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}

	protected get EmploymentStatus(): IEmploymentStatus | null {

		const state = this.ModelState as IDeleteEmploymentStatusDialogueModelState;
		//const id = state.id;

		if (!state.id) {
			return null;
		}

		return EmploymentStatus.ForId(state.id);
	}

}
</script>