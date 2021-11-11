
import store from '@/plugins/store/store';
import ExportToCSV from '@/Utility/ExportToCSV';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';

export default (): void => {
	const all: Record<string, ILabour> = store.state.Database.labour;
	if (!all) {
		return;
	}
	
	
	const array = [];
	
	array.push([
		'Export Type:',
		'Labour',
		'Export Version:',
		'CSV1',
	]);
	
	array.push([
		'id',
		'lastModifiedISO8601',
		'lastModifiedBillingId',
		'projectId',
		'agentId',
		'assignmentId',
		'typeId',
		'timeMode',
		'hours',
		'startISO8601',
		'endISO8601',
		'isActive',
		'locationType',
		'isExtra',
		'isBilled',
		'isPaidOut',
		'exceptionTypeId',
		'holidayTypeId',
		'nonBillableTypeId',
		'notes',
		'bankedPayOutAmount',
	]);
	
	for (const o of Object.values(all)) {
		array.push([
			o.id || '',
			o.lastModifiedISO8601 || '',
			o.json.lastModifiedBillingId || '',
			o.json.projectId || '',
			o.json.agentId || '',
			o.json.assignmentId || '',
			o.json.typeId || '',
			o.json.timeMode || '',
			`${o.json.hours || ''}` || '',
			o.json.startISO8601 || '',
			o.json.endISO8601 || '',
			o.json.isActive ? 'YES' : 'NO' || '',
			o.json.locationType || '',
			Labour.IsExtraForId(o.id || null) ? 'YES' : 'NO' || '',
			o.json.isBilled ? 'YES' : 'NO' || '',
			o.json.isPaidOut ? 'YES' : 'NO' || '',
			o.json.exceptionTypeId || '',
			o.json.holidayTypeId || '',
			o.json.nonBillableTypeId || '',
			o.json.notes || '',
			`${o.json.bankedPayOutAmount || ''}` || '',
		]);
	}
	
	ExportToCSV('Labour-All.csv', array);
};

