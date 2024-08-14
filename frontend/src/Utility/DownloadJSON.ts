// https://stackoverflow.com/questions/14964035/how-to-export-javascript-array-info-to-csv-on-client-side

export default (text: string, filename: string): void => {
	const link = document.createElement("a");

	if (link.download !== undefined) {
		// feature detection

		// Browsers that support HTML5 download attribute

		if (!(window as any).Cypress) {
			const blob = new Blob([text], { type: "application/json" });
			const url = URL.createObjectURL(blob);
			link.setAttribute("href", url);
			link.setAttribute("download", filename);
			link.style.visibility = "hidden";
			document.body.appendChild(link);
			link.click();
			document.body.removeChild(link);
		}
	}
};
