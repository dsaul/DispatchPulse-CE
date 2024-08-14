import _ from "lodash";

export interface IDeleteContactDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;

	deleteProjectMaterialsAsWell: boolean;
	deleteProjectNotesAsWell: boolean;
	deleteProjectLabourAsWell: boolean;
	deleteProjectAssignmentsAsWell: boolean;
}

export class DeleteContactDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteContactDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteContactDialogueModelState {
		const ret: IDeleteContactDialogueModelState = {
			redirectToIndex: false,
			id: null,
			deleteProjectMaterialsAsWell: false,
			deleteProjectNotesAsWell: false,
			deleteProjectLabourAsWell: false,
			deleteProjectAssignmentsAsWell: false
		};

		return ret;
	}
}

export default {};
