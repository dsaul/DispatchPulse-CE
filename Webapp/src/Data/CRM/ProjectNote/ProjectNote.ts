import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { guid } from '@/Utility/GlobalTypes';
import ITracker from '@/Utility/ITracker';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestProjectNotes } from '@/Data/CRM/ProjectNote/RPCRequestProjectNotes';
import { RPCDeleteProjectNotes } from '@/Data/CRM/ProjectNote/RPCDeleteProjectNotes';
import { RPCPushProjectNotes } from '@/Data/CRM/ProjectNote/RPCPushProjectNotes';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IProjectNoteStyledText } from '../ProjectNoteStyledText/ProjectNoteStyledText';
import { IProjectNoteCheckbox } from '../ProjectNoteCheckbox/ProjectNoteCheckbox';
import { IProjectNoteImage } from '../ProjectNoteImage/ProjectNoteImage';
import { IProjectNoteVideo } from '../ProjectNoteVideo/ProjectNoteVideo';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IProjectNote extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
		originalBillingId: string | null;
		originalISO8601: string | null;
		
		projectId: string | null;
		assignmentId: string | null;
		contentType: 'styled-text' | 'checkbox' | 'image' | 'video' | null;
		content: IProjectNoteStyledText | IProjectNoteCheckbox | IProjectNoteImage | IProjectNoteVideo | null;
		internalOnly: boolean;
		resolved: boolean;
		noLongerRelevant: boolean;
	};
}

export class ProjectNote {
	// RPC Methods
	public static RequestProjectNotes = RPCMethod.Register<RPCRequestProjectNotes>(new RPCRequestProjectNotes());
	public static DeleteProjectNotes = RPCMethod.Register<RPCDeleteProjectNotes>(new RPCDeleteProjectNotes());
	public static PushProjectNotes = RPCMethod.Register<RPCPushProjectNotes>(new RPCPushProjectNotes());
	
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
		
		
		const projectNote = ProjectNote.ForId(id);
		if (projectNote) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(projectNote);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = ProjectNote.RequestProjectNotes.Send({
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
					ret._completeRequestPromiseResolve(ProjectNote.ForId(id));
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
	
	
	
	public static ValidateObject(o: IProjectNote) : IProjectNote {
		
		if (!o.lastModifiedISO8601 || IsNullOrEmpty(o.lastModifiedISO8601)) {
			if (o.json.lastModifiedISO8601 && !IsNullOrEmpty(o.json.lastModifiedISO8601)) {// tslint:disable-line
				o.lastModifiedISO8601 = o.json.lastModifiedISO8601;// tslint:disable-line
			} else {
				o.lastModifiedISO8601 = DateTime.utc().toISO();
			}
		}
		
		if (!o.json.lastModifiedISO8601 || IsNullOrEmpty(o.json.lastModifiedISO8601)) {// tslint:disable-line
			if (o.lastModifiedISO8601 && !IsNullOrEmpty(o.lastModifiedISO8601)) {
				o.json.lastModifiedISO8601 = o.lastModifiedISO8601;// tslint:disable-line
			} else {
				o.json.lastModifiedISO8601 = DateTime.utc().toISO();// tslint:disable-line
			}
		}
		
		if (!o.json.originalISO8601 || IsNullOrEmpty(o.json.originalISO8601)) {
			if (o.json.lastModifiedISO8601 && !IsNullOrEmpty(o.json.lastModifiedISO8601)) {// tslint:disable-line
				o.json.originalISO8601 = o.json.lastModifiedISO8601;// tslint:disable-line
			}
		}
		
		if (!o.json.originalBillingId || IsNullOrEmpty(o.json.originalBillingId)) {
			if (o.json.lastModifiedBillingId && !IsNullOrEmpty(o.json.lastModifiedBillingId)) {
				o.json.originalBillingId = o.json.lastModifiedBillingId;
			}
		}
		
		if (o.json.assignmentId === undefined) {
			o.json.assignmentId = null;
		}
		
		if (o.json.internalOnly === undefined) {
			o.json.internalOnly = false;
		}
		
		if (o.json.resolved === undefined) {
			o.json.resolved = false;
		}
		
		if (o.json.noLongerRelevant === undefined) {
			o.json.noLongerRelevant = false;
		}
		
		
		return o;
	}
	
	
	public static GetMerged(mergeValues: Record<string, any>): IProjectNote {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IProjectNote {
		const id = GenerateID();
		const ret: IProjectNote = {
			id,
			json: {
				id,
				lastModifiedISO8601: DateTime.utc().toISO(),
				lastModifiedBillingId: null,
				originalISO8601: null,
				originalBillingId: null,
				projectId: null,
				assignmentId: null,
				contentType: null,
				content: null,
				internalOnly: false,
				resolved: false,
				noLongerRelevant: false,
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteProjectNotes', ids);
		
	}
	
	public static UpdateIds(payload: Record<string, IProjectNote>): void {
		store.commit('UpdateProjectNotes', payload);
	}
	
	public static ForId(id: string | null): IProjectNote | null {
		
		if (!id) {
			return null;
		}
		
		const statuses = store.state.Database.projectNotes as Record<string, IProjectNote>;
		if (!statuses || Object.keys(statuses).length === 0) {
			return null;
		}
		
		let status = statuses[id];
		if (!status || !status.json) {
			status = CaseInsensitivePropertyGet(statuses, id);
		}
		if (!status || !status.json) {
			return null;
		}
		
		return status;
		
	}
	
	public static ForProjectIds(projectIds: string[]): IProjectNote[] {
		
		const all: Record<string, IProjectNote> = store.state.Database.projectNotes;
		if (!all) {
			return [];
		}
		
		
		const filtered = _.filter(all, (o: IProjectNote) => {
			const projectId = o.json.projectId;
			
			return !!_.find(projectIds, (suppliedId: string) => suppliedId === projectId);
		});
		
		
		
		
		
		return filtered;
		
	}
	
	public static ForAssignmentIds(assignmentIds: string[]): IProjectNote[] {
		
		const all: Record<string, IProjectNote> = store.state.Database.projectNotes;
		if (!all) {
			return [];
		}
		
		
		const filtered = _.filter(all, (o: IProjectNote) => {
			const assignmentId = o.json.assignmentId;
			
			return !!_.find(assignmentIds, (suppliedId: string) => suppliedId === assignmentId);
		});
		
		
		return filtered;
		
	}
	
	
	
	
	
	
	
	
	
	
	public static PermProjectNotesCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.project-notes.push-any') !== -1 ||
			perms.indexOf('crm.project-notes.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermProjectNotesCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.project-notes.request-any') !== -1 ||
			perms.indexOf('crm.project-notes.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermProjectNotesCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.project-notes.delete-any') !== -1 ||
			perms.indexOf('crm.project-notes.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
}


 

export default {};

