import _ from 'lodash';

export interface IDeleteProjectNoteDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteProjectNoteDialogueModelState {
	
	public static GetMerged(mergeValues: Record<string, any>): IDeleteProjectNoteDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IDeleteProjectNoteDialogueModelState {
		const ret: IDeleteProjectNoteDialogueModelState = {
			redirectToIndex: false,
			id: null,
		};
		
		return ret;
	}
	
}



export default {};

