import _ from "lodash";

export interface IChangeCompanyPhoneIdModel {
	originalId: string | null;
	newId: string | null;
}

export class ChangeCompanyPhoneIdModel {
	public static GetMerged(
		mergeValues: Record<string, any>
	): IChangeCompanyPhoneIdModel {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IChangeCompanyPhoneIdModel {
		const ret: IChangeCompanyPhoneIdModel = {
			originalId: null,
			newId: null
		};

		return ret;
	}
}

export default {};
