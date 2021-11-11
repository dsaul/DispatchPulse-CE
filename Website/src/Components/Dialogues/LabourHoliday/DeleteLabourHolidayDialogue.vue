<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Labour Holiday Status</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="LabourHoliday">{{LabourHoliday.json.name}}</span>"?
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-labour-holiday-confirm-button"
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
	import { DeleteLabourHolidayDialogueModelState, IDeleteLabourHolidayDialogueModelState } from '@/Data/Models/DeleteLabourHolidayDialogueModelState/DeleteLabourHolidayDialogueModelState';
	import { ILabourSubtypeHoliday, LabourSubtypeHoliday } from '@/Data/CRM/LabourSubtypeHoliday/LabourSubtypeHoliday';
	import Dialogues from '@/Utility/Dialogues';
	
	@Component({
		components: {
			
		},
	})
	export default class DeleteLabourHolidayDialogue extends DialogueBase {
		
		public static GenerateEmpty(): IDeleteLabourHolidayDialogueModelState {
			return DeleteLabourHolidayDialogueModelState.GetEmpty();
		}
		
		constructor() {
			super();
			this.ModelState = DeleteLabourHolidayDialogueModelState.GetEmpty();
		}
		
		get DialogueName(): string {
			return 'DeleteLabourHolidayDialogue';
		}
		
		protected Cancel(): void {
			Dialogues.Close(this.DialogueName);
			this.ModelState = DeleteLabourHolidayDialogueModelState.GetEmpty();
		}
		
		protected Confirm(): void {
			
			const state = this.ModelState as IDeleteLabourHolidayDialogueModelState;
			const redirect = state.redirectToIndex;
			
			console.log('confirm', this.ModelState);
			
			if (state.id) {
				LabourSubtypeHoliday.DeleteIds([state.id]);
			}
			
			Dialogues.Close(this.DialogueName);
			this.ModelState = DeleteLabourHolidayDialogueModelState.GetEmpty();
			
			if (redirect) {
				this.$router.push(`/section/labour-holiday-definitions`).catch(((e: Error) => { }));// eslint-disable-line
			}
			
			
		}
		
		protected get LabourHoliday(): ILabourSubtypeHoliday | null {
			
			const state = this.ModelState as IDeleteLabourHolidayDialogueModelState;
			//const id = state.id;
			
			if (!state.id) {
				return null;
			}
			
			return LabourSubtypeHoliday.ForId(state.id);
		}
		
	}
</script>
