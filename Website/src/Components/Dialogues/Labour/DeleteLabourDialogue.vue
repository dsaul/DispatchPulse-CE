<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Labour</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="LabourDescription">{{LabourDescription}}</span>"?
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="red darken-1" text @click="Confirm()">Confirm</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import { DeleteLabourDialogueModelState, IDeleteLabourDialogueModelState } from '@/Data/Models/DeleteLabourDialogueModelState/DeleteLabourDialogueModelState';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {
		
	},
})
export default class DeleteLabourDialogue extends DialogueBase {
	
	public static GenerateEmpty(): IDeleteLabourDialogueModelState {
		return DeleteLabourDialogueModelState.GetEmpty();
	}
	
	constructor() {
		super();
		this.ModelState = DeleteLabourDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteLabourDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteLabourDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		const state = this.ModelState as IDeleteLabourDialogueModelState;
		const redirect = state.redirectToIndex;
		
		console.log('confirm', this.ModelState);
		
		if (redirect) {
			this.$router.push(`/section/labour/`).catch(((e: Error) => { }));// eslint-disable-line
		}
		
		if (state.id) {
			Labour.DeleteIds([state.id]);
		}
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteLabourDialogueModelState.GetEmpty();
	}
	
	protected get Labour(): ILabour | null {
		
		const state = this.ModelState as IDeleteLabourDialogueModelState;
		//const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return Labour.ForId(state.id);
	}
	
	protected get LabourDescription(): string | null {
		
		const state = this.ModelState as IDeleteLabourDialogueModelState;
		const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return Labour.DescriptionForId(id);
		
	}
	
}
</script>