import { IAddress } from "@/Data/Models/Address/Address";
import { IEMail } from "@/Data/Models/EMail/EMail";
import { IPhoneNumber } from "@/Data/Models/PhoneNumber/PhoneNumber";
import ExportToCSV from "@/Utility/ExportToCSV";
import { IContact } from "./Contact";

export default (contact: IContact): void => {
	console.log("DoExportToCSV", contact);

	if (!contact || !contact.json) {
		return;
	}

	const PhoneNumbersToCSVString = (numbers: IPhoneNumber[]) => {
		let s = "";
		for (const number of numbers) {
			s += `${number.label}: ${number.value}\n`;
		}
		return s;
	};
	const EMailsToCSVString = (emails: IEMail[]) => {
		let s = "";
		for (const email of emails) {
			s += `${email.label}: ${email.value}\n`;
		}
		return s;
	};
	const AddressesToCSVString = (addresses: IAddress[]) => {
		let s = "";
		for (const address of addresses) {
			s += `${address.label}: ${address.value}\n`;
		}
		return s;
	};

	const array = [
		["Export Type:", "Contacts", "Export Version:", "CSV1"],
		[
			"id",
			"lastModifiedISO8601",
			"lastModifiedBillingId",
			"name",
			"title",
			"companyId",
			"notes",
			"phoneNumbers",
			"emails",
			"addresses"
		],
		[
			contact.id || "",
			contact.lastModifiedISO8601 || "",
			contact.json.lastModifiedBillingId || "",
			contact.json.name || "",
			contact.json.title || "",
			contact.json.companyId || "",
			contact.json.notes || "",
			PhoneNumbersToCSVString(contact.json.phoneNumbers),
			EMailsToCSVString(contact.json.emails),
			AddressesToCSVString(contact.json.addresses)
		]
	];

	ExportToCSV(`Contacts-1.${contact.json.name}.csv`, array);
};
