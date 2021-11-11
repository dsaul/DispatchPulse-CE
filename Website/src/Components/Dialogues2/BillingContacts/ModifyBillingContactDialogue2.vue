<template>
	<v-dialog
		v-model="isOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>{{dialogueName}}</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<BillingContactEditor
					ref="editor"
					v-model="EditorValue"
					:showAppBar="false"
					:showFooter="false "
					preselectTabName="General"
					:isMakingNew="isMakingNew"
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
import { Component, Prop } from 'vue-property-decorator';
import BillingContactEditor from '@/Components/Editors/BillingContactEditor.vue';
import DialogueBase2 from '@/Components/Dialogues2/DialogueBase2';
import { DateTime } from 'luxon';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts, IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import _ from 'lodash';

@Component({
	components: {
		BillingContactEditor,
	},
})
export default class ModifyBillingContactDialogue2 extends DialogueBase2 {
	
	@Prop({ default: null }) declare public readonly value: IBillingContacts | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		editor: BillingContactEditor,
	};
	
	
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	public SwitchToTabFromRoute(): void {
		if (this.$refs.editor) {
			this.$refs.editor.SwitchToTabFromRoute();
		}
		
	}
	
	protected get EditorValue(): IBillingContacts | null {
		return this.value;
	}
	
	protected set EditorValue(val: IBillingContacts | null) {
		
		const clone = _.cloneDeep(val);
		if (null != clone) {
			
			clone.json.lastModifiedISO8601 = DateTime.utc().toISO();
			clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
			
			this.$emit('input', clone);
		}
		
		
	}
	
	
	
	protected Cancel(): void {
		//console.log('Cancel');
		
		
		this.$refs.editor.ResetValidation();
		
		this.$emit('cancel', null);
		
		this.$refs.editor.SelectFirstTab();
		this.$refs.editor.ResetPasswordToDefault();
	}
	
	protected Save(): void {
		console.log('Save', this.$refs);
		
		if (this.$refs.editor.IsValidated()) {
			
			this.$emit('save', this.value);
			
			this.$refs.editor.ResetValidation();
			this.$refs.editor.SelectFirstTab();
			this.$refs.editor.ResetPasswordToDefault();
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