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
			<v-card-text style="padding: 20px;">
				
				Are you sure you want to delete {{!value ? '' : value.fullName}}?
				

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="red darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="green darken-1" text @click="Save()">Delete</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import BillingContactEditor from '@/Components/Editors/BillingContactEditor.vue';
import DialogueBase2 from '@/Components/Dialogues2/DialogueBase2';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';

@Component({
	components: {
		BillingContactEditor,
	},
})
export default class DeleteBillingContactDialogue2 extends DialogueBase2 {
	
	@Prop({ default: null }) declare public readonly value: IBillingContacts | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	@Prop({ default: 'Delete Account' }) declare public readonly dialogueName: string | null;
	
	public $refs!: {
		
	};
	
	
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected Cancel(): void {
		//console.log('Cancel');
		
		this.$emit('cancel', null);
		
	}
	
	protected Save(): void {
		console.log('Save', this.$refs);
		
		this.$emit('save', null);
	}
	
	
}
</script>