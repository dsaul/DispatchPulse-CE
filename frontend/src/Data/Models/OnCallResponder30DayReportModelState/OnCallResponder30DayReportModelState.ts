import _ from "lodash";

export interface IOnCallResponder30DayReportModelState {
	// tslint:disable-line
	_renderingActive: boolean;
	_showProgress: boolean;
	_renderingProgressMessage: string;
	_renderingComplete: boolean;
	_downloadLink: string | null;
	_errorMessage: string;
}

export class OnCallResponder30DayReportModelState {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IOnCallResponder30DayReportModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IOnCallResponder30DayReportModelState {
		const ret: IOnCallResponder30DayReportModelState = {
			_renderingActive: false,
			_showProgress: false,
			_renderingComplete: false,
			_downloadLink: null,
			_renderingProgressMessage: "",
			_errorMessage: ""
		};

		return ret;
	}
}

export default {};
