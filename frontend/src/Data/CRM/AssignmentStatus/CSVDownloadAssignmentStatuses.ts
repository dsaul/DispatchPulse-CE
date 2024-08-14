import store from "@/plugins/store/store";
import ExportToCSV from "@/Utility/ExportToCSV";
import { IAssignmentStatus } from "./AssignmentStatus";

export default (): void => {
	const all: Record<string, IAssignmentStatus> =
		store.state.Database.assignmentStatus;
	if (!all) {
		return;
	}

	const array = [];

	array.push(["Export Type:", "AssignmentStatus", "Export Version:", "CSV1"]);
	array.push([
		"id",
		"name",
		"isOpen",
		"isReOpened",
		"isAssigned",
		"isWaitingOnClient",
		"isWaitingOnVendor",
		"isBillable",
		"isBillableReview",
		"isDefault"
	]);

	for (const o of Object.values(all)) {
		array.push([
			o.id || "",
			o.json.name || "",
			o.json.isOpen ? "YES" : "NO" || "",
			o.json.isReOpened ? "YES" : "NO" || "",
			o.json.isAssigned ? "YES" : "NO" || "",
			o.json.isWaitingOnClient ? "YES" : "NO" || "",
			o.json.isWaitingOnVendor ? "YES" : "NO" || "",
			o.json.isBillable ? "YES" : "NO" || "",
			o.json.isBillableReview ? "YES" : "NO" || "",
			o.json.isDefault ? "YES" : "NO" || ""
		]);
	}

	ExportToCSV("AssignmentStatus-All.csv", array);
};
