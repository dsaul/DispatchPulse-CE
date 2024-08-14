import _ from "lodash";

export interface IDeleteCompanyDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteCompanyDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteCompanyDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteCompanyDialogueModelState {
		const ret: IDeleteCompanyDialogueModelState = {
			redirectToIndex: false,
			id: null
		};

		return ret;
	}
}

export default {};
