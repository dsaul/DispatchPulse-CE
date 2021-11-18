import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import Dialogues from '@/Utility/Dialogues';

@Component({
	
})
export default class AssistantDialogueBase extends DialogueBase {
	
	
	protected currentStep = 1;
	
	
	protected get MaxSteps(): number {
		throw Error('not implemented');
	}
	
	protected Next(): void {
		
		if (+this.currentStep === +this.MaxSteps) {
			this.Finalize();
		} else {
			this.currentStep++;
		}
		
	}
	
	protected Finalize(): void {
		
		// Do stuff
		
		
		this.ResetAndClose();
		
	}
	
	protected ResetAndClose(): void {
		Dialogues.Close(this.DialogueName);
		this.currentStep = 1;
	}
}
