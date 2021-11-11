
import store from '@/plugins/store/store';
import ExportToCSV from '@/Utility/ExportToCSV';
import { ICompany } from './Company';

export default (): void => {
	
	const all: Record<string, ICompany> = store.state.Database.companies;
	if (!all) {
		return;
	}
	
	const array = [
		[
			'Export Type:',
			'Companies',
			'Export Version:',
			'CSV1',
		],
		[
			'id',
			'name',
			'logoURI',
			'websiteURI',
			'lastModifiedISO8601',
			'lastModifiedBillingId',
		],
		
		
	];
	
	for (const o of Object.values(all)) {
		array.push([
			o.id || '',
			o.json.name || '',
			o.json.logoURI || '',
			o.json.websiteURI || '',
			o.lastModifiedISO8601 || '',
			o.json.lastModifiedBillingId || '',
		]);
	}
	
	ExportToCSV('Companies-All.csv', array);
	
};

