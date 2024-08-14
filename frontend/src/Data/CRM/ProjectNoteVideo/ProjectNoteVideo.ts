import _ from 'lodash';

export interface IProjectNoteVideo {
	uri: string;
}


export class ProjectNoteVideo {
	
	public static GetMerged(mergeValues: Record<string, any>): IProjectNoteVideo {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IProjectNoteVideo {
		const ret: IProjectNoteVideo = {
			uri: '',
		};
		
		return ret;
	}
	
	public static ValidateObject(o: IProjectNoteVideo): IProjectNoteVideo {
		
		
		
		return o;
	}
	
}












export default {};
