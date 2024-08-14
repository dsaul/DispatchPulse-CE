import _ from "lodash";

export interface IDeleteLabourNonBillableDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteLabourNonBillableDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteLabourNonBillableDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteLabourNonBillableDialogueModelState {
		const ret: IDeleteLabourNonBillableDialogueModelState = {
			redirectToIndex: false,
			id: null
		};

		return ret;
	}
}

export default {};
