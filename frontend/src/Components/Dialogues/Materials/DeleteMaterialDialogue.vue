<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Material</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="MaterialDescription">{{MaterialDescription}}</span>"?
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-material-confirm-button"
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
import { DeleteAssignmentDialogueModelState, IDeleteAssignmentDialogueModelState } from '@/Data/Models/DeleteAssignmentDialogueModelState/DeleteAssignmentDialogueModelState';
import { IMaterial, Material } from '@/Data/CRM/Material/Material';
import { Product } from '@/Data/CRM/Product/Product';
import Dialogues from '@/Utility/Dialogues';
import { IDeleteMaterialDialogueModelState } from '@/Data/Models/DeleteMaterialDialogueModelState/DeleteMaterialDialogueModelState';

@Component({
	components: {
		
	},
})
export default class DeleteMaterialDialogue extends DialogueBase {
	
	
	public static GenerateEmpty(): IDeleteAssignmentDialogueModelState {
		return DeleteAssignmentDialogueModelState.GetEmpty();
	}
	
	constructor() {
		super();
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteMaterialDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		const state = this.ModelState as IDeleteMaterialDialogueModelState;
		const redirect = state.redirectToIndex;

		console.log('confirm', this.ModelState);
		
		if (state.id) {
			Material.DeleteIds([state.id]);
		}
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAssignmentDialogueModelState.GetEmpty();
		
		if (redirect) {
			this.$router.push(`/section/material-entries/`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}
	
	protected get Material(): IMaterial | null {
		
		const state = this.ModelState as IDeleteMaterialDialogueModelState;
		//const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return Material.ForId(state.id);
	}
	
	protected get MaterialDescription(): string | null {
		
		const material = this.Material;
		if (!material) {
			return null;
		}
		
		const productId = material.json.productId;
		if (!productId) {
			return null;
		}
		
		const product = Product.ForId(productId);
		if (!product) {
			return null;
		}
		
		return `${material.json.quantity} ${material.json.quantityUnit} ${product.json.name}`;
	}
	
}
</script>