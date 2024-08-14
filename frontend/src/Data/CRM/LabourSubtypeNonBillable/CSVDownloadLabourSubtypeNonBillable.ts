import store from "@/plugins/store/store";
import ExportToCSV from "@/Utility/ExportToCSV";
import { ILabourSubtypeNonBillable } from "./LabourSubtypeNonBillable";

export default (): void => {
	const all: Record<string, ILabourSubtypeNonBillable> =
		store.state.Database.labourSubtypeNonBillable;
	if (!all) {
		return;
	}

	const array = [];

	array.push([
		"Export Type:",
		"LabourNonBillableDefinitions",
		"Export Version:",
		"CSV1"
	]);
	array.push(["id", "name", "description", "icon"]);

	for (const o of Object.values(all)) {
		array.push([
			o.id || "",
			o.json.name || "",
			o.json.description || "",
			o.json.icon || ""
		]);
	}

	ExportToCSV("LabourNonBillableDefinitions-All.csv", array);
};
