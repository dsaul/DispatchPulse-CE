<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>Global Search</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<v-stepper v-model="currentStep" vertical style="-webkit-box-shadow: inherit; box-shadow: inherit;">
					<v-stepper-step :complete="currentStep > 1" editable step="1">
						Select an app
						<small>Summarize if needed</small>
					</v-stepper-step>

					<v-stepper-content step="1">
						<v-card color="grey lighten-1" class="mb-12" height="200px"></v-card>
						<v-btn color="primary" @click="currentStep = 2">Continue</v-btn>
						<v-btn text>Cancel</v-btn>
					</v-stepper-content>

					<v-stepper-step :complete="currentStep > 2" editable step="2">Configure analytics for this app</v-stepper-step>

					<v-stepper-content step="2">
						<v-card color="grey lighten-1" class="mb-12" height="200px"></v-card>
						<v-btn color="primary" @click="currentStep = 3">Continue</v-btn>
						<v-btn text>Cancel</v-btn>
					</v-stepper-content>

					<v-stepper-step :complete="currentStep > 3" editable step="3">Select an ad format and name ad unit</v-stepper-step>

					<v-stepper-content step="3">
						<v-card color="grey lighten-1" class="mb-12" height="200px"></v-card>
						<v-btn color="primary" @click="currentStep = 4">Continue</v-btn>
						<v-btn text>Cancel</v-btn>
					</v-stepper-content>

					<v-stepper-step step="4" editable>View setup instructions</v-stepper-step>
					<v-stepper-content step="4">
						<v-card color="grey lighten-1" class="mb-12" height="200px"></v-card>
						<v-btn color="primary" @click="currentStep = 1">Continue</v-btn>
						<v-btn text>Cancel</v-btn>
					</v-stepper-content>
				</v-stepper>
				

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="blue darken-1" text @click="ResetAndClose()">Close</v-btn>
				<v-btn color="blue darken-1" text @click="Next()">Next</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import AssistantDialogueBase from '@/Components/Dialogues/AssistantDialogueBase';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';

@Component({
	components: {
		
	},
})
export default class GlobalSearchDialogue extends AssistantDialogueBase {
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	get MaxSteps(): number {
		return 4;
	}
	
	get DialogueName(): string {
		return 'GlobalSearchDialogue';
	}
	
	
}
</script>