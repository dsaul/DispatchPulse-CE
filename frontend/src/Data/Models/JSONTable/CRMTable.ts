import _ from "lodash";
import GenerateID from "@/Utility/GenerateID";
import { IJSONTable } from "./JSONTable";

export interface ICRMTable extends IJSONTable {
	id?: string;
	json: Record<string, any>;
	searchString: string | null;
	lastModifiedISO8601: string | null;
}

export class CRMTable {
	public static GetMerged(mergeValues: Record<string, any>): ICRMTable {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): ICRMTable {
		const id = GenerateID();
		const ret: ICRMTable = {
			id: id,
			json: {},
			searchString: null,
			lastModifiedISO8601: null
		};

		return ret;
	}
}
