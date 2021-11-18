<template>
	<v-card-text
		class="value-checkbox"
		>
		<v-checkbox
			:label="value.text"
			v-model="value.checkboxState"
			@click.stop="ToggleCheckbox"
			dense
			></v-checkbox>
		
	</v-card-text>
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import _ from 'lodash';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { IProjectNoteCheckbox } from '@/Data/CRM/ProjectNoteCheckbox/ProjectNoteCheckbox';

@Component({
	components: {
	},
})
export default class ContentCheckbox extends Vue {
	
	@Prop({ default: null }) public readonly value!: IProjectNoteCheckbox;
	@Prop({ default: null }) public readonly note!: IProjectNote;
	
	protected ToggleCheckbox(): void {
		
		//console.debug('ToggleCheckbox');
		
		const clone = _.cloneDeep(this.note) as IProjectNote;
		if (!clone ||
			!clone.id) {
			return;
		}
		
		clone.lastModifiedISO8601 = DateTime.utc().toISO();
		clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		
		// For some reason you can't assign on a line where there is a cast and have it work.
		let checkboxState = !!(clone.json.content as IProjectNoteCheckbox).checkboxState;
		checkboxState = !checkboxState;
		const clone2: any = clone;
		clone2.checkboxState = checkboxState;
		
		const payload: Record<string, IProjectNote> = {};
		payload[clone.id] = clone;
		
		console.debug('payload', payload);
		
		ProjectNote.UpdateIds(payload);
	}
	
}
</script>
<style scoped>
.value-checkbox {
	padding-top: 0px;
	padding-bottom: 0px;
	margin-top: 16px;
}
</style>