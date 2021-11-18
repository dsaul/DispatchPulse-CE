
import store from '@/plugins/store/store';
import ExportToCSV from '@/Utility/ExportToCSV';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import { IProject } from './Project';
import { IAddress } from '@/Data/Models/Address/Address';
import { ILabeledContactId } from '@/Data/Models/LabeledContactId/LabeledContactId';
import { ILabeledCompanyId } from '@/Data/Models/LabeledCompanyId/LabeledCompanyId';
import { IProjectNote } from '../ProjectNote/ProjectNote';
import { IProjectNoteStyledText } from '../ProjectNoteStyledText/ProjectNoteStyledText';
import { IProjectNoteCheckbox } from '../ProjectNoteCheckbox/ProjectNoteCheckbox';
import { IProjectNoteImage } from '../ProjectNoteImage/ProjectNoteImage';
import { IProjectNoteVideo } from '../ProjectNoteVideo/ProjectNoteVideo';
import { IAssignment } from '../Assignment/Assignment';
import { IMaterial } from '../Material/Material';

export default (project: IProject): void => {
		
	console.log('DoExportToCSV', project);
	
	if (!project ||
		!project.json) {
		return;
	}
	
	
	const AddressesToCSVString = (addresses: IAddress[]) => {
		let s = '';
		for (const address of addresses) {
			s += `${address.label}: ${address.value}\n`;
		}
		return s;
	};
	
	const ContactsToCSVString = (contacts: ILabeledContactId[]) => {
		let s = '';
		for (const contact of contacts) {
			s += `${contact.label}: ${contact.value}\n`;
		}
		return s;
	};
	
	const CompaniesToCSVString = (companies: ILabeledCompanyId[]) => {
		let s = '';
		for (const company of companies) {
			s += `${company.label}: ${company.value}\n`;
		}
		return s;
	};
	
	
	
	
	
	const array = [
		[
			'Export Type:',
			'Projects',
			'Export Version:',
			'CSV1',
		],
		[
			'id',
			'parentId',
			'lastModifiedISO8601',
			'lastModifiedBillingId',
			'name',
			'statusId',
			'addresses',
			'contacts',
			'companies',
			'hasStartISO8601',
			'startTimeMode',
			'startISO8601',
			'hasEndISO8601',
			'endTimeMode',
			'endISO8601',
		],
		[
			project.id || '',
			project.json.parentId || '',
			project.lastModifiedISO8601 || '',
			project.json.lastModifiedBillingId || '',
			project.json.name || '',
			project.json.statusId || '',
			AddressesToCSVString(project.json.addresses),
			ContactsToCSVString(project.json.contacts),
			CompaniesToCSVString(project.json.companies),
			project.json.hasStartISO8601 ? 'YES' : 'NO' || '',
			project.json.startTimeMode || '',
			project.json.startISO8601 || '',
			project.json.hasEndISO8601 ? 'YES' : 'NO' || '',
			project.json.endTimeMode || '',
			project.json.endISO8601 || '',
		],
		[
			'Export Type:',
			'ProjectNotes',
			'Export Version:',
			'CSV1',
		],
		[
			'id',
			'lastModifiedISO8601',
			'lastModifiedBillingId',
			'projectId',
			'contentType',
			'contentHTML',
			'contentText',
			'contentCheckboxState',
			'contentURI',
		],
	];
	
	const allNotes = store.state.Database.projectNotes as Record<string, IProjectNote>;
	for (const note of Object.values(allNotes)) {
		if (project.id !== note.json.projectId) {
			continue;
		}
		
		let contentHTML = '';
		if (note.json.contentType === 'styled-text') {
			contentHTML = (note.json.content as IProjectNoteStyledText).html;
		}
		
		let contentText = '';
		let contentCheckboxState = '';
		if (note.json.contentType === 'checkbox') {
			contentText = (note.json.content as IProjectNoteCheckbox).text;
			contentCheckboxState = (note.json.content as IProjectNoteCheckbox).checkboxState ? 'YES' : 'NO';
		}
		
		let contentURI = '';
		if (note.json.contentType === 'image' || note.json.contentType === 'video') {
			contentURI = (note.json.content as IProjectNoteImage | IProjectNoteVideo).uri;
		}
		
		array.push([
			note.id || '',
			note.lastModifiedISO8601 || '',
			note.json.lastModifiedBillingId || '',
			note.json.projectId || '',
			note.json.contentType || '',
			contentHTML || '',
			contentText || '',
			contentCheckboxState || '',
			contentURI || '',
		]);
		
		
	}
	
	array.push([
		'Export Type:',
		'Assignments',
		'Export Version:',
		'CSV1',
	]);
	array.push([
		'id',
		'lastModifiedISO8601',
		'lastModifiedBillingId',
		'projectId',
		'agentId',
		'workRequested',
		'workPerformed',
		'internalComments',
		'hasStartISO8601',
		'startTimeMode',
		'startISO8601',
		'hasEndISO8601',
		'endTimeMode',
		'endISO8601',
		'statusId',
	]);
	
	const allAssignments = store.state.Database.assignments as Record<string, IAssignment>;
	for (const o of Object.values(allAssignments)) {
		if (project.id !== o.json.projectId) {
			continue;
		}
		
		array.push([
			o.id || '',
			o.lastModifiedISO8601 || '',
			o.json.lastModifiedBillingId || '',
			o.json.projectId || '',
			(o.json.agentIds.join(' ')) || '',
			o.json.workRequested || '',
			o.json.workPerformed || '',
			o.json.internalComments || '',
			o.json.hasStartISO8601 ? 'YES' : 'NO' || '',
			o.json.startTimeMode || '',
			o.json.startISO8601 || '',
			o.json.hasEndISO8601 ? 'YES' : 'NO' || '',
			o.json.endTimeMode || '',
			o.json.endISO8601 || '',
			o.json.statusId || '',
		]);
		
		
	}
	
	
	
	
	
	array.push([
		'Export Type:',
		'Materials',
		'Export Version:',
		'CSV1',
	]);
	
	array.push([
		'id',
		'lastModifiedISO8601',
		'lastModifiedBillingId',
		'dateUsedISO8601',
		'projectId',
		'quantity',
		'quantityUnit',
		'productId',
		'isExtra',
		'isBilled',
		'location',
		'notes',
	]);
	
	const allMaterials = store.state.Database.materials as Record<string, IMaterial>;
	for (const o of Object.values(allMaterials)) {
		if (project.id !== o.json.projectId) {
			continue;
		}
		
		
		array.push([
			o.id || '',
			o.lastModifiedISO8601 || '',
			o.json.lastModifiedBillingId || '',
			o.json.dateUsedISO8601 || '',
			o.json.projectId || '',
			`${o.json.quantity}`,
			o.json.quantityUnit || '',
			o.json.productId || '',
			o.json.isExtra ? 'YES' : 'NO' || '',
			o.json.isBilled ? 'YES' : 'NO' || '',
			o.json.location || '',
			o.json.notes || '',
		]);
		
	}
	
	
	
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
		if (project.id !== o.json.projectId) {
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
	
	
	
	
	
	
	ExportToCSV(`Projects-1.${project.json.name}.csv`, array);
	
	
	
};
