
import store from '@/plugins/store/store';
import ExportToCSV from '@/Utility/ExportToCSV';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import { IAgent } from './Agent';

export default (agent: IAgent): void => {
	
	if (!agent ||
		!agent.json) {
		return;
	}
	
	const all: Record<string, IAgent> = store.state.Database.agents;
	if (!all) {
		return;
	}
	
	
	const array = [];
	
	array.push([
		'Export Type:',
		'Agents',
		'Export Version:',
		'CSV1',
	]);
	array.push([
		'id',
		'lastModifiedISO8601',
		'lastModifiedBillingId',
		'name',
		'title',
		'employmentStatusId',
		'hourlyWage',
		'notificationSMSNumber',
	]);
	
	array.push([
		agent.id || '',
		agent.lastModifiedISO8601 || '',
		agent.json.lastModifiedBillingId || '',
		agent.json.name || '',
		agent.json.title || '',
		agent.json.employmentStatusId || '',
		`${agent.json.hourlyWage}` || '',
		agent.json.notificationSMSNumber || '',
	]);
	
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
	
	const allLabour = store.state.Database.labour as Record<string, ILabour>;
	
	
	for (const o of Object.values(allLabour)) {
		if (agent.id !== o.json.agentId) {
			continue;
		}
		
		array.push([
			o.id || '',
			o.lastModifiedISO8601 || '',
			o.json.lastModifiedBillingId || '',
			o.json.projectId || '',
			o.json.agentId || '',
			o.json.assignmentId || '',
			o.json.typeId || '',
			o.json.timeMode || '',
			`${o.json.hours}` || '',
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
			`${o.json.bankedPayOutAmount}` || '',
		]);
	}
	
	
	
	
	
	
	
	
	
	
	
	ExportToCSV('Agents-1.csv', array);
};

