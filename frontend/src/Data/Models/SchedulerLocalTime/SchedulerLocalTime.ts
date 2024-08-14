import _ from "lodash";

export interface ISchedulerLocalTime {
	hour: number | null;
	minute: number | null;
	second: number | null;
}

export class SchedulerLocalTime {
	public static GetMerged(
		mergeValues: Record<string, any>
	): ISchedulerLocalTime {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): ISchedulerLocalTime {
		const ret: ISchedulerLocalTime = {
			hour: null,
			minute: null,
			second: null
		};

		return ret;
	}
}

export default {};
