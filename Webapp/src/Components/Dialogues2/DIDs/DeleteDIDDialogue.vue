<template>
	<v-dialog
		v-model="isOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>Delete Phone Number</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				Are you sure you want to delete "<span v-if="value">{{value.json.DIDNumber}}</span>"?
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="red darken-1" text @click="Delete()">Delete</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import DialogueBase2 from '@/Components/Dialogues2/DialogueBase2';
import { DateTime } from 'luxon';
import _ from 'lodash';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IDID } from '@/Data/CRM/DID/DID';

@Component({
	components: {
	},
})
export default class DeleteDIDDialogue extends DialogueBase2 {
	
	@Prop({ default: null }) declare public readonly value: IDID | null;
	
	public $refs!: {
	};
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	public SwitchToTabFromRoute(): void {
		//
	}
	
	protected get EditorValue(): IDID | null {
		return this.value;
	}
	
	protected set EditorValue(val: IDID | null) {
		
		const clone = _.cloneDeep(val);
		if (null != clone) {
			
			clone.lastModifiedISO8601 = DateTime.utc().toISO();
			clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
			
			this.$emit('input', clone);
		}
		
		
	}
	
	
	
	protected Cancel(): void {
		
		this.$emit('Cancel', null);
		
	}
	
	protected Delete(): void {
		this.$emit('Delete', this.value);
	}
	
	
}
</script>