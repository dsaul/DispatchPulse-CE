<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>Add Project Note</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<ProjectNoteEditor
					ref="editor"
					v-model="ModelState"
					:showAppBar="false"
					:showFooter="false"
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
import ProjectNoteEditor from '@/Components/Editors/ProjectNoteEditor.vue';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';

@Component({
	components: {
		ProjectNoteEditor,
	},
})
export default class AddProjectNoteDialogue extends DialogueBase {
	
	public $refs!: {
		editor: ProjectNoteEditor,
	};
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	constructor() {
		super();
		this.ModelState = ProjectNote.GetEmpty();
	}
	
	
	
	get DialogueName(): string {
		return 'AddProjectNoteDialogue';
	}
	
	protected Cancel(): void {
		//console.log('Cancel');
		
		
		this.$refs.editor.ResetValidation();
		Dialogues.Close(this.DialogueName);
		this.ModelState = ProjectNote.GetEmpty();
		this.$refs.editor.SelectFirstTab();
	}
	
	protected Save(): void {
		//console.log('Save', );
		
		if (this.$refs.editor.IsValidated()) {
			const state = this.ModelState as IProjectNote;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				state.json.originalBillingId = BillingContacts.CurrentBillingContactId();
				state.json.originalISO8601 = state.lastModifiedISO8601;
				this.ModelState = state;
				
				const payload: Record<string, IProjectNote> = {};
				payload[state.id] = state;
				ProjectNote.UpdateIds(payload);
				
				//this.$router.push(`/section/companies/${state.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
				
				Dialogues.Close(this.DialogueName);
				this.ModelState = ProjectNote.GetEmpty();
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