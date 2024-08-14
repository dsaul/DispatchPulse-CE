//https://stackoverflow.com/questions/1199352/smart-way-to-truncate-long-strings

export default (str: string, numChars: number, useWordBoundary = true, useLatex = false): string => {
	if (str.length <= numChars) {
		return str;
	}
	const subString = str.substr(0, numChars - 1);
	
	return (
		useWordBoundary ? 
			subString.substr(0, subString.lastIndexOf(' ')) : 
			subString) + (useLatex ? '\\ldots' : '…');
};
