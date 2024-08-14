
import store from '@/plugins/store/store';
import ExportToCSV from '@/Utility/ExportToCSV';
import { IProduct } from './Product';

export default (): void => {
	const all: Record<string, IProduct> = store.state.Database.products;
	if (!all) {
		return;
	}
	
	
	const array = [];
	
	array.push([
		'Export Type:',
		'ProductDefinitions',
		'Export Version:',
		'CSV1',
	]);
	array.push([
		'id',
		'name',
		'quantityUnit',
	]);
	
	for (const o of Object.values(all)) {
		array.push([
			o.id || '',
			o.json.name || '',
			o.json.quantityUnit || '',
		]);
	}
	
	ExportToCSV('ProductDefinitions-All.csv', array);
};

