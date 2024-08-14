import _ from "lodash";

export interface IDeleteLabourHolidayDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteLabourHolidayDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteLabourHolidayDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteLabourHolidayDialogueModelState {
		const ret: IDeleteLabourHolidayDialogueModelState = {
			redirectToIndex: false,
			id: null
		};

		return ret;
	}
}

export default {};
