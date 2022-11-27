
import store from '@/plugins/store/store';
import ExportToCSV from '@/Utility/ExportToCSV';
import { IEmploymentStatus } from './EmploymentStatus';

export default (): void => {
	
	const all: Record<string, IEmploymentStatus> = store.state.Database.agentsEmploymentStatus;
	if (!all) {
		return;
	}
	
	
	const array = [];
	
	array.push([
		'Export Type:',
		'EmploymentStatusDefinitions',
		'Export Version:',
		'CSV1',
	]);
	array.push([
		'id',
		'name',
	]);
	
	for (const o of Object.values(all)) {
		array.push([
			o.id || '',
			o.json.name || '',
		]);
	}
	
	ExportToCSV('EmploymentStatusDefinitions-All.csv', array);
		
};

