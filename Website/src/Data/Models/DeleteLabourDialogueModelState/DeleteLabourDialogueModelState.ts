import _ from 'lodash';

export interface IDeleteLabourDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteLabourDialogueModelState {
	
	public static GetMerged(mergeValues: Record<string, any>): IDeleteLabourDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IDeleteLabourDialogueModelState {
		const ret: IDeleteLabourDialogueModelState = {
			redirectToIndex: false,
			id: null,
		};
		
		return ret;
	}
	
}


export default {};

