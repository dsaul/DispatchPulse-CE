import _ from "lodash";

export interface IDeleteProductDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteProductDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteProductDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteProductDialogueModelState {
		const ret: IDeleteProductDialogueModelState = {
			redirectToIndex: false,
			id: null
		};

		return ret;
	}
}

export default {};
