<template>
	<v-dialog v-model="IsOpen" persistent scrollable :fullscreen="MobileDeviceWidth()">
		<v-card>
			<v-card-title>Add Company</v-card-title>
			<v-divider></v-divider>
			<v-card-text>

				<CompanyEditor ref="editor" v-model="ModelState" :showAppBar="false" :showFooter="false"
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
import CompanyEditor from '@/Components/Editors/CompanyEditor.vue';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { Company, ICompany } from '@/Data/CRM/Company/Company';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';

@Component({
	components: {
		CompanyEditor,
	},
})
export default class ModifyCompanyDialogue extends DialogueBase {

	public $refs!: {
		editor: CompanyEditor,
	};

	protected MobileDeviceWidth = MobileDeviceWidth;

	constructor() {
		super();
		this.ModelState = Company.GetEmpty();
	}

	get DialogueName(): string {
		return 'ModifyCompanyDialogue';
	}

	protected Cancel(): void {
		//console.log('Cancel');


		this.$refs.editor.ResetValidation();
		Dialogues.Close(this.DialogueName);
		this.ModelState = Company.GetEmpty();
		this.$refs.editor.SelectFirstTab();
	}

	protected Save(): void {
		//console.log('Save', );

		if (this.$refs.editor.IsValidated()) {
			const state = this.ModelState as ICompany;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				this.ModelState = state;

				const payload: Record<string, ICompany> = {};
				payload[state.id] = state;
				Company.UpdateIds(payload);

				this.$router.push(`/section/companies/${state.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line

				Dialogues.Close(this.DialogueName);
				this.ModelState = Company.GetEmpty();
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