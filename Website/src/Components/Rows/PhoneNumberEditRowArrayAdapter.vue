<template>
	<div>
		<PhoneNumberEditRow 
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
						class="e2e-add-phone-number"
						:disabled="disabled || readonly"
						>
						<v-icon left>add</v-icon>
						Add Phone Number
					</v-btn>
				</v-col>
			</v-row>
	</div>
</template>
<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import PhoneNumberEditRow from '@/Components/Rows/PhoneNumberEditRow.vue';
import ArrayAdapterBase from './ArrayAdapterBase';
import GenerateID from '@/Utility/GenerateID';
import { IPhoneNumber } from '@/Data/Models/PhoneNumber/PhoneNumber';

@Component({
	components: {
		PhoneNumberEditRow,
	},
	
})
export default class PhoneNumberEditRowArrayAdapter extends ArrayAdapterBase {
	
	@Prop({ default: null }) declare public value: IPhoneNumber[] | null;
	
	protected GenerateEmptyRow(): IPhoneNumber {
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