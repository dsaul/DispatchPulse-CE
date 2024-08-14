import GenerateID from "@/Utility/GenerateID";
import { guid } from "@/Utility/GlobalTypes";
import _ from "lodash";

export interface ILabeledCompanyId {
	id: string;
	value: guid | null;
	label: string | null;
}

export class LabeledCompanyId {
	public static GetMerged(
		mergeValues: Record<string, any>
	): ILabeledCompanyId {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): ILabeledCompanyId {
		const id = GenerateID();
		const ret: ILabeledCompanyId = {
			id,
			value: null,
			label: null
		};

		return ret;
	}

	public static ValidateObject(o: ILabeledCompanyId): ILabeledCompanyId {
		return o;
	}
}

export default {};
