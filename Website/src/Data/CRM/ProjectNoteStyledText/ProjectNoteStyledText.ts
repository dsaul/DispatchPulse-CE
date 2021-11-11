import _ from 'lodash';

export interface IProjectNoteStyledText {
	html: string;
}

export class ProjectNoteStyledText {
	
	public static GetMerged(mergeValues: Record<string, any>): IProjectNoteStyledText {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IProjectNoteStyledText {
		const ret: IProjectNoteStyledText = {
			html: '',
		};
		
		return ret;
	}
	
	public static ValidateObject(o: IProjectNoteStyledText): IProjectNoteStyledText {
		
		
		
		return o;
	}
	
}













export default {};
