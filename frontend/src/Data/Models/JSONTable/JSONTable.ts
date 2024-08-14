import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";

export interface IJSONTable {
	uuid?: string;
	id?: string;
	json: Record<string, any>;
}

export class JSONTable {
	public static GetMerged(mergeValues: Record<string, any>): IJSONTable {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IJSONTable {
		const id = GenerateID();
		const ret: IJSONTable = {
			id: id,
			json: {}
		};

		return ret;
	}
}

export default {};
