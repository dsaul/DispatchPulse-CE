import { DateTime } from 'luxon';

export default (iso: string | null): string | null => {
	if (!iso) {
		return null;
	}
	
	const date = DateTime.fromISO(iso);
	// Feb 26, 2021, 8:03 AM
	return date.toFormat('MMM d, yyyy \'at\' h:mm:ss.SSS a');
	
	
};
