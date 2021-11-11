import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import { guid } from '@/Utility/GlobalTypes';
import ITracker from '@/Utility/ITracker';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestAssignmentStatus } from '@/Data/CRM/AssignmentStatus/RPCRequestAssignmentStatus';
import { RPCDeleteAssignmentStatus } from '@/Data/CRM/AssignmentStatus/RPCDeleteAssignmentStatus';
import { RPCPushAssignmentStatus } from '@/Data/CRM/AssignmentStatus/RPCPushAssignmentStatus';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IAssignmentStatus extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string;
		isOpen: boolean;
		isReOpened: boolean;
		isAssigned: boolean;
		isWaitingOnClient: boolean;
		isWaitingOnVendor: boolean;
		isBillable: boolean;
		isBillableReview: boolean;
		isToBeScheduled: boolean;
		isNonBillable: boolean;
		isInProgress: boolean;
		isScheduled: boolean;
		isDefault: boolean;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}

export class AssignmentStatus {
	
	// RPC Methods
	public static RequestAssignmentStatus = 
		RPCMethod.Register<RPCRequestAssignmentStatus>(new RPCRequestAssignmentStatus());
	public static DeleteAssignmentStatus = 
		RPCMethod.Register<RPCDeleteAssignmentStatus>(new RPCDeleteAssignmentStatus());
	public static PushAssignmentStatus = 
		RPCMethod.Register<RPCPushAssignmentStatus>(new RPCPushAssignmentStatus());
	
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
		
		
		const assignmentStatus = AssignmentStatus.ForId(id);
		if (assignmentStatus) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(assignmentStatus);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = AssignmentStatus.RequestAssignmentStatus.Send({
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
					ret._completeRequestPromiseResolve(AssignmentStatus.ForId(id));
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
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): IAssignmentStatus {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IAssignmentStatus {
		const id = GenerateID();
		const ret: IAssignmentStatus = {
			id,
			json: {
				id,
				name: '',
				isOpen: false,
				isReOpened: false,
				isAssigned: false,
				isWaitingOnClient: false,
				isWaitingOnVendor: false,
				isBillable: false,
				isBillableReview: false,
				isDefault: false,
				isToBeScheduled: false,
				isInProgress: false,
				isNonBillable: false,
				isScheduled: false,
				lastModifiedBillingId: null,
				lastModifiedISO8601: DateTime.utc().toISO(),
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): IAssignmentStatus | null {
		
		if (!id) {
			return null;
		}
		
		const statuses = store.state.Database.assignmentStatus as Record<string, IAssignmentStatus>;
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
	
	public static UpdateIds(payload: Record<string, IAssignmentStatus>): void {
		store.commit('UpdateAssignmentStatus', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteAssignmentStatus', ids);
		
	}
	
	public static ValidateObject(o: IAssignmentStatus): IAssignmentStatus {
		
		
		
		return o;
	}
	
	
	
	
	
	
	public static PermAssignmentStatusCanPush(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.assignments-status.push-any') !== -1 ||
			perms.indexOf('crm.assignments-status.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermAssignmentStatusCanRequest(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.assignments-status.request-any') !== -1 ||
			perms.indexOf('crm.assignments-status.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermAssignmentStatusCanDelete(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.assignments-status.delete-any') !== -1 ||
			perms.indexOf('crm.assignments-status.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
	public static PermCRMExportAssignmentStatusCSV(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.export.assignment-status-definitions-csv') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
}




 

export default {};

