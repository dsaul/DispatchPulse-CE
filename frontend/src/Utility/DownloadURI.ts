// https://stackoverflow.com/questions/14964035/how-to-export-javascript-array-info-to-csv-on-client-side

export default (uri: string): void => {
	const link = document.createElement("a");

	if (link.download !== undefined) {
		// feature detection

		if (!(window as any).Cypress) {
			// Browsers that support HTML5 download attribute
			link.setAttribute("href", uri);
			link.setAttribute("download", "");
			link.style.visibility = "hidden";
			document.body.appendChild(link);
			link.click();
			document.body.removeChild(link);
		}
	}
};
