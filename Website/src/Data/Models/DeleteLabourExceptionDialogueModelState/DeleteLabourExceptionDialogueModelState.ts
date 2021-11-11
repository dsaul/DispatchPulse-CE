import _ from 'lodash';

export interface IDeleteLabourExceptionDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}


export class DeleteLabourExceptionDialogueModelState {
	
	public static GetMerged(mergeValues: Record<string, any>): IDeleteLabourExceptionDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IDeleteLabourExceptionDialogueModelState {
		const ret: IDeleteLabourExceptionDialogueModelState = {
			redirectToIndex: false,
			id: null,
		};
		
		return ret;
	}
	
}




export default {};

