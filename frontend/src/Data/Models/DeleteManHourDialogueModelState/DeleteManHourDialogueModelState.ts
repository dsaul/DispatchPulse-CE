import _ from "lodash";

export interface IDeleteManHourDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteManHourDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteManHourDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteManHourDialogueModelState {
		const ret: IDeleteManHourDialogueModelState = {
			redirectToIndex: false,
			id: null
		};

		return ret;
	}
}

export default {};
