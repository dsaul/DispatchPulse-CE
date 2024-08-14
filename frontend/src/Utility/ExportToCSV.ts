// https://stackoverflow.com/questions/14964035/how-to-export-javascript-array-info-to-csv-on-client-side

export default (filename: string, rows: any[]): void => {
	const ProcessRow = (row: any) => {
		let finalVal = "";
		for (let j = 0; j < row.length; j++) {
			let innerValue = row[j] === null ? "" : row[j].toString();

			if (row[j] instanceof Date) {
				innerValue = row[j].toLocaleString();
			}

			let result = innerValue.replace(/"/g, '""');
			if (result.search(/("|,|\n)/g) >= 0) {
				result = '"' + result + '"';
			}

			if (j > 0) {
				finalVal += ",";
			}
			finalVal += result;
		}
		return finalVal + "\n";
	};

	let csvFile = "";

	for (const row of rows) {
		csvFile += ProcessRow(row);
	}

	const blob = new Blob([csvFile], { type: "text/csv;charset=utf-8;" });

	if (navigator.msSaveBlob) {
		// IE 10+

		navigator.msSaveBlob(blob, filename);
	} else {
		const link = document.createElement("a");

		if (link.download !== undefined) {
			// feature detection

			if (!(window as any).Cypress) {
				// Browsers that support HTML5 download attribute
				const url = URL.createObjectURL(blob);
				link.setAttribute("href", url);
				link.setAttribute("download", filename);
				link.style.visibility = "hidden";
				document.body.appendChild(link);
				link.click();
				document.body.removeChild(link);
			}
		}
	}
};
