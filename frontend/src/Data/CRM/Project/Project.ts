import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import ProjectStatusForNewProjects from '@/Utility/DataAccess/ProjectStatusForNewProjects';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ISO8601ToLocalDateOnly from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDateOnly';
import ISO8601ToLocalDatetime from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDatetime';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { guid } from '@/Utility/GlobalTypes';
import ITracker from '@/Utility/ITracker';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestProjects } from '@/Data/CRM/Project/RPCRequestProjects';
import { RPCDeleteProjects } from '@/Data/CRM/Project/RPCDeleteProjects';
import { RPCPushProjects } from '@/Data/CRM/Project/RPCPushProjects';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IAddress } from '@/Data/Models/Address/Address';
import { ILabeledContactId } from '@/Data/Models/LabeledContactId/LabeledContactId';
import { ILabeledCompanyId } from '@/Data/Models/LabeledCompanyId/LabeledCompanyId';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { ILabourType } from '../LabourType/LabourType';

export interface IProject extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		parentId: string | null;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
		name: string | null;
		statusId: string | null;
		
		addresses: IAddress[];
		contacts: ILabeledContactId[];
		companies: ILabeledCompanyId[];
		
		hasStartISO8601: boolean;
		startTimeMode: 'none' | 'morning-first-thing' | 
			'morning-second-thing' | 'afternoon-first-thing' | 
			'afternoon-second-thing' | 'time';
		startISO8601: string | null;
		
		hasEndISO8601: boolean;
		endTimeMode: 'none' | 'time';
		endISO8601: string | null;
		
		forceLabourAsExtra: boolean | null;
		forceAssignmentsToUseProjectSchedule: boolean | null;
	};
}

export class Project {
	// RPC Methods
	public static RequestProjects = RPCMethod.Register<RPCRequestProjects>(new RPCRequestProjects());
	public static DeleteProjects = RPCMethod.Register<RPCDeleteProjects>(new RPCDeleteProjects());
	public static PushProjects = RPCMethod.Register<RPCPushProjects>(new RPCPushProjects());
	
	
	public static _RefreshTracker: { [id: string]: ITracker } = {};
	
	public static FetchForId(id: guid): IRoundTripRequest {
		
		const ret: IRoundTripRequest = {
			roundTripRequestId: GenerateID(),
			outboundRequestPromise: null,
			completeRequestPromise: null,
			_completeRequestPromiseResolve: null,
			_completeRequestPromiseReject: null,
		};
		
		
		// If we have no id, reject.
		if (!id || IsNullOrEmpty(id)) {
			ret.outboundRequestPromise = Promise.reject();
			ret.completeRequestPromise = Promise.reject();
			return ret;
		}
		
		// Remove all trackers that are complete and older than 5 seconds.
		const keys = Object.keys(this._RefreshTracker);
		for (const key of keys) {
			const tracker: ITracker = this._RefreshTracker[key];
			if (!tracker.isComplete) {
				continue;
			}
			
			if (DateTime.utc() > tracker.lastRequestTimeUtc.plus({seconds: 5})) {
				delete this._RefreshTracker[key];
			}
		}
		
		// Check and see if we already have a request pending.
		const existing = this._RefreshTracker[id];
		if (existing) {
			return existing.rtr;
		}
		
		
		const project = Project.ForId(id);
		if (project) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(project);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = Project.RequestProjects.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			limitToIds: [id],
		});
		
		ret.outboundRequestPromise = rtrNew.outboundRequestPromise;
		
		ret.completeRequestPromise = new Promise((resolve, reject) => {
			ret._completeRequestPromiseResolve = resolve;
			ret._completeRequestPromiseReject = reject;
		});
		
		
		// Handlers once we get a response.
		if (rtrNew.completeRequestPromise) {
			
			rtrNew.completeRequestPromise.then(() => {
				if (ret._completeRequestPromiseResolve) {
					ret._completeRequestPromiseResolve(Project.ForId(id));
				}
			});
			
			rtrNew.completeRequestPromise.catch((e: Error) => {
				if (ret._completeRequestPromiseReject) {
					ret._completeRequestPromiseReject(e);
				}
			});
			
			rtrNew.completeRequestPromise.finally(() => {
				this._RefreshTracker[id].isComplete = true;
			});
		}
		
		
		this._RefreshTracker[id] = {
			lastRequestTimeUtc: DateTime.utc(),
			isComplete: false,
			rtr: rtrNew,
		};
		
		return ret;
	}
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): IProject {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IProject {
		const id = GenerateID();
		const ret: IProject = {
			id,
			json: {
				id,
				parentId: null,
				lastModifiedISO8601: DateTime.utc().toISO(),
				lastModifiedBillingId: null,
				name: null,
				statusId: ProjectStatusForNewProjects(),
				
				addresses: [],
				contacts: [],
				companies: [],
				
				hasStartISO8601: false,
				startTimeMode: 'none',
				startISO8601: null,
				
				hasEndISO8601: false,
				endTimeMode: 'none',
				endISO8601: null,
				
				forceLabourAsExtra: true,
				forceAssignmentsToUseProjectSchedule: true,
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): IProject | null {
		if (!id) {
			return null;
		}
		
		const projects = store.state.Database.projects as Record<string, IProject>;
		if (!projects || Object.keys(projects).length === 0) {
			return null;
		}
		
		let project = projects[id];
		if (!project || !project.json) {
			project = CaseInsensitivePropertyGet(projects, id);
		}
		if (!project || !project.json) {
			return null;
		}
		
		return project;
	}
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteProjects', ids);
		
	}
	
	public static UpdateIds(payload: Record<string, IProject>): void {
		store.commit('UpdateProjects', payload);
	}
	
	
	
	
	
	public static AddressForId(id: string | null): string | null {
		
		try {
			if (id == null) {
				return null;
			}
			
			const project = Project.ForId(id);
			if (!project) {
				return null;
			}
			
			const addreses = project.json.addresses;
			const addressFiltered = [];
			for (const a of addreses) {
				if (a.value != null && !IsNullOrEmpty(a.value)) {
					addressFiltered.push(a.value);
				}
			}
			
			if (!addressFiltered || addressFiltered.length === 0) {
				return null;
			}
			
			let ret = '';
			
			for (let i = 0; i < addressFiltered.length; i++) {
			
				if (i !== 0) {
					ret += ', ';
				}
				if (i !== 0 && i === addressFiltered.length - 1) {
					ret += ' and ';
				}
				
				ret += addressFiltered[i];
				
			}
			
			return ret;
		} catch (e) {
			//console.error('Project.AddressForId', e);
			return null;
		}
		
	}
	
	public static NameForId(id: string | null): string | null {
		try {
			if (id == null) {
				return null;
			}
			
			const project = Project.ForId(id);
			if (!project) {
				return null;
			}
			
			
			
			return project.json.name;
		} catch (e) {
			//console.error('Project.NameForId', e);
			return null;
		}
	}
	
	public static CombinedDescriptionForId(id: string | null): string {
		
		let ret = '';
		
		const address = Project.AddressForId(id);
		const name = Project.NameForId(id);
		
		//console.log('address', address, 'name', name);
		
		if (null != address && !IsNullOrEmpty(address)) {
			ret += address;
			
			if (null != name && !IsNullOrEmpty(name)) {
				ret += ` (${name})`;
			}
		} else if (null != name && !IsNullOrEmpty(name)) {
			ret += name;
		} else {
			ret += `Unnamed, no address (${id})`;
		}
		
		return ret;
		
	}
	
	
	public static EndScheduleDescriptionForId(projectId: string): string {
		
		let ret = '';
	
		do {
			
			if (!projectId || IsNullOrEmpty(projectId)) {
				ret += 'No project.';
				break;
			}
			
			const project = Project.ForId(projectId);
			if (!project) {
				ret += 'No project.';
				break;
			}
			
			if (!project.json.hasEndISO8601) {
				ret += 'No end time.';
				break;
			}
			
			if ([
					'none',
				].indexOf(project.json.startTimeMode) !== -1) {
				
				ret += ISO8601ToLocalDateOnly(project.json.endISO8601);
				ret += ' ';
			} else {
				ret += ISO8601ToLocalDatetime(project.json.endISO8601);
				ret += ' ';
			}
			
			
			
			
		} while (false);
		
		
		return ret;
		
	}
	
	public static StartScheduleDescriptionForId(projectId: string): string {
		
		
		let ret = '';
	
		do {
			
			if (!projectId || IsNullOrEmpty(projectId)) {
				ret += 'No project.';
				break;
			}
			
			const project = Project.ForId(projectId);
			if (!project) {
				ret += 'No project.';
				break;
			}
			
			if (!project.json.hasStartISO8601) {
				ret += 'No start time.';
				break;
			}
			
			if ([
					'none',
					'morning-first-thing',
					'morning-second-thing',
					'afternoon-first-thing',
					'afternoon-second-thing',
				].indexOf(project.json.startTimeMode) !== -1) {
				
				ret += ISO8601ToLocalDateOnly(project.json.startISO8601);
				ret += ' ';
			} else {
				ret += ISO8601ToLocalDatetime(project.json.startISO8601);
				ret += ' ';
			}
			
			if (project.json.startTimeMode === 'morning-first-thing') {
				ret += 'First Thing in the Morning';
			} else if (project.json.startTimeMode === 'morning-second-thing') {
				ret += 'Second Thing in the Morning';
			} else if (project.json.startTimeMode === 'afternoon-first-thing') {
				ret += 'First Thing in the Afternoon';
			} else if (project.json.startTimeMode === 'afternoon-second-thing') {
				ret += 'Second Thing in the Afternoon';
			}
			
			
			
		} while (false);
		
		
		return ret;
		
		
	}
	
	
	
	public static StartTravelForId(id: string | null): void {
		
		if (!id) {
			console.error('Project.StartTravelOnId !id', id);
			return;
		}
		
		const agent = Agent.ForLoggedInAgent();
		if (!agent) {
			console.error('Project.StartTravelOnId !agent', agent);
			return;
		}
		if (!agent.id) {
			console.error('Project.StartTravelOnId !agent.id', agent);
			return;
		}
		
		
		const labour = Labour.GetEmpty();
		if (!labour.id) {
			console.error('!labour.id');
			return;
		}
		
		labour.lastModifiedISO8601 = DateTime.utc().toISO();
		labour.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		labour.json.projectId = id;
		labour.json.agentId = agent.id;
		// labour.json.assignmentId = id;
		labour.json.isEnteredThroughTelephoneCompanyAccess = false;
		
		// Find the default type id for billable.
		const allTypes = store.state.Database.labourTypes as Record<string, ILabourType>;
		let billableType = null;
		for (const type of Object.values(allTypes) as ILabourType[]) {
			if (type.json.isBillable === true && type.json.default === true) {
				billableType = type;
			}
		}
		
		if (!billableType) {
			console.error('Project.StartTravelOnId !billableType');
			return;
		}
		
		if (!billableType.id) {
			console.error('Project.StartTravelOnId !billableType.id');
			return;
		}
		
		labour.json.typeId = billableType.id;
		labour.json.timeMode = 'start-stop-timestamp';
		labour.json.hours = null;
		labour.json.startISO8601 = DateTime.utc().toISO();
		labour.json.endISO8601 = null;
		labour.json.isActive = true;
		labour.json.locationType = 'travel';
		labour.json.isExtra = null;
		labour.json.isBilled = null;
		labour.json.isPaidOut = null;
		
		labour.json.exceptionTypeId = null;
		labour.json.holidayTypeId = null;
		labour.json.nonBillableTypeId = null;
		
		labour.json.notes = null;
		labour.json.bankedPayOutAmount = null;
		
		//console.log('start travel', labour);
		
		const payload: Record<string, ILabour> = {};
		payload[labour.id] = labour;
		store.commit('UpdateLabour', payload);
		
	}
	
	public static StartOnSiteForId(id: string | null): void {
		
		if (!id) {
			console.error('Project.StartTravelOnId !id', id);
			return;
		}
		
		const agent = Agent.ForLoggedInAgent();
		if (!agent) {
			console.error('Project.StartTravelOnId !agent', agent);
			return;
		}
		if (!agent.id) {
			console.error('Project.StartTravelOnId !agent.id', agent);
			return;
		}
		
		const labour = Labour.GetEmpty();
		if (!labour.id) {
			console.error('!labour.id');
			return;
		}
		
		labour.lastModifiedISO8601 = DateTime.utc().toISO();
		labour.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		labour.json.projectId = id;
		labour.json.agentId = agent.id;
		// labour.json.assignmentId = id;
		labour.json.isEnteredThroughTelephoneCompanyAccess = false;
		
		// Find the default type id for billable.
		const allTypes = store.state.Database.labourTypes as Record<string, ILabourType>;
		let billableType = null;
		for (const type of Object.values(allTypes) as ILabourType[]) {
			if (type.json.isBillable === true && type.json.default === true) {
				billableType = type;
			}
		}
		
		if (!billableType) {
			console.error('Project.StartOnSiteOnId !billableType');
			return;
		}
		if (!billableType.id) {
			console.error('Project.StartOnSiteOnId !billableType.id');
			return;
		}
		
		labour.json.typeId = billableType.id;
		labour.json.timeMode = 'start-stop-timestamp';
		labour.json.hours = null;
		labour.json.startISO8601 = DateTime.utc().toISO();
		labour.json.endISO8601 = null;
		labour.json.isActive = true;
		labour.json.locationType = 'on-site';
		labour.json.isExtra = null;
		labour.json.isBilled = null;
		labour.json.isPaidOut = null;
		
		labour.json.exceptionTypeId = null;
		labour.json.holidayTypeId = null;
		labour.json.nonBillableTypeId = null;
		
		labour.json.notes = null;
		labour.json.bankedPayOutAmount = null;
		
		console.log('start on-site', labour);
		
		const payload: Record<string, ILabour> = {};
		payload[labour.id] = labour;
		store.commit('UpdateLabour', payload);
		
		
	}
	
	public static StartRemoteForId(id: string | null): void {
		
		if (!id) {
			console.error('Project.StartTravelOnId !id', id);
			return;
		}
		
		const agent = Agent.ForLoggedInAgent();
		if (!agent) {
			console.error('Project.StartTravelOnId !agent', agent);
			return;
		}
		if (!agent.id) {
			console.error('Project.StartTravelOnId !agent.id', agent);
			return;
		}
		
		
		const labour = Labour.GetEmpty();
		if (!labour.id) {
			console.error('!labour.id');
			return;
		}
		
		labour.lastModifiedISO8601 = DateTime.utc().toISO();
		labour.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		labour.json.projectId = id;
		labour.json.agentId = agent.id;
		// labour.json.assignmentId = id;
		labour.json.isEnteredThroughTelephoneCompanyAccess = false;
		
		// Find the default type id for billable.
		const allTypes = store.state.Database.labourTypes as Record<string, ILabourType>;
		let billableType = null;
		for (const type of Object.values(allTypes) as ILabourType[]) {
			if (type.json.isBillable === true && type.json.default === true) {
				billableType = type;
			}
		}
		
		if (!billableType) {
			console.error('Project.StartRemoteForId !billableType');
			return;
		}
		if (!billableType.id) {
			console.error('Project.StartRemoteForId !billableType.id');
			return;
		}
		
		labour.json.typeId = billableType.id;
		labour.json.timeMode = 'start-stop-timestamp';
		labour.json.hours = null;
		labour.json.startISO8601 = DateTime.utc().toISO();
		labour.json.endISO8601 = null;
		labour.json.isActive = true;
		labour.json.locationType = 'remote';
		labour.json.isExtra = null;
		labour.json.isBilled = null;
		labour.json.isPaidOut = null;
		
		labour.json.exceptionTypeId = null;
		labour.json.holidayTypeId = null;
		labour.json.nonBillableTypeId = null;
		
		labour.json.notes = null;
		labour.json.bankedPayOutAmount = null;
		
		console.log('start remote', labour);
		
		const payload: Record<string, ILabour> = {};
		payload[labour.id] = labour;
		store.commit('UpdateLabour', payload);
	}
	
	
	public static UpdateStartTimeForId(id: string | null, params: {
		startTimeMode: 'none' | 'morning-first-thing' | 
		'morning-second-thing' | 'afternoon-first-thing' | 
		'afternoon-second-thing' | 'time';
		hasStartISO8601: boolean,
		startISO8601: string | null,
	}): void {
		
		console.debug('Project.UpdateStartTimeForId', id, params);
		
		
		if (!id) {
			console.error('Project.UpdateAgentForId !id');
			return;
		}
		
		//console.debug('#');
		
		const project = Project.ForId(id);
		
		const clone = _.cloneDeep(project) as IProject;
		clone.lastModifiedISO8601 = DateTime.utc().toISO();
		clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		
		clone.json.startTimeMode = params.startTimeMode;
		clone.json.hasStartISO8601 = params.hasStartISO8601;
		clone.json.startISO8601 = params.startISO8601;
		
		//console.debug('##');
		
		// Save changes.
		const payload: Record<string, IProject> = {};
		payload[id] = clone;
		store.commit('UpdateProjects', payload);
	}
	
	public static UpdateForceAssignmentsToUseProjectScheduleForId(id: string | null, flag: boolean): void {
		
		if (!id) {
			console.error('Project.UpdateAgentForId !id');
			return;
		}
		
		const project = Project.ForId(id);
		
		const clone = _.cloneDeep(project) as IProject;
		clone.lastModifiedISO8601 = DateTime.utc().toISO();
		clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		clone.json.forceAssignmentsToUseProjectSchedule = flag;
		
		const payload: Record<string, IProject> = {};
		payload[id] = clone;
		store.commit('UpdateProjects', payload);
		
	}
	
	public static ValidateObject(o: IProject): IProject {
		
		
		
		return o;
	}
	
	public static ChildProjectsOfId(id: string | null): IProject[] {
		
		if (!id) {
			return [];
		}
		
		const projects = store.state.Database.projects as Record<string, IProject>;
		if (!projects || Object.keys(projects).length === 0) {
			return [];
		}
		
		const filtered = _.filter(projects, (value) => {
			return value.json.parentId === id;
		});
		
		return filtered;
	}
	
	public static RecursiveChildProjectsOfId(id: string | null): IProject[] {
		
		if (!id) {
			return [];
		}
		
		const root = Project.ForId(id);
		if (!root) {
			return [];
		}
		
		
		const projects = [root];
		
		const fn = (project: IProject | null) => {
			
			if (!project) {
				return;
			}
			if (!project.id) {
				return;
			}
			
			const children = Project.ChildProjectsOfId(project.id);
			if (children.length === 0) {
				return;
			}
			
			for (const child of children) {
				
				const found = !!_.find(projects, (value) => {
					return value.id === child.id;
				});
				
				if (!found) {
					projects.push(child);
					fn(child);
				}
			}
			
		};
		
		fn(root);
		
		return projects;
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static PermProjectsCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.projects.delete-any') !== -1 ||
			perms.indexOf('crm.projects.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermProjectsCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.projects.push-any') !== -1 ||
			perms.indexOf('crm.projects.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermProjectsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.projects.request-any') !== -1 ||
			perms.indexOf('crm.projects.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermCRMReportProjectsPDF(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.report.projects-pdf') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	public static PermCRMExportProjectsCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.export.projects-csv') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	
	
}

(window as any).DEBUG_Project = Project;

export default {};

