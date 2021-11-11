import _ from 'lodash';

export interface IProjectNoteCheckbox {
	text: string;
	checkboxState: boolean;
}

export class ProjectNoteCheckbox {
	
	public static GetMerged(mergeValues: Record<string, any>): IProjectNoteCheckbox {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IProjectNoteCheckbox {
		const ret: IProjectNoteCheckbox = {
			text: '',
			checkboxState: false,
		};
		
		return ret;
	}
	
	
	public static ValidateObject(o: IProjectNoteCheckbox): IProjectNoteCheckbox {
		
		
		
		return o;
	}
	
	
}













export default {};
