
import _ from 'lodash';

export interface IDemoIntroductionDialogueModelState {// tslint:disable-line
	
}

export class DemoIntroductionDialogueModelState {
	
	public static GetMerged(mergeValues: Record<string, any>): IDemoIntroductionDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IDemoIntroductionDialogueModelState {
		const ret: IDemoIntroductionDialogueModelState = {};
		
		return ret;
	}
	
}


 

export default {};

