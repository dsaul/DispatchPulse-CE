// https://stackoverflow.com/questions/822452/strip-html-from-text-javascript/47140708#47140708
export default (html: string): string => {
	const doc = new DOMParser().parseFromString(html, "text/html");
	return doc.body.textContent || "";
};
