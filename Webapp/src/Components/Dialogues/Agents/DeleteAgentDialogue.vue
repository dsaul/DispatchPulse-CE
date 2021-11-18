<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		>
		<v-card>
			<v-card-title>Delete Agent</v-card-title>
			<v-divider></v-divider>
			<v-card-text style="color: black; padding-top: 20px;">
				
				Are you sure you want to delete "<span v-if="Agent">{{Agent.json.name}}</span>"?
				

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn color="green darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="red darken-1" text @click="Confirm()">Confirm</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import { DeleteAgentDialogueModelState, IDeleteAgentDialogueModelState } from '@/Data/Models/DeleteAgentDialogueModelState/DeleteAgentDialogueModelState';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {
		
	},
})
export default class DeleteAgentDialogue extends DialogueBase {
	
	public static GenerateEmpty(): IDeleteAgentDialogueModelState {
		return DeleteAgentDialogueModelState.GetEmpty();
	}
	
	
	
	constructor() {
		super();
		this.ModelState = DeleteAgentDialogueModelState.GetEmpty();
	}
	
	get DialogueName(): string {
		return 'DeleteAgentDialogue';
	}
	
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAgentDialogueModelState.GetEmpty();
	}
	
	protected Confirm(): void {
		
		console.log('confirm', this.ModelState);
		
		const state = this.ModelState as IDeleteAgentDialogueModelState;
		const redirect = state.redirectToIndex;
		
		if (state.id) {
			Agent.DeleteIds([state.id]);
		}
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = DeleteAgentDialogueModelState.GetEmpty();
		
		if (redirect) {
			this.$router.push(`/section/agents/`).catch(((e: Error) => { }));// eslint-disable-line
		}
	}
	
	protected get Agent(): IAgent | null {
		
		const state = this.ModelState as IDeleteAgentDialogueModelState;
		const id = state.id;
		
		if (!state.id) {
			return null;
		}
		
		const contact = Agent.ForId(id);
		return contact;
	}
	
	
	
	
	
	
	
}
</script>