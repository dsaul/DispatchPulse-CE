import store from "@/plugins/store/store";
import ExportToCSV from "@/Utility/ExportToCSV";
import { IEstimatingManHours } from "./EstimatingManHours";

export default (): void => {
	const all: Record<string, IEstimatingManHours> =
		store.state.Database.estimatingManHours;
	if (!all) {
		return;
	}

	const array = [];

	array.push([
		"Export Type:",
		"ManHoursDefinitions",
		"Export Version:",
		"CSV1"
	]);
	array.push(["id", "item", "manHours", "measurement"]);

	for (const o of Object.values(all)) {
		array.push([
			o.id || "",
			o.json.item || "",
			o.json.manHours || "",
			o.json.measurement || ""
		]);
	}

	ExportToCSV("ManHoursDefinitions-All.csv", array);
};
