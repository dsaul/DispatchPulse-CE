import { DateTime } from 'luxon';

export default (date: DateTime | null): string | null => {
		
	if (!date) {
		return null;
	}
	
	return date.toFormat('yyyy-LL-dd');
	
	
};
