import _ from 'lodash';

export interface IDeleteAgentDialogueModelState {
	redirectToIndex: boolean;
	id: string | null;
}

export class DeleteAgentDialogueModelState {
	
	public static GetMerged(mergeValues: Record<string, any>): IDeleteAgentDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IDeleteAgentDialogueModelState {
		const ret: IDeleteAgentDialogueModelState = {
			redirectToIndex: false,
			id: null,
		};
		
		return ret;
	}
	
}

 

export default {};

