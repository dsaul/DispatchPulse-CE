import { Component, Vue } from "vue-property-decorator";
import Dialogues from "@/Utility/Dialogues";

@Component({
	components: {}
})
export default class DialogueBase extends Vue {
	get DialogueName(): string {
		throw Error("not implemented");
	}

	// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
	protected get ModelState(): any {
		return Dialogues.GetDialogueModelState(this.DialogueName);
	}

	// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
	protected set ModelState(val: any) {
		//console.debug('set DialogueBase.ModelState', val);

		Dialogues.SetDialogueModelState({
			name: this.DialogueName,
			state: val
		});
	}

	public get IsOpen(): boolean {
		return Dialogues.IsDialogueOpen(this.DialogueName);
	}
}
