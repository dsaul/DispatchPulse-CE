<template>
	<div>
		<LabeledContactEditRow v-for="(entry, index) in value" :key="entry.id" v-model="value[index]"
			@input="PostChanged()" :index="index" :isFirstIndex="index === 0" :isLastIndex="index === value.length - 1"
			:reducePadding="reducePadding" :hideLabel="hideLabel" @InsertNewRowAtIndex="OnInsertNewRowAtIndex"
			@RemoveRowAtIndex="OnRemoveRowAtIndex" @MoveUp="OnMoveUp" @MoveDown="OnMoveDown" :isDialogue="isDialogue"
			:disabled="disabled" :readonly="readonly" />
		<v-row>
			<v-col cols="12" sm="10" :offset-sm="hideLabel ? 0 : 2">
				<v-btn text large @click="OnInsertNewRowAtIndex(value.length)" :disabled="disabled || readonly">
					<v-icon left>add</v-icon>
					Add Contact
				</v-btn>
			</v-col>
		</v-row>
	</div>
</template>
<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import LabeledContactEditRow from '@/Components/Rows/LabeledContactEditRow.vue';
import GenerateID from '@/Utility/GenerateID';
import ArrayAdapterBase from './ArrayAdapterBase';
import { ILabeledContactId } from '@/Data/Models/LabeledContactId/LabeledContactId';

@Component({
	components: {
		LabeledContactEditRow,
	},

})
export default class LabeledContactEditRowArrayAdapter extends ArrayAdapterBase {

	public static GenerateEmptyRow(): ILabeledContactId {
		return {
			id: GenerateID(),
			label: '',
			value: '',
		};
	}

	@Prop({ default: null }) declare public value: ILabeledContactId[] | null;
	@Prop({ default: false }) public readonly reducePadding!: boolean;
	@Prop({ default: false }) public readonly hideLabel!: boolean;



	protected GenerateEmptyRow(): ILabeledContactId {
		return LabeledContactEditRowArrayAdapter.GenerateEmptyRow();
	}

}

</script>
<style scoped></style>