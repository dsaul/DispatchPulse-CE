<template>
	<div>
		<AddressEditRow 
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
						class="e2e-add-address"
						:disabled="disabled || readonly"
						>
						<v-icon left>add</v-icon>
						Add Address
					</v-btn>
				</v-col>
			</v-row>
	</div>
</template>
<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import AddressEditRow from '@/Components/Rows/AddressEditRow.vue';
import GenerateID from '@/Utility/GenerateID';
import ArrayAdapterBase from './ArrayAdapterBase';
import { IAddress } from '@/Data/Models/Address/Address';

@Component({
	components: {
		AddressEditRow,
	},
	
})
export default class AddressEditRowArrayAdapter extends ArrayAdapterBase {
	
	@Prop({ default: null }) declare public value: IAddress[] | null;  // this.$emit("input", newVal);
	
	protected GenerateEmptyRow(): IAddress {
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