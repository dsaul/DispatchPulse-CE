<template>
	<div>
		<EMailEditRow 
			v-for="(entry, index) in value" 
			:key="entry.id" 
			v-model="value[index]" 
			@input="PostChanged()" 
			:index="index"
			:isFirstIndex="index === 0"
			:isLastIndex="index === value.length - 1"
			@InsertNewRowAtIndex="OnInsertNewRowAtIndex"
			@RemoveRowAtIndex="OnRemoveRowAtIndex"
			@MoveUp="OnMoveUp"
			@MoveDown="OnMoveDown"
			:isDialogue="isDialogue"
			:disabled="disabled"
			:readonly="readonly"
			/>
			<v-row>
				<v-col cols="12" sm="10" offset-sm="2">
					<v-btn
						text
						large
						@click="OnInsertNewRowAtIndex(value.length)"
						class="e2e-add-email"
						:disabled="disabled || readonly"
						>
						<v-icon left>add</v-icon>
						Add E-Mail
					</v-btn>
				</v-col>
			</v-row>
	</div>
</template>
<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import EMailEditRow from '@/Components/Rows/EMailEditRow.vue';
import GenerateID from '@/Utility/GenerateID';
import ArrayAdapterBase from './ArrayAdapterBase';
import { IEMail } from '@/Data/Models/EMail/EMail';

@Component({
	components: {
		EMailEditRow,
	},
	
})
export default class EMailEditRowArrayAdapter extends ArrayAdapterBase {
	
	@Prop({ default: null }) declare public value: IEMail[] | null;  // this.$emit("input", newVal);
	
	protected GenerateEmptyRow(): IEMail {
		return {
			id: GenerateID(),
			label: '',
			value: '',
		};
	}
	
}

</script>
<style scoped>

</style>