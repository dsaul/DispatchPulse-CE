import _ from "lodash";

export interface IDeleteProjectDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteProjectDialogueModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IDeleteProjectDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDeleteProjectDialogueModelState {
		const ret: IDeleteProjectDialogueModelState = {
			redirectToIndex: false,
			id: null
		};

		return ret;
	}
}

export const GenerateEmpty = (
	mergeValues?: Record<string, any> | null
): IDeleteProjectDialogueModelState => {
	const ret: IDeleteProjectDialogueModelState = {
		redirectToIndex: false,
		id: null
	};

	if (mergeValues) {
		_.merge(ret, mergeValues);
	}

	return ret;
};

export default {};
