

import Vue from 'vue';
import store from '@/plugins/store/store';
import _ from 'lodash';

export class Dialogues {
	// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
	public static Open(params: any): void {
		
		//console.log('Dialogues.Open', params);
		
		if (Object.keys(store.state.openDialogues).indexOf(params.name) === -1) {
			
			// apply params opts to state.
			let existingState = store.state.dialogueModelStates[params.name];
			if (!existingState) {
				existingState = {};
			}
			_.merge(existingState, params.state);
			
			Vue.set(store.state.dialogueModelStates, params.name, existingState);
			
			Vue.set(store.state.openDialogues, params.name, params);
			
		}
		
	}
	
	public static Close(name: string): void {
	
		Vue.delete(store.state.openDialogues, name);
		
	}
	
	public static GetOpen(): void {
		return store.state.openDialogues;
	}
	
	public static IsDialogueOpen(name: string): boolean {
		
		return Object.keys(store.state.openDialogues).indexOf(name) !== -1;
		
	}
	
	public static GetDialogueModelState(name: string): any {
		
		if (
			!store.state ||
			!store.state.dialogueModelStates
		) {
			return null;
		}
		
		const r = store.state.dialogueModelStates[name] || null;
		//console.debug('get DialogueBase.ModelState', r);
		return r;
		
	}
	
	public static SetDialogueModelState(payload: { name: string; state: any; }): void {
		store.commit('SetModelState', payload);
	}
	
}














export default Dialogues;
