<template>
	<v-dialog v-model="IsOpen" persistent scrollable>
		<v-card>
			<v-card-title>Confirm Log Out</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">

				Are you sure you want to log out? If you have any unsaved changes they will be lost.


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
import { mapActions } from 'vuex';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {

	},
	methods: {
		...mapActions([
			'DestroySession',
		]),
	},
})
export default class ConfirmLogOutDialogue extends DialogueBase {




	public static GenerateEmpty(): Record<string, unknown> {
		return {

		};
	}

	public DestroySession!: () => void;

	constructor() {
		super();
		this.ModelState = ConfirmLogOutDialogue.GenerateEmpty();
	}

	get DialogueName(): string {
		return 'ConfirmLogOutDialogue';
	}

	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = ConfirmLogOutDialogue.GenerateEmpty();
	}

	protected Confirm(): void {

		this.DestroySession();
		Dialogues.Close(this.DialogueName);
		this.ModelState = ConfirmLogOutDialogue.GenerateEmpty();

	}

}
</script>