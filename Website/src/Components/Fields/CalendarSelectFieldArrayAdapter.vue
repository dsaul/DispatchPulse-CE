<template>
	<div>
		<CalendarSelectField 
			v-for="(entry, index) in value" 
			:key="index" 
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
			>
			<template v-slot:custom-menu-options>
				<v-divider />
				<v-list-item
					@click="OnMoveUp(index)"
					:disabled="index === 0 || disabled || readonly"
					>
					<v-list-item-icon>
						<v-icon>arrow_upward</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Move Up</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					@click="OnMoveDown(index)"
					:disabled="index === value.length - 1 || disabled || readonly"
					>
					<v-list-item-icon>
						<v-icon>arrow_downward</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Move Down</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-divider />
				<v-list-item
					@click="OnInsertNewRowAtIndex(index)"
					:disabled="disabled || readonly"
					>
					<v-list-item-icon>
						<v-icon>add</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Add Row Above</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					@click="OnInsertNewRowAtIndex(index + 1)"
					:disabled="disabled || readonly"
					>
					<v-list-item-icon>
						<v-icon>add</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Add Row Below</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					@click="OnRemoveRowAtIndex(index)"
					:disabled="disabled || readonly"
					>
					<v-list-item-icon>
						<v-icon>remove</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Remove This Row</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</template>
		</CalendarSelectField>
		<v-row>
			<v-col cols="12" sm="10" offset-sm="2">
				<v-btn
					text
					large
					@click="OnInsertNewRowAtIndex(value.length)"
					:disabled="disabled || readonly"
					>
					<v-icon left>add</v-icon>
					Add Calendar
				</v-btn>
			</v-col>
		</v-row>
	</div>
</template>
<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import CalendarSelectField from '@/Components/Fields/CalendarSelectField.vue';
import ArrayAdapterBase from '@/Components/Rows/ArrayAdapterBase';
import { ILabelledValue } from '@/Data/Models/LabelledValue/LabelledValue';

@Component({
	components: {
		CalendarSelectField,
	},
	
})
export default class CalendarSelectFieldArrayAdapter extends ArrayAdapterBase {
	
	@Prop({ default: null }) declare public readonly value: Array<string | null>;  // this.$emit("input", newVal);
	
	protected GenerateEmptyRow(): ILabelledValue | null {
		return null;
	}
	
}

</script>
<style scoped>

</style>