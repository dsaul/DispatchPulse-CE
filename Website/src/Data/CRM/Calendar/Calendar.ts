import _ from 'lodash';
import store from '@/plugins/store/store';
import { guid } from '@/Utility/GlobalTypes';
import { RPCMethod } from '@/RPC/RPCMethod';
import GenerateID from '@/Utility/GenerateID';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ITracker from '@/Utility/ITracker';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { DateTime } from 'luxon';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestCalendars } from '@/Data/CRM/Calendar/RPCRequestCalendars';
import { RPCDeleteCalendars } from '@/Data/CRM/Calendar/RPCDeleteCalendars';
import { RPCPushCalendars } from '@/Data/CRM/Calendar/RPCPushCalendars';
import { RPCPerformRetrieveCalendar } from '@/Data/CRM/Calendar/RPCPerformRetrieveCalendar';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

// tslint:disable-next-line
/// <reference path="../../../shims-cal-parser.d.ts" />

export interface ICalendar extends ICRMTable {
	json: {
		name: string | null;
		iCalFileURI: string | null;
		lastModifiedBillingId: string | null;
		iCalFileLastRetrievedISO8601: string | null;
		iCalFileLastData: string | null;
		occurancesRoughlyAroundThisMonth: Array<{
			startISO8601: string;
			endISO8601: string;
			durationTotalSeconds: number;
			description: string;
			phoneNumber: string;
		}>;
	};
}


export class Calendar {
	// RPC Methods
	public static RequestCalendars = RPCMethod.Register<RPCRequestCalendars>(new RPCRequestCalendars());
	public static DeleteCalendars = RPCMethod.Register<RPCDeleteCalendars>(new RPCDeleteCalendars());
	public static PushCalendars = RPCMethod.Register<RPCPushCalendars>(new RPCPushCalendars());
	public static PerformRetrieveCalendar = 
		RPCMethod.Register<RPCPerformRetrieveCalendar>(new RPCPerformRetrieveCalendar());
	
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
		
		
		const cal = Calendar.ForId(id);
		if (cal) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(cal);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = Calendar.RequestCalendars.Send({
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
					ret._completeRequestPromiseResolve(Calendar.ForId(id));
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
	
	
	
	
	
	
	public static ValidateObject(o: ICalendar): ICalendar {
		
		
		
		return o;
	}
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): ICalendar {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): ICalendar {
		const id = GenerateID();
		const ret: ICalendar = {
			id,
			json: {
				name: null,
				iCalFileURI: null,
				lastModifiedBillingId: null,
				iCalFileLastRetrievedISO8601: null,
				iCalFileLastData: null,
				occurancesRoughlyAroundThisMonth: [],
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): ICalendar | null {
		if (!id) {
			return null;
		}
		
		const cals = store.state.Database.calendars as { [id: string]: ICalendar; };
		if (!cals || Object.keys(cals).length === 0) {
			return null;
		}
		
		let cal = cals[id];
		if (!cal) {
			cal = CaseInsensitivePropertyGet(cals, id);
		}
		if (!cal) {
			return null;
		}
		
		return cal;
	}
	
	public static UpdateIds(payload: { [id: string]: ICalendar; }): void {
		store.commit('UpdateCalendars', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteCalendars', ids);
		
	}
	
	public static PermCalendarsCanRequest(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.calendars.request-any') !== -1 ||
			perms.indexOf('crm.calendars.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
	}
	
	public static PermCalendarsCanPush(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.calendars.push-any') !== -1 ||
			perms.indexOf('crm.calendars.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermCalendarsCanDelete(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.calendars.delete-any') !== -1 ||
			perms.indexOf('crm.calendars.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static NameForId(id: string | null): string | null {
		const cal = Calendar.ForId(id);
		if (!cal || !cal.json) {
			return null;
		}
		
		return cal.json.name || null;
	}
	
	
	
	
}

(window as any).DEBUG_Calendar = Calendar;

export default {};
