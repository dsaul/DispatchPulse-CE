<template>
	<v-dialog v-model="isOpen" persistent scrollable :fullscreen="MobileDeviceWidth()">
		<v-card>
			<v-card-title>{{ dialogueName }}</v-card-title>
			<v-divider></v-divider>
			<v-card-text>

				<BillingContactPermissionsEditor ref="editor" v-model="EditorValue" :showAppBar="false"
					:showFooter="false" preselectTabName="General" :isMakingNew="isMakingNew" :isDialogue="true" />


			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer />
				<v-btn color="darken-1" text @click="Close()">Close</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import BillingContactPermissionsEditor from '@/Components/Editors/BillingContactPermissionsEditor.vue';
import DialogueBase2 from '@/Components/Dialogues2/DialogueBase2';
import { DateTime } from 'luxon';
import _ from 'lodash';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts, IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';

@Component({
	components: {
		BillingContactPermissionsEditor,
	},
})
export default class ModifyBillingContactPermissionsDialogue2 extends DialogueBase2 {

	@Prop({ default: null }) declare public readonly value: IBillingContacts | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;

	public $refs!: {
		editor: BillingContactPermissionsEditor,
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

			this.$emit('save', this.value);
		}


	}



	protected Close(): void {
		//console.log('Cancel');


		this.$refs.editor.ResetValidation();

		this.$emit('close', null);

		this.$refs.editor.SelectFirstTab();
	}



}
</script>