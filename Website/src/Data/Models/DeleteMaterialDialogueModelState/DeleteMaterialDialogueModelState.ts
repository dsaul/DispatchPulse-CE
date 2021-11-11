import _ from 'lodash';

export interface IDeleteMaterialDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteMaterialDialogueModelState {
	
	
	public static GetMerged(mergeValues: Record<string, any>): IDeleteMaterialDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IDeleteMaterialDialogueModelState {
		const ret: IDeleteMaterialDialogueModelState = {
			redirectToIndex: false,
			id: null,
		};
		
		return ret;
	}
	
}


 

export default {};

