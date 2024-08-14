<template>
	<v-dialog v-model="IsOpen" persistent scrollable :fullscreen="MobileDeviceWidth()">
		<v-card>
			<v-card-title>Modify Material</v-card-title>
			<v-divider></v-divider>
			<v-card-text>

				<MaterialEditor ref="editor" v-model="ModelState" :showAppBar="false" :showFooter="false"
					preselectTabName="General" :isMakingNew="true" :isDialogue="true" />


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
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import MaterialEditor from '@/Components/Editors/MaterialEditor.vue';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IMaterial, Material } from '@/Data/CRM/Material/Material';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';

@Component({
	components: {
		MaterialEditor,
	},
})
export default class ModifyMaterialDialogue extends DialogueBase {

	public $refs!: {
		editor: MaterialEditor,
	};

	protected MobileDeviceWidth = MobileDeviceWidth;

	constructor() {
		super();
		this.ModelState = Material.GetEmpty();
	}



	get DialogueName(): string {
		return 'ModifyMaterialDialogue';
	}

	protected Cancel(): void {
		//console.log('Cancel');


		this.$refs.editor.ResetValidation();
		Dialogues.Close(this.DialogueName);
		this.ModelState = Material.GetEmpty();
		this.$refs.editor.SelectFirstTab();
	}

	protected Save(): void {
		//console.log('Save', );

		if (this.$refs.editor.IsValidated()) {
			const state = this.ModelState as IMaterial;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				this.ModelState = state;

				const payload: Record<string, IMaterial> = {};
				payload[state.id] = state;
				Material.UpdateIds(payload);

				//this.$router.push(`/section/companies/${state.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line

				Dialogues.Close(this.DialogueName);
				this.ModelState = Material.GetEmpty();
				this.$refs.editor.ResetValidation();
				this.$refs.editor.SelectFirstTab();
			}

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