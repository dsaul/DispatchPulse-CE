import store from "@/plugins/store/store";
import ExportToCSV from "@/Utility/ExportToCSV";
import { IProjectStatus } from "./ProjectStatus";

export default (): void => {
	const all: Record<string, IProjectStatus> =
		store.state.Database.projectStatus;
	if (!all) {
		return;
	}

	const array = [];

	array.push(["Export Type:", "ProjectStatus", "Export Version:", "CSV1"]);
	array.push(["id", "name", "isOpen", "isAwaitingPayment", "isClosed"]);

	for (const o of Object.values(all)) {
		array.push([
			o.id || "",
			o.json.name || "",
			o.json.isOpen ? "YES" : "NO" || "",
			o.json.isAwaitingPayment ? "YES" : "NO" || "",
			o.json.isClosed ? "YES" : "NO" || ""
		]);
	}

	ExportToCSV("ProjectStatus-All.csv", array);
};
