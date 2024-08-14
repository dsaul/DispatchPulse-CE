import _ from "lodash";

export interface IProjectNoteImage {
	uri: string;
}

export class ProjectNoteImage {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IProjectNoteImage {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IProjectNoteImage {
		const ret: IProjectNoteImage = {
			uri: ""
		};

		return ret;
	}

	public static ValidateObject(o: IProjectNoteImage): IProjectNoteImage {
		return o;
	}
}

export default {};
