import { DateTime, DurationObjectUnits } from 'luxon';

export default (start: string, end: string): DurationObjectUnits => {
		
	const startISO = DateTime.fromISO(start);
	const startLocal = startISO.toLocal();
	const endISO = DateTime.fromISO(end);
	const endLocal = endISO.toLocal();
	const res = endLocal.diff(startLocal, ['hours', 'minutes', 'seconds']).toObject();
	
	if (!res.hasOwnProperty('hours') || !res.hasOwnProperty('minutes') || !res.hasOwnProperty('seconds')) {
		console.error('DataTableBase.Difference() !res', res, start, end);
		return { 
			hours: -9999, 
			minutes: -9999, 
			seconds: -9999 } as DurationObjectUnits; // Return something obviously wrong.
	}
	
	//console.debug('DataTableBase.Difference() !res', res, start, end);
	
	return res;
};
