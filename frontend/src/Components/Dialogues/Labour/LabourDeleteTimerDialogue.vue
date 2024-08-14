<template>
	<v-dialog v-model="IsOpen" persistent scrollable :fullscreen="MobileDeviceWidth()">
		<v-card>
			<v-card-title>Delete Timer</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="padding-top: 40px; color: black;">
				<p>Are you sure that you wish to delete this timer?</p>

				<!-- {{this.ModelState}} -->
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer />
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="red darken-1" text @click="Delete()">Delete</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import Dialogues from '@/Utility/Dialogues';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { Notifications } from '@/Data/Models/Notifications/Notifications';

interface LabourDeleteTimerDialogueModelState {
	labourId: string | null;
}

@Component({
	components: {

	},
})
export default class LabourDeleteTimerDialogue extends DialogueBase {

	public static GenerateEmpty(): LabourDeleteTimerDialogueModelState {

		return {
			labourId: null,
		};
	}

	protected MobileDeviceWidth = MobileDeviceWidth;

	constructor() {
		super();
		this.ModelState = LabourDeleteTimerDialogue.GenerateEmpty();
	}


	get DialogueName(): string {
		return 'LabourDeleteTimerDialogue';
	}

	protected Cancel(): void {
		//console.log('Cancel');


		//this.$refs.editor.ResetValidation();
		Dialogues.Close(this.DialogueName);
		this.ModelState = LabourDeleteTimerDialogue.GenerateEmpty();
		//this.$refs.editor.SelectFirstTab();
	}

	protected Delete(): void {

		const state = this.ModelState as LabourDeleteTimerDialogueModelState;

		if (null == state.labourId) {
			Notifications.AddNotification({
				severity: 'error',
				message:
					'An error occured while deleting ' +
					'this timer. Please close and re-open this dialogue. \n' +
					'Error: null == this.ModelState.labourId',
				autoClearInSeconds: 10,
			});
			console.error(state);
			return;
		}

		console.log('Delete', state.labourId);

		Labour.DeleteIds([state.labourId]);


		Dialogues.Close(this.DialogueName);
		this.ModelState = LabourDeleteTimerDialogue.GenerateEmpty();



	}







}
</script>