import { ILabeledContactId } from "@/Data/Models/LabeledContactId/LabeledContactId";
import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";

export default (val: Array<ILabeledContactId>): boolean | string => {
	//console.log('EnsureContactArrayHasAtLeastOneContact', val);

	let validated = false;
	for (const o of val) {
		if (!o) {
			continue;
		}
		if (!o.value) {
			continue;
		}
		if (IsNullOrEmpty(o.value)) {
			continue;
		}

		validated = true;
		break;
	}

	return !validated ? "At least one contact must be entered." : true;
};
