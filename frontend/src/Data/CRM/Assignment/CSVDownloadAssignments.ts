import store from "@/plugins/store/store";
import ExportToCSV from "@/Utility/ExportToCSV";
import { Assignment, IAssignment } from "@/Data/CRM/Assignment/Assignment";

export default (): void => {
	const all: Record<string, IAssignment> = store.state.Database.assignments;
	if (!all) {
		return;
	}

	const array = [];
	array.push(["Export Type:", "Assignments", "Export Version:", "CSV1"]);
	array.push([
		"id",
		"lastModifiedISO8601",
		"lastModifiedBillingId",
		"projectId",
		"agentId",
		"workRequested",
		"workPerformed",
		"internalComments",
		"hasStartISO8601",
		"startTimeMode",
		"startISO8601",
		"hasEndISO8601",
		"endTimeMode",
		"endISO8601",
		"statusId"
	]);

	for (const o of Object.values(all)) {
		array.push([
			o.id || "",
			o.lastModifiedISO8601 || "",
			o.json.lastModifiedBillingId || "",
			o.json.projectId || "",
			o.json.agentId || "",
			o.json.workRequested || "",
			o.json.workPerformed || "",
			o.json.internalComments || "",
			Assignment.HasStartISO8601ForId(o.id || null) ? "YES" : "NO" || "",
			Assignment.StartTimeModeForId(o.id || null) || "",
			Assignment.StartISO8601ForId(o.id || null) || "",
			Assignment.HasEndISO8601ForId(o.id || null) ? "YES" : "NO" || "",
			Assignment.EndTimeModeForId(o.id || null) || "",
			Assignment.EndISO8601ForId(o.id || null) || "",
			o.json.statusId || ""
		]);
	}

	ExportToCSV("Assignments-All.csv", array);
};
