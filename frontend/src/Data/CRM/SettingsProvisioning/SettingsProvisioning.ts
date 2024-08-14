import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import { guid } from '@/Utility/GlobalTypes';
import ITracker from '@/Utility/ITracker';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { RPCRequestSettingsProvisioning } from '@/Data/CRM/SettingsProvisioning/RPCRequestSettingsProvisioning';
import { RPCDeleteSettingsProvisioning } from '@/Data/CRM/SettingsProvisioning/RPCDeleteSettingsProvisioning';
import { RPCMethod } from '@/RPC/RPCMethod';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface ISettingsProvisioning extends ICRMTable {
	json: {
		key: string;
		value: string;
	};
}

export class SettingsProvisioning {
	// RPC Methods
	public static RequestSettingsProvisioning = 
		RPCMethod.Register<RPCRequestSettingsProvisioning>(new RPCRequestSettingsProvisioning());
	public static DeleteSettingsProvisioning = 
		RPCMethod.Register<RPCDeleteSettingsProvisioning>(new RPCDeleteSettingsProvisioning());
	
	
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
		
		
		const sp = SettingsProvisioning.ForId(id);
		if (sp) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(sp);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = SettingsProvisioning.RequestSettingsProvisioning.Send({
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
					ret._completeRequestPromiseResolve(SettingsProvisioning.ForId(id));
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
	
	public static GetMerged(mergeValues: Record<string, any>): ISettingsProvisioning {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): ISettingsProvisioning {
		const id = GenerateID();
		const ret: ISettingsProvisioning = {
			id,
			json: {
				key: '',
				value: '',
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): ISettingsProvisioning | null {
		
		if (!id) {
			return null;
		}
		
		const statuses = store.state.Database.settingsProvisioning as Record<string, ISettingsProvisioning>;
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
	
	public static UpdateIds(payload: Record<string, ISettingsProvisioning>): void {
		store.commit('UpdateSettingsProvisioning', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteSettingsProvisioning', ids);
		
	}
	
	public static ValidateObject(o: ISettingsProvisioning): ISettingsProvisioning {
		
		
		
		return o;
	}
	
}



 

export default {};

