import store from "@/plugins/store/store";
import ExportToCSV from "@/Utility/ExportToCSV";
import { IMaterial } from "./Material";

export default (): void => {
	const all: Record<string, IMaterial> = store.state.Database.materials;
	if (!all) {
		return;
	}

	const array = [
		["Export Type:", "Materials", "Export Version:", "CSV1"],
		[
			"id",
			"lastModifiedISO8601",
			"lastModifiedBillingId",
			"dateUsedISO8601",
			"projectId",
			"quantity",
			"quantityUnit",
			"productId",
			"isExtra",
			"isBilled",
			"location",
			"notes"
		]
	];

	for (const contact of Object.values(all)) {
		array.push([
			contact.id || "",
			contact.lastModifiedISO8601 || "",
			contact.json.lastModifiedBillingId || "",
			contact.json.dateUsedISO8601 || "",
			contact.json.projectId || "",
			`${contact.json.quantity}` || "",
			contact.json.quantityUnit || "",
			contact.json.productId || "",
			contact.json.isExtra ? "YES" : "NO" || "",
			contact.json.isBilled ? "YES" : "NO" || "",
			contact.json.location || "",
			contact.json.notes || ""
		]);
	}

	ExportToCSV("Materials-All.csv", array);
};
