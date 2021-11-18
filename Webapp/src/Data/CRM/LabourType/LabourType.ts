import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import { guid } from '@/Utility/GlobalTypes';
import ITracker from '@/Utility/ITracker';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestLabourTypes } from '@/Data/CRM/LabourType/RPCRequestLabourTypes';
import { RPCDeleteLabourTypes } from '@/Data/CRM/LabourType/RPCDeleteLabourTypes';
import { RPCPushLabourTypes } from '@/Data/CRM/LabourType/RPCPushLabourTypes';
import { RPCMethod } from '@/RPC/RPCMethod';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface ILabourType extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string | null;
		icon: string | null;
		description: string | null;
		
		default: boolean;
		
		isBillable: boolean;
		isHoliday: boolean;
		isNonBillable: boolean;
		isException: boolean;
		isPayOutBanked: boolean;
		
		lastModifiedBillingId: string | null;
	};
}

export class LabourType {
	// RPC Methods
	public static RequestLabourTypes = RPCMethod.Register<RPCRequestLabourTypes>(new RPCRequestLabourTypes());
	public static DeleteLabourTypes = RPCMethod.Register<RPCDeleteLabourTypes>(new RPCDeleteLabourTypes());
	public static PushLabourTypes = RPCMethod.Register<RPCPushLabourTypes>(new RPCPushLabourTypes());
	
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
		
		
		const type = LabourType.ForId(id);
		if (type) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(type);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = LabourType.RequestLabourTypes.Send({
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
					ret._completeRequestPromiseResolve(LabourType.ForId(id));
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
	
	
	
	
	
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): ILabourType {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): ILabourType {
		const id = GenerateID();
		const ret: ILabourType = {
			id,
			json: {
				id,
				name: null,
				icon: null,
				description: null,
				default: false,
				isBillable: false,
				isHoliday: false,
				isNonBillable: false,
				isException: false,
				isPayOutBanked: false,
				lastModifiedBillingId: null,
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}

	public static DefaultBillableTypeId(): string | null {
		
		const types = store.state.Database.labourTypes as Record<string, ILabourType>;
		if (!types || Object.keys(types).length === 0) {
			return null;
		}

		const fBillable = _.filter(
			types,
			(o: ILabourType) => {
				return o.json.isBillable === true;
			},
		);

		if (fBillable.length === 0) {
			return null;
		} else if (fBillable.length === 1) {
			return fBillable[0].id  || null;
		} else if (fBillable.length > 1) { 
			// If filtered is greater than 1, lets see if 
			// one of those has the default tag chosen.
			const fDefault = _.filter(
				fBillable,
				(o: ILabourType) => {
					return o.json.default === true;
				},
			);

			if (fDefault.length === 0) {
				// Return the first in billable if none have an entry for default.
				return fBillable[0].id  || null;
			} else {
				// Just return the first default in case multiple are found.
				return fDefault[0].id || null;
			}
		}

		return null;
	}
	
	public static ForId(id: string | null): ILabourType | null {
		
		if (!id) {
			return null;
		}
		
		const types = store.state.Database.labourTypes as Record<string, ILabourType>;
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
	
	public static UpdateIds(payload: Record<string, ILabourType>): void {
		store.commit('UpdateLabourTypes', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteLabourTypes', ids);
		
	}
	
	public static All(): ILabourType[] {
		
		const dblist = store.state.Database.labourTypes as Record<string, ILabourType>;
		
		const filtered = _.filter(
			dblist,
			(o: ILabourType) => { // eslint-disable-line @typescript-eslint/no-unused-vars
				return true;
			});
		
		const sorted = _.sortBy(filtered, (o: ILabourType) => {
			return o.json.name;
		});
		
		return sorted;
	}
	
	public static NameForId(id: string | null): string | null {
		
		try {
			if (id == null) {
				return null;
			}
			
			const all = store.state.Database.labourTypes as Record<string, ILabourType>;
			
			let dbObj: ILabourType = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			
			return dbObj.json.name;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return null;
		}
		
	}
	
	public static IsPayOutBankedForId(id: string | null): boolean {
		
		try {
			if (id == null) {
				return false;
			}
			const all = store.state.Database.labourTypes as Record<string, ILabourType>;
			
			let dbObj: ILabourType = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			return dbObj.json.isPayOutBanked;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return false;
		}
		
	}
	
	public static IsNonBillableForId(id: string | null): boolean {
		
		try {
			if (id == null) {
				return false;
			}
			
			const all = store.state.Database.labourTypes as Record<string, ILabourType>;
			
			let dbObj: ILabourType = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			
			return dbObj.json.isNonBillable;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return false;
		}
		
	}
	
	public static IsHolidayForId(id: string | null): boolean {
		
		try {
			if (id == null) {
				return false;
			}
			
			const all = store.state.Database.labourTypes as Record<string, ILabourType>;
			
			let dbObj: ILabourType = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			return dbObj.json.isHoliday;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return false;
		}
		
	}
	
	public static IsExceptionForId(id: string | null): boolean {
		
		try {
			if (id == null) {
				return false;
			}
			
			const all = store.state.Database.labourTypes as Record<string, ILabourType>;
			
			let dbObj: ILabourType = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			return dbObj.json.isException;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return false;
		}
		
	}
	
	public static IsBillableForId(id: string | null): boolean {
		
		try {
			if (id == null) {
				return false;
			}
			
			const all = store.state.Database.labourTypes as Record<string, ILabourType>;
			
			let dbObj: ILabourType = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			return dbObj.json.isBillable;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return false;
		}
		
	}
	
	public static IconForId(id: string | null): string | null {
		
		try {
			if (id == null) {
				return null;
			}
			
			const all = store.state.Database.labourTypes as Record<string, ILabourType>;
			
			let dbObj: ILabourType = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			
			return dbObj.json.icon;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return null;
		}
		
	}
	
	public static ValidateObject(o: ILabourType): ILabourType {
		
		
		
		return o;
	}
	
	
	
	public static PermLabourTypesCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-types.request-any') !== -1 ||
			perms.indexOf('crm.labour-types.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermLabourTypesCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-types.push-any') !== -1 ||
			perms.indexOf('crm.labour-types.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermLabourTypesCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-types.delete-any') !== -1 ||
			perms.indexOf('crm.labour-types.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
}


 

export default {};

