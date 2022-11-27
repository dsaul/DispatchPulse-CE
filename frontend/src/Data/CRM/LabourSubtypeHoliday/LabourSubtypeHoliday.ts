import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import ITracker from '@/Utility/ITracker';
import { guid } from '@/Utility/GlobalTypes';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestLabourSubtypeHolidays } from '@/Data/CRM/LabourSubtypeHoliday/RPCRequestLabourSubtypeHoliday';
import { RPCDeleteLabourSubtypeHolidays } from '@/Data/CRM/LabourSubtypeHoliday/RPCDeleteLabourSubtypeHoliday';
import { RPCPushLabourSubtypeHolidays } from '@/Data/CRM/LabourSubtypeHoliday/RPCPushLabourSubtypeHoliday';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface ILabourSubtypeHoliday extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string;
		icon: string | null;
		description: string | null;
		
		isStaticDate: boolean;
		staticDateMonth: number | null;
		staticDateDay: number | null;
		
		isObservationDay: boolean;
		observationDayStatic: boolean;
		observationDayStaticMonth: number | null;
		observationDayStaticDay: number | null;
		observationDayActivateIfWeekend: boolean;
		
		isFirstMondayInMonthDate: boolean;
		firstMondayMonth: number | null;
		
		isGoodFriday: boolean;
		
		isThirdMondayInMonthDate: boolean;
		thirdMondayMonth: number | null;
		
		isSecondMondayInMonthDate: boolean;
		secondMondayMonth: number | null;
		
		isMondayBeforeDate: boolean;
		mondayBeforeDateMonth: number | null;
		mondayBeforeDateDay: number | null;
		
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}


export class LabourSubtypeHoliday {
	// RPC Methods
	public static RequestLabourSubtypeHolidays = 
		RPCMethod.Register<RPCRequestLabourSubtypeHolidays>(new RPCRequestLabourSubtypeHolidays());
	public static DeleteLabourSubtypeHolidays = 
		RPCMethod.Register<RPCDeleteLabourSubtypeHolidays>(new RPCDeleteLabourSubtypeHolidays());
	public static PushLabourSubtypeHolidays = 
		RPCMethod.Register<RPCPushLabourSubtypeHolidays>(new RPCPushLabourSubtypeHolidays());
	
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
		
		
		const lsh = LabourSubtypeHoliday.ForId(id);
		if (lsh) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(lsh);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = LabourSubtypeHoliday.RequestLabourSubtypeHolidays.Send({
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
					ret._completeRequestPromiseResolve(LabourSubtypeHoliday.ForId(id));
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
	
	
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): ILabourSubtypeHoliday {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): ILabourSubtypeHoliday {
		const id = GenerateID();
		const ret: ILabourSubtypeHoliday = {
			id,
			json: {
				id,
				name: '',
				description: null,
				icon: null,
				
				isStaticDate: false,
				staticDateMonth: null,
				staticDateDay: null,
				
				isObservationDay: false,
				observationDayStatic: false,
				observationDayStaticMonth: null,
				observationDayStaticDay: null,
				observationDayActivateIfWeekend: false,
				
				isFirstMondayInMonthDate: false,
				firstMondayMonth: null,
				
				isGoodFriday: false,
				
				isThirdMondayInMonthDate: false,
				thirdMondayMonth: null,
				
				isSecondMondayInMonthDate: false,
				secondMondayMonth: null,
				
				isMondayBeforeDate: false,
				mondayBeforeDateMonth: null,
				mondayBeforeDateDay: null,
				
				lastModifiedBillingId: null,
				lastModifiedISO8601: DateTime.utc().toISO(),
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): ILabourSubtypeHoliday | null {
		
		if (!id) {
			return null;
		}
		
		const types = store.state.Database.labourSubtypeHolidays as Record<string, ILabourSubtypeHoliday>;
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
	
	
	public static UpdateIds(payload: Record<string, ILabourSubtypeHoliday>): void {
		store.commit('UpdateLabourSubtypeHolidays', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteLabourSubtypeHolidays', ids);
		
	}
	
	public static All(): ILabourSubtypeHoliday[] {
		
		const dblist = store.state.Database.labourSubtypeHolidays as Record<string, ILabourSubtypeHoliday>;
		
		const filtered = _.filter(
			dblist,
			(o: ILabourSubtypeHoliday) => {// eslint-disable-line @typescript-eslint/no-unused-vars
				return true;
			});
		
		const sorted = _.sortBy(filtered, (o: ILabourSubtypeHoliday) => {
			return o.json.name;
		});
		
		return sorted;
		
	}
	
	public static NameForId(id: string | null): string | null {
		
		try {
			if (id == null) {
				return null;
			}
			
			const all = store.state.Database.labourSubtypeHolidays as Record<string, ILabourSubtypeHoliday>;
			
			let dbObj: ILabourSubtypeHoliday = all[id];
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
			
			const all = store.state.Database.labourSubtypeHolidays as Record<string, ILabourSubtypeHoliday>;
			
			let dbObj: ILabourSubtypeHoliday = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}
			
			const icon = dbObj.json.icon;
			//console.log('icon', icon);
			return icon;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return null;
		}
		
	}
	
	
	public static ValidateObject(o: ILabourSubtypeHoliday): ILabourSubtypeHoliday {
		
		
		
		return o;
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static PermLabourSubtypeHolidayCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-subtype-holidays.push-any') !== -1 ||
			perms.indexOf('crm.labour-subtype-holidays.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermLabourSubtypeHolidayCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-subtype-holidays.request-any') !== -1 ||
			perms.indexOf('crm.labour-subtype-holidays.request-company') !== -1 ||
			perms.indexOf('crm.labour-subtype-holidays.request-self') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermLabourSubtypeHolidayCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.labour-subtype-holidays.delete-any') !== -1 ||
			perms.indexOf('crm.labour-subtype-holidays.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
	public static PermCRMExportLabourSubtypeHolidayCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.export.labour-holidays-definitions-csv') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	
	
	
	
}


(window as any).DEBUG_LabourSubtypeHoliday = LabourSubtypeHoliday;

export default {};

