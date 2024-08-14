import _ from "lodash";

export interface IDeleteAssignmentStatusDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteAssignmentStatusDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteAssignmentStatusDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteAssignmentStatusDialogueModelState {
		const ret: IDeleteAssignmentStatusDialogueModelState = {
			redirectToIndex: false,
			id: null
		};

		return ret;
	}
}

export default {};
