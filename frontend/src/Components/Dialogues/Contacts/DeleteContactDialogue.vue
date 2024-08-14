<template>
	<v-dialog v-model="IsOpen" persistent scrollable>
		<v-card>
			<v-card-title>Delete Address Book Contact</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">

				Are you sure you want to delete "<span v-if="Contact">{{ Contact.json.name }}</span>"?

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
import { DeleteContactDialogueModelState, IDeleteContactDialogueModelState } from '@/Data/Models/DeleteContactDialogueModelState/DeleteContactDialogueModelState';//tslint:disable-line
import { Contact, IContact } from '@/Data/CRM/Contact/Contact';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {

	},
})
export default class DeleteContactDialogue extends DialogueBase {

	public static GenerateEmpty(): IDeleteContactDialogueModelState {
		return DeleteContactDialogueModelState.GetEmpty();
	}



	constructor() {
		super();
		this.ModelState = DeleteContactDialogueModelState.GetEmpty();
	}

	get DialogueName(): string {
		return 'DeleteContactDialogue';
	}

	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteContactDialogueModelState.GetEmpty();
	}

	protected Confirm(): void {

		const state = this.ModelState as IDeleteContactDialogueModelState;
		const redirect = state.redirectToIndex;

		if (!state.id) {
			return;
		}

		Contact.DeleteIds([state.id]);

		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteContactDialogueModelState.GetEmpty();

		if (redirect) {
			this.$router.push(`/section/contacts/`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}

	protected get Contact(): IContact | null {

		const state = this.ModelState as IDeleteContactDialogueModelState;
		const id = state.id;

		if (!state.id) {
			return null;
		}

		const contact = Contact.ForId(id);
		return contact;
	}



}
</script>