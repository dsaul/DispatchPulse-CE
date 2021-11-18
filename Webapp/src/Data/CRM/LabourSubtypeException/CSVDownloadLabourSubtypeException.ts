
import store from '@/plugins/store/store';
import ExportToCSV from '@/Utility/ExportToCSV';
import { ILabourSubtypeException } from './LabourSubtypeException';

export default (): void => {
	const all: Record<string, ILabourSubtypeException> = store.state.Database.labourSubtypeException;
	if (!all) {
		return;
	}
	
	
	const array = [];
	
	array.push([
		'Export Type:',
		'LabourExceptionDefinitions',
		'Export Version:',
		'CSV1',
	]);
	array.push([
		'id',
		'name',
		'description',
		'icon',
	]);
	
	for (const o of Object.values(all)) {
		array.push([
			o.id || '',
			o.json.name || '',
			o.json.description || '',
			o.json.icon || '',
		]);
	}
	
	ExportToCSV('LabourExceptionDefinitions-All.csv', array);
};

