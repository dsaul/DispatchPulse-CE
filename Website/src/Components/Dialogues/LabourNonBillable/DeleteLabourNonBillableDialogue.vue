<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Non Billable Entry</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				AAre you sure you want to delete "<span v-if="LabourNonBillable">{{LabourNonBillable.json.name}}</span>"?
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-labour-non-billable-confirm-button"
					>
					Confirm
				</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
	import { Component } from 'vue-property-decorator';
	import DialogueBase from '@/Components/Dialogues/DialogueBase';
	import { DeleteLabourNonBillableDialogueModelState, IDeleteLabourNonBillableDialogueModelState } from '@/Data/Models/DeleteLabourNonBillableDialogueModelState/DeleteLabourNonBillableDialogueModelState';//tslint:disable-line
	import { ILabourSubtypeNonBillable, LabourSubtypeNonBillable } from '@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';
	import Dialogues from '@/Utility/Dialogues';
	
	@Component({
		components: {
			
		},
	})
	export default class DeleteLabourNonBillableDialogue extends DialogueBase {
		
		public static GenerateEmpty(): IDeleteLabourNonBillableDialogueModelState {
			return DeleteLabourNonBillableDialogueModelState.GetEmpty();
		}
		
		constructor() {
			super();
			this.ModelState = DeleteLabourNonBillableDialogueModelState.GetEmpty();
		}
		
		get DialogueName(): string {
			return 'DeleteLabourNonBillableDialogue';
		}
		
		protected Cancel(): void {
			Dialogues.Close(this.DialogueName);
			this.ModelState = DeleteLabourNonBillableDialogueModelState.GetEmpty();
		}
		
		protected Confirm(): void {
			
			const state = this.ModelState as IDeleteLabourNonBillableDialogueModelState;
			const redirect = state.redirectToIndex;
			
			console.log('confirm', this.ModelState);
			
			if (state.id) {
				LabourSubtypeNonBillable.DeleteIds([state.id]);
			}
			
			
			Dialogues.Close(this.DialogueName);
			this.ModelState = DeleteLabourNonBillableDialogueModelState.GetEmpty();
			
			if (redirect) {
				this.$router.push(`/section/labour-non-billable-definitions/`).catch(((e: Error) => { }));// eslint-disable-line
			}
		}
		
		protected get LabourNonBillable(): ILabourSubtypeNonBillable | null {
			
			const state = this.ModelState as IDeleteLabourNonBillableDialogueModelState;
			//const id = state.id;
			
			if (!state.id) {
				return null;
			}
			
			return LabourSubtypeNonBillable.ForId(state.id);
		}
	}
</script>
