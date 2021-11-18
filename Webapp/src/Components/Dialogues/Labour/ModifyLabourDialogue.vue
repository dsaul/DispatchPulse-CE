<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		class="e2e-modify-labour-dialogue"
		>
		<v-card>
			<v-card-title>Modify Labour</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<LabourEditor
					ref="editor"
					v-model="ModelState"
					:showAppBar="false"
					:showFooter="false "
					preselectTabName="General"
					:isMakingNew="true"
					:isDialogue="true"
					/>
				

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="red darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="green darken-1" text @click="Save()">Save</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import LabourEditor from '@/Components/Editors/LabourEditor.vue';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';

@Component({
	components: {
		LabourEditor,
	},
})
export default class ModifyLabourDialogue extends DialogueBase {
	
	public $refs!: {
		editor: LabourEditor,
	};
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	constructor() {
		super();
		
		const empty = Labour.GetEmpty();
		empty.json.isEnteredThroughTelephoneCompanyAccess = false;
		this.ModelState = empty;
	}
	
	
	
	get DialogueName(): string {
		return 'ModifyLabourDialogue';
	}
	
	protected Cancel(): void {
		//console.log('Cancel');
		
		
		this.$refs.editor.ResetValidation();
		Dialogues.Close(this.DialogueName);
		const empty = Labour.GetEmpty();
		empty.json.isEnteredThroughTelephoneCompanyAccess = false;
		this.ModelState = empty;
		this.$refs.editor.SelectFirstTab();
	}
	
	protected Save(): void {
		//console.log('Save', );
		
		if (!this.ModelState) {
			console.warn('!this.ModelState');
			return;
		}
		
		if (this.$refs.editor.IsValidated()) {
			const state = this.ModelState as ILabour;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				this.ModelState = state;
				
				const payload: Record<string, ILabour> = {};
				payload[state.id] = state;
				
				console.log('payload', payload);
				
				Labour.UpdateIds(payload);
				
				//this.$router.push(`/section/agents/${state.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
				
				Dialogues.Close(this.DialogueName);
				const empty = Labour.GetEmpty();
				empty.json.isEnteredThroughTelephoneCompanyAccess = false;
				this.ModelState = empty;
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