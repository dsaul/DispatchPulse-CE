import _ from "lodash";

export interface IDeleteAssignmentDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteAssignmentDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteAssignmentDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteAssignmentDialogueModelState {
		const ret: IDeleteAssignmentDialogueModelState = {
			redirectToIndex: false,
			id: null
		};

		return ret;
	}
}

export default {};
