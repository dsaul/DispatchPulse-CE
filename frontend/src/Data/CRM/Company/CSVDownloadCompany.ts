import ExportToCSV from "@/Utility/ExportToCSV";
import { ICompany } from "./Company";

export default (company: ICompany): void => {
	console.log("DoExportToCSV", company);

	if (!company || !company.json) {
		return;
	}

	const array = [
		["Export Type:", "Companies", "Export Version:", "CSV1"],
		[
			"id",
			"name",
			"logoURI",
			"websiteURI",
			"lastModifiedISO8601",
			"lastModifiedBillingId"
		],
		[
			company.id || "",
			company.json.name || "",
			company.json.logoURI || "",
			company.json.websiteURI || "",
			company.lastModifiedISO8601 || "",
			company.json.lastModifiedBillingId || ""
		]
	];

	ExportToCSV(`Companies-1.${company.json.name}.csv`, array);
};
