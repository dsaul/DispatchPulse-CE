<template>
	<v-dialog v-model="IsOpen" persistent scrollable>
		<v-card>
			<v-card-title>Delete Man Hours Entry</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="EstimatingManHours">{{EstimatingManHours.json.item}}</span>"?
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-man-hours-confirm-button"
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
import { DeleteManHourDialogueModelState, IDeleteManHourDialogueModelState } from '@/Data/Models/DeleteManHourDialogueModelState/DeleteManHourDialogueModelState';//tslint:disable-line
import { EstimatingManHours, IEstimatingManHours } from '@/Data/CRM/EstimatingManHours/EstimatingManHours';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {
		
	},
})
export default class DeleteManHourDialogue extends DialogueBase {
	
	
	public static GenerateEmpty(): IDeleteManHourDialogueModelState {
		return DeleteManHourDialogueModelState.GetEmpty();
	}
	
	constructor() {
		super();
		this.ModelState = DeleteManHourDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteManHourDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteManHourDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		const state = this.ModelState as IDeleteManHourDialogueModelState;
		const redirect = state.redirectToIndex;

		console.log('confirm', this.ModelState);
		
		if (state.id) {
			EstimatingManHours.DeleteIds([state.id]);
		}
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteManHourDialogueModelState.GetEmpty();
		
		if (redirect) {
			this.$router.push(`/section/man-hours-definitions`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}
	
	protected get EstimatingManHours(): IEstimatingManHours | null {
		
		const state = this.ModelState as IDeleteManHourDialogueModelState;
		//const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return EstimatingManHours.ForId(state.id);
	}
	
}
</script>