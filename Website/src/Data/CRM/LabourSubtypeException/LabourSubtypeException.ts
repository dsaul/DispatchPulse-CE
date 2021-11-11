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
import { RPCRequestLabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/RPCRequestLabourSubtypeException';
import { RPCDeleteLabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/RPCDeleteLabourSubtypeException';
import { RPCPushLabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/RPCPushLabourSubtypeException';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface ILabourSubtypeException extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string;
		description: string | null;
		icon: string | null;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}

export class LabourSubtypeException {
	// RPC Methods
	public static RequestLabourSubtypeException = 
		RPCMethod.Register<RPCRequestLabourSubtypeException>(new RPCRequestLabourSubtypeException());
	public static DeleteLabourSubtypeException = 
		RPCMethod.Register<RPCDeleteLabourSubtypeException>(new RPCDeleteLabourSubtypeException());
	public static PushLabourSubtypeException = 
		RPCMethod.Register<RPCPushLabourSubtypeException>(new RPCPushLabourSubtypeException());
	
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
		
		
		const lse = LabourSubtypeException.ForId(id);
		if (lse) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(lse);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = LabourSubtypeException.RequestLabourSubtypeException.Send({
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
					ret._completeRequestPromiseResolve(LabourSubtypeException.ForId(id));
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
	
	
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): ILabourSubtypeException {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): ILabourSubtypeException {
		const id = GenerateID();
		const ret: ILabourSubtypeException = {
			id,
			json: {
				id,
				name: '',
				description: null,
				icon: null,
				lastModifiedBillingId: null,
				lastModifiedISO8601: DateTime.utc().toISO(),
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): ILabourSubtypeException | null {
		
		if (!id) {
			return null;
		}
		
		const types = store.state.Database.labourSubtypeException as Record<string, ILabourSubtypeException>;
		if (!types || Object.keys(types).length === 0) {
			return null;
		}
		
		let type = types[id];
		if (!type || !type.json) {
			type = CaseInsensitivePropertyGet(types, id);
		}
		if (!type || !type.json) {
			return null;
		}
		
		return type;
		
	}
	
	public static UpdateIds(payload: Record<string, ILabourSubtypeException>): void {
		store.commit('UpdateLabourSubtypeException', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteLabourSubtypeException', ids);
		
	}
	
	public static All(): ILabourSubtypeException[] {
		
		const dblist = store.state.Database.labourSubtypeException as Record<string, ILabourSubtypeException>;
	
		const filtered = _.filter(
			dblist,
			(o: ILabourSubtypeException) => {// eslint-disable-line @typescript-eslint/no-unused-vars
				return true;
			});
		
		const sorted = _.sortBy(filtered, (o: ILabourSubtypeException) => {
			return o.json.name;
		});
		
		return sorted;
		
	}
	
	public static NameForId(id: string | null): string | null {
		
		try {
			if (id == null) {
				return null;
			}
			
			const all = store.state.Database.labourSubtypeException as Record<string, ILabourSubtypeException>;
			
			let dbObj: ILabourSubtypeException = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			
			return dbObj.json.name;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return null;
		}
		
	}
	
	public static IconForId(id: string | null): string | null {
		
		try {
			if (id == null) {
				return null;
			}
			
			const all = store.state.Database.labourSubtypeException as Record<string, ILabourSubtypeException>;
			
			let dbObj: ILabourSubtypeException = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			
			return dbObj.json.icon;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return null;
		}
		
	}
	
	public static ValidateObject(o: ILabourSubtypeException): ILabourSubtypeException {
		
		
		
		return o;
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static PermLabourSubtypeExceptionCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-subtype-exception.push-any') !== -1 ||
			perms.indexOf('crm.labour-subtype-exception.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermLabourSubtypeExceptionCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-subtype-exception.request-any') !== -1 ||
			perms.indexOf('crm.labour-subtype-exception.request-company') !== -1 ||
			perms.indexOf('crm.labour-subtype-exception.request-self') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermLabourSubtypeExceptionCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-subtype-exception.delete-any') !== -1 ||
			perms.indexOf('crm.labour-subtype-exception.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermCRMExportLabourExceptionCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.export.labour-exception-definitions-csv') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	
}



 

export default {};

