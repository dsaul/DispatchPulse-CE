
import store from '@/plugins/store/store';
import ExportToCSV from '@/Utility/ExportToCSV';
import { ILabourSubtypeHoliday } from './LabourSubtypeHoliday';

export default (): void => {
	const all: Record<string, ILabourSubtypeHoliday> = store.state.Database.labourSubtypeHolidays;
	if (!all) {
		return;
	}
	
	
	const array = [];
	
	array.push([
		'Export Type:',
		'LabourHolidayDefinitions',
		'Export Version:',
		'CSV1',
	]);
	array.push([
		'id',
		'name',
		'icon',
		'description',
		'isStaticDate',
		'staticDateMonth',
		'staticDateDay',
		'isObservationDay',
		'observationDayStatic',
		'observationDayStaticMonth',
		'observationDayStaticDay',
		'observationDayActivateIfWeekend',
		'isFirstMondayInMonthDate',
		'firstMondayMonth',
		'isGoodFriday',
		'isThirdMondayInMonthDate',
		'thirdMondayMonth',
		'isSecondMondayInMonthDate',
		'secondMondayMonth',
		'isMondayBeforeDate',
		'mondayBeforeDateMonth',
		'mondayBeforeDateDay',
	]);
	
	for (const o of Object.values(all)) {
		array.push([
			o.id || '',
			o.json.name || '',
			o.json.icon || '',
			o.json.description || '',
			o.json.isStaticDate ? 'YES' : 'NO' || '',
			`${o.json.staticDateMonth || ''}`,
			`${o.json.staticDateDay || ''}` || '',
			o.json.isObservationDay ? 'YES' : 'NO' || '',
			o.json.observationDayStatic ? 'YES' : 'NO' || '',
			`${o.json.observationDayStaticMonth || ''}` || '',
			`${o.json.observationDayStaticDay || ''}` || '',
			o.json.observationDayActivateIfWeekend ? 'YES' : 'NO' || '',
			o.json.isFirstMondayInMonthDate ? 'YES' : 'NO' || '',
			`${o.json.firstMondayMonth || ''}` || '',
			o.json.isGoodFriday ? 'YES' : 'NO' || '',
			o.json.isThirdMondayInMonthDate ? 'YES' : 'NO' || '',
			`${o.json.thirdMondayMonth || ''}` || '',
			o.json.isSecondMondayInMonthDate ? 'YES' : 'NO' || '',
			`${o.json.secondMondayMonth || ''}` || '',
			o.json.isMondayBeforeDate ? 'YES' : 'NO' || '',
			`${o.json.mondayBeforeDateMonth || ''}` || '',
			`${o.json.mondayBeforeDateDay || ''}` || '',
		]);
	}
	
	ExportToCSV('LabourHolidayDefinitions-All.csv', array);
};

