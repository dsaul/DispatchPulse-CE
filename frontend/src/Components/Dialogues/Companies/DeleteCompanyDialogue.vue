<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Company</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				Are you sure you want to delete "<span v-if="Company">{{Company.json.name}}</span>"?
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
import { Company, ICompany } from '@/Data/CRM/Company/Company';
import { DeleteCompanyDialogueModelState, IDeleteCompanyDialogueModelState } from '@/Data/Models/DeleteCompanyDialogueModelState/DeleteCompanyDialogueModelState';//tslint:disable-line
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {
		
	},
})
export default class DeleteCompanyDialogue extends DialogueBase {
	
	public static GenerateEmpty(): IDeleteCompanyDialogueModelState {
		return DeleteCompanyDialogueModelState.GetEmpty();
	}
	
	constructor() {
		super();
		this.ModelState = DeleteCompanyDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteCompanyDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteCompanyDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		const state = this.ModelState as IDeleteCompanyDialogueModelState;
		const redirect = state.redirectToIndex;
		
		console.log('confirm', this.ModelState);
		
		Company.DeleteIds([this.ModelState.id]);
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteCompanyDialogueModelState.GetEmpty();
		
		if (redirect) {
			this.$router.push(`/section/companies`).catch(((e: Error) => { }));// eslint-disable-line
		}
		
	}
	
	protected get Company(): ICompany | null {
		
		const state = this.ModelState as IDeleteCompanyDialogueModelState;
		const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		const contact = Company.ForId(id);
		return contact;
	}
	
}
</script>