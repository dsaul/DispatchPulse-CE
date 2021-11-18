import _ from 'lodash';

export interface IDeleteEmploymentStatusDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteEmploymentStatusDialogueModelState {
	
	public static GetMerged(mergeValues: Record<string, any>): IDeleteEmploymentStatusDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IDeleteEmploymentStatusDialogueModelState {
		const ret: IDeleteEmploymentStatusDialogueModelState = {
			redirectToIndex: false,
			id: null,
		};
		
		return ret;
	}
	
}
 

export default {};

