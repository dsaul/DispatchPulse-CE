<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Exception Entry</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="LabourException">{{LabourException.json.name}}</span>"?
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-labour-exception-confirm-button"
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
	import { DeleteLabourExceptionDialogueModelState, IDeleteLabourExceptionDialogueModelState } from '@/Data/Models/DeleteLabourExceptionDialogueModelState/DeleteLabourExceptionDialogueModelState';
	import { ILabourSubtypeException, LabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/LabourSubtypeException';
	import Dialogues from '@/Utility/Dialogues';
	
	@Component({
		components: {
			
		},
	})
	export default class DeleteLabourExceptionDialogue extends DialogueBase {
		
		public static GenerateEmpty(): IDeleteLabourExceptionDialogueModelState {
			return DeleteLabourExceptionDialogueModelState.GetEmpty();
		}
		
		constructor() {
			super();
			this.ModelState = DeleteLabourExceptionDialogueModelState.GetEmpty();
		}
		
		get DialogueName(): string {
			return 'DeleteLabourExceptionDialogue';
		}
		
		protected Cancel(): void {
			Dialogues.Close(this.DialogueName);
			this.ModelState = DeleteLabourExceptionDialogueModelState.GetEmpty();
		}
		
		protected Confirm(): void {
			
			const state = this.ModelState as IDeleteLabourExceptionDialogueModelState;
			const redirect = state.redirectToIndex;
			
			console.log('confirm', this.ModelState);
			
			if (state.id) {
				LabourSubtypeException.DeleteIds([state.id]);
			}
			
			Dialogues.Close(this.DialogueName);
			this.ModelState = DeleteLabourExceptionDialogueModelState.GetEmpty();
			
			if (redirect) {
				this.$router.push(`/section/labour-exception-definitions`).catch(((e: Error) => { }));// eslint-disable-line
			}
		}
		
		protected get LabourException(): ILabourSubtypeException | null {
			
			const state = this.ModelState as IDeleteLabourExceptionDialogueModelState;
			//const id = state.id;
			
			if (!state.id) {
				return null;
			}
			
			return LabourSubtypeException.ForId(state.id);
		}
		
		
	}
</script>
