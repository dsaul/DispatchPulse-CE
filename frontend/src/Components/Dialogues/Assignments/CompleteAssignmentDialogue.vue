<template>
	<v-dialog v-model="IsOpen" persistent scrollable :fullscreen="MobileDeviceWidth()">
		<v-card>
			<v-card-title>Complete Assignment</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">

				Do you want to mark this assignment as complete and put it into Billable Review?

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer />
				<v-btn color="blue darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="blue darken-1" text @click="Complete()">Complete</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import _ from 'lodash';
import { DateTime } from 'luxon';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import Dialogues from '@/Utility/Dialogues';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { IAssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';

interface CompleteAssignmentDialogueModelState {
	assignment: IAssignment | null;
}

@Component({
	components: {

	},
})
export default class CompleteAssignmentDialogue extends DialogueBase {

	public static GenerateEmpty(): CompleteAssignmentDialogueModelState {
		return {
			assignment: null,
		};
	}

	protected MobileDeviceWidth = MobileDeviceWidth;

	constructor() {
		super();
		this.ModelState = CompleteAssignmentDialogue.GenerateEmpty();
	}

	get DialogueName(): string {
		return 'CompleteAssignmentDialogue';
	}

	private Cancel() {
		Dialogues.Close(this.DialogueName);
		this.ModelState = CompleteAssignmentDialogue.GenerateEmpty();
	}

	private Complete() {

		//console.log('Complete', this.ModelState);


		const clone = _.cloneDeep(this.ModelState.assignment) as IAssignment;
		if (!clone.id) {
			console.error('!clone.id');
			return;
		}
		clone.lastModifiedISO8601 = DateTime.utc().toISO();
		clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

		// Find a status id with isBillableReview


		const all = this.$store.state.Database.assignmentStatus as Record<string, IAssignmentStatus>;

		const filtered = _.filter(
			all,
			(o: IAssignmentStatus) => {
				return o.json.isBillableReview && o.json.isDefault;
			});

		const sorted = _.sortBy(filtered, (o: IAssignmentStatus) => {
			return o.json.name;
		});

		if (0 === sorted.length) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Can\'t mark as billable review as no status is default and has billable review set.',
				autoClearInSeconds: 10,
			});
			return;
		}

		const status = sorted[0] as IAssignmentStatus;


		clone.json.statusId = status.id || null;

		const payload: Record<string, IAssignment> = {};
		payload[clone.id] = clone;
		Assignment.UpdateIds(payload);

		Dialogues.Close(this.DialogueName);
		this.ModelState = CompleteAssignmentDialogue.GenerateEmpty();
	}

}
</script>