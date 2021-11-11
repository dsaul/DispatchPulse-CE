import ISO8601ToLocalDateOnly from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDateOnly';

export default (start: string | null, end: string | null): string | null => {
		

		
	let startFormat: string | null = null;
	let endFormat: string | null = null;
	
	if (start == null) {
		startFormat = '?';
	} else {
		startFormat = ISO8601ToLocalDateOnly(start);
	}
	
	if (end == null) {
		endFormat = '?';
	} else {
		endFormat = ISO8601ToLocalDateOnly(end);
	}
	
	
	if (startFormat === endFormat) {
		return startFormat;
	} else {
		return `${startFormat}—${endFormat}`;
	}
	
};
