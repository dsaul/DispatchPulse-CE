<template>
	<div>
		<LabeledCompanyEditRow 
			v-for="(entry, index) in value" 
			:key="entry.id" 
			v-model="value[index]" 
			@input="PostChanged()" 
			:hideLabel="hideLabel"
			:index="index"
			:reducePadding="reducePadding"
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
			<v-col cols="12" sm="10" :offset-sm="hideLabel ? 0 : 2">
				<v-btn
					text
					large
					@click="OnInsertNewRowAtIndex(value.length)"
					:disabled="disabled || readonly"
					>
					<v-icon left>add</v-icon>
					Add Company
				</v-btn>
			</v-col>
		</v-row>
	</div>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import LabeledCompanyEditRow from '@/Components/Rows/LabeledCompanyEditRow.vue';
import GenerateID from '@/Utility/GenerateID';
import ArrayAdapterBase from './ArrayAdapterBase';
import { ILabeledCompanyId } from '@/Data/Models/LabeledCompanyId/LabeledCompanyId';

@Component({
	components: {
		LabeledCompanyEditRow,
	},
	
})

export default class LabeledCompanyEditRowArrayAdapter extends ArrayAdapterBase {
	
	@Prop({ default: null }) declare public readonly value: ILabeledCompanyId[] | null;  // this.$emit('input', newVal);
	@Prop({ default: false }) public readonly hideLabel!: boolean;
	@Prop({ default: false }) public readonly reducePadding!: boolean;
	
	protected GenerateEmptyRow(): ILabeledCompanyId {
		return {
			id: GenerateID(),
			label: '',
			value: '',
		};
	}
	
}
</script>