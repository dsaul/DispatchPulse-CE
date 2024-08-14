import _ from "lodash";
import GenerateID from "@/Utility/GenerateID";
import { guid } from "@/Utility/GlobalTypes";

export interface ILabeledContactId {
	id: string;
	value: guid | null;
	label: string | null;
}

export class LabeledContactId {
	public static GetMerged(
		mergeValues: Record<string, any>
	): ILabeledContactId {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): ILabeledContactId {
		const id = GenerateID();
		const ret: ILabeledContactId = {
			id,
			value: null,
			label: null
		};

		return ret;
	}

	public static ValidateObject(o: LabeledContactId): LabeledContactId {
		return o;
	}
}

export default {};
