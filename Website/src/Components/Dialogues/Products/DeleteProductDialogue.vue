<template>
	<v-dialog v-model="IsOpen" persistent  scrollable>
		<v-card>
			<v-card-title>Delete Product</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="Product">{{Product.json.name}}</span>"?
				
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn
					color="red darken-1"
					text
					@click="Confirm()"
					class="e2e-delete-product-confirm-button"
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
import { DeleteProductDialogueModelState, IDeleteProductDialogueModelState } from '@/Data/Models/DeleteProductDialogueModelState/DeleteProductDialogueModelState';//tslint:disable-line
import { IProduct, Product } from '@/Data/CRM/Product/Product';
import Dialogues from '@/Utility/Dialogues';
import { IDeleteContactDialogueModelState } from '@/Data/Models/DeleteContactDialogueModelState/DeleteContactDialogueModelState';

@Component({
	components: {
		
	},
})
export default class DeleteProductDialogue extends DialogueBase {
	
	
	public static GenerateEmpty(): IDeleteProductDialogueModelState {
		return DeleteProductDialogueModelState.GetEmpty();
	}
	
	constructor() {
		super();
		this.ModelState = DeleteProductDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteProductDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteProductDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		const redirect = state.redirectToIndex;
		
		console.log('confirm', this.ModelState);
		
		if (state.id) {
			Product.DeleteIds([state.id]);
		}
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteProductDialogueModelState.GetEmpty();
		
		if (redirect) {
			this.$router.push(`/section/product-definitions`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}
	
	protected get Product(): IProduct | null {
		
		const state = this.ModelState as IDeleteContactDialogueModelState;
		//const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		return Product.ForId(state.id);
	}
	
}
</script>