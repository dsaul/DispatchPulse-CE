<template>
	<v-dialog v-model="isOpen" persistent scrollable :fullscreen="MobileDeviceWidth()">
		<v-card>
			<v-card-title>Add Assignment</v-card-title>
			<v-divider></v-divider>
			<v-card-text>

				<AssignmentEditor ref="editor" v-model="EditorValue" :showAppBar="false" :showFooter="false"
					preselectTabName="Admin" :isMakingNew="true" :isDialogue="true" />


			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer />
				<v-btn color="red darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="green darken-1" text @click="Save()">Save</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import AssignmentEditor from '@/Components/Editors/AssignmentEditor.vue';
import DialogueBase2 from '@/Components/Dialogues2/DialogueBase2';
import { DateTime } from 'luxon';
import _ from 'lodash';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { IAssignment } from '@/Data/CRM/Assignment/Assignment';

@Component({
	components: {
		AssignmentEditor,
	},
})
export default class AddAssignmentDialogue2 extends DialogueBase2 {

	@Prop({ default: null }) declare public readonly value: IAssignment | null;

	public $refs!: {
		editor: AssignmentEditor,
	};



	protected MobileDeviceWidth = MobileDeviceWidth;

	public SwitchToTabFromRoute(): void {
		if (this.$refs.editor) {
			this.$refs.editor.SwitchToTabFromRoute();
		}

	}

	protected get EditorValue(): IAssignment | null {
		return this.value;
	}

	protected set EditorValue(val: IAssignment | null) {

		const clone = _.cloneDeep(val);
		if (null != clone) {

			clone.lastModifiedISO8601 = DateTime.utc().toISO();
			clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

			this.$emit('input', clone);
		}


	}



	protected Cancel(): void {
		//console.log('Cancel');


		this.$refs.editor.ResetValidation();

		this.$emit('Cancel', null);

		this.$refs.editor.SelectFirstTab();
	}

	protected Save(): void {
		//console.log('Save', );

		if (this.$refs.editor.IsValidated()) {

			this.$emit('Save', this.value);

			this.$refs.editor.ResetValidation();
			this.$refs.editor.SelectFirstTab();
		} else {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Some of the form fields didn\'t pass validation.',
				autoClearInSeconds: 10,
			});
		}

	}


}
</script>