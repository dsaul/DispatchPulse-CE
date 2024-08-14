<template>
	<v-row>
		<v-col 
			v-if="!hideLabel"
			cols="4" 
			:sm="reducePadding ? 3 : 2" 
			:offset-sm="reducePadding ? 0 : 2">
			<v-combobox
				v-model="Label"
				label="Label"
				auto-select-first
				hint="Relation to this project."
				:items="['Unspecified', 'Homeowner', 'Designer', 'Architect', 'Manager']"
				class="e2e-labeled-contact-edit-row-label"
				:disabled="disabled"
				:readonly="readonly"
				>
			</v-combobox>
		</v-col>
		<v-col :cols="hideLabel ? 12 : 8" :sm="hideLabel ? 12 : (reducePadding ? 9 : 6)">
			<ContactSelectField
				v-model="Value"
				:isDialogue="isDialogue"
				class="e2e-labeled-contact-edit-row-value"
				:disabled="disabled"
				:readonly="readonly"
				>
				<template v-slot:custom-menu-options>
					<v-divider />
					<v-list-item
						@click="$emit('MoveUp', index)"
						:disabled="isFirstIndex || disabled || readonly"
						>
						<v-list-item-icon>
							<v-icon>arrow_upward</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Move Up</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="$emit('MoveDown', index)"
						:disabled="isLastIndex || disabled || readonly"
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
						@click="$emit('InsertNewRowAtIndex', index)"
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
						@click="$emit('InsertNewRowAtIndex', index + 1)"
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
						@click="$emit('RemoveRowAtIndex', index)"
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
			</ContactSelectField>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import ContactSelectField from '@/Components/Fields/ContactSelectField.vue';
import RowBase from './RowBase';
import { ILabeledContactId } from '@/Data/Models/LabeledContactId/LabeledContactId';

@Component({
	components: {
		ContactSelectField,
	},
	
})

export default class LabeledContactEditRow extends RowBase {
	
	@Prop({ default: null }) declare public readonly value: ILabeledContactId | null;
	@Prop({ default: false }) public readonly reducePadding!: boolean;
	@Prop({ default: false }) public readonly hideLabel!: boolean;
	
}
</script>