<template>
	<div>
		<AgentSelectField 
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
				
			</template>
		</AgentSelectField>
		<v-row>
			<v-col cols="12" sm="10" offset-sm="2">
				<v-btn
					text
					large
					@click="OnInsertNewRowAtIndex(value.length)"
					:disabled="disabled || readonly"
					>
					<v-icon left>add</v-icon>
					Add Agent
				</v-btn>
			</v-col>
		</v-row>
	</div>
</template>
<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import AgentSelectField from '@/Components/Fields/AgentSelectField.vue';
import ArrayAdapterBase from '@/Components/Rows/ArrayAdapterBase';
import { ILabelledValue } from '@/Data/Models/LabelledValue/LabelledValue';

@Component({
	components: {
		AgentSelectField,
	},
	
})
export default class AgentSelectFieldArrayAdapter extends ArrayAdapterBase {
	
	@Prop({ default: null }) declare public readonly value: Array<string | null>;  // this.$emit("input", newVal);
	
	protected GenerateEmptyRow(): ILabelledValue | null {
		return null;
	}
	
}

</script>
<style scoped>

</style>