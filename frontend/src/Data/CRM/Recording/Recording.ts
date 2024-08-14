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
import { RPCRequestRecordings } from '@/Data/CRM/Recording/RPCRequestRecordings';
import { RPCDeleteRecordings } from '@/Data/CRM/Recording/RPCDeleteRecordings';
import { RPCPushRecordings } from '@/Data/CRM/Recording/RPCPushRecordings';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRecording extends ICRMTable {
	json: {
		name: string | null;
		
		tmpMP3Path: string | null;
		tmpPCMPath: string | null;
		tmpWAVPath: string | null;
		
		S3Host: string | null;
		S3Bucket: string | null;
		
		S3MP3Key: string | null;
		S3MP3HTTPSURI: string | null;
		S3CMDMP3URI: string | null;
		
		S3WAVKey: string | null;
		S3WAVHTTPSURI: string | null;
		S3CMDWAVURI: string | null;
		
		S3PCMKey: string | null;
		S3PCMHTTPSURI: string | null;
		S3CMDPCMURI: string | null;
	};
}

export class Recording {
	// RPC Methods
	public static RequestRecordings = RPCMethod.Register<RPCRequestRecordings>(new RPCRequestRecordings());
	public static DeleteRecordings = RPCMethod.Register<RPCDeleteRecordings>(new RPCDeleteRecordings());
	public static PushRecordings = RPCMethod.Register<RPCPushRecordings>(new RPCPushRecordings());
	
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
		
		
		const recording = Recording.ForId(id);
		if (recording) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(recording);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = Recording.RequestRecordings.Send({
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
					ret._completeRequestPromiseResolve(Recording.ForId(id));
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
	
	public static GetMerged(mergeValues: Record<string, any>): IRecording {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IRecording {
		const id = GenerateID();
		const ret: IRecording = {
			id,
			json: {
				name: null,
				tmpMP3Path: null,
				tmpPCMPath: null,
				tmpWAVPath: null,
				
				S3Host: null,
				S3Bucket: null,
				
				S3MP3Key: null,
				S3MP3HTTPSURI: null,
				S3CMDMP3URI: null,
				
				S3WAVKey: null,
				S3WAVHTTPSURI: null,
				S3CMDWAVURI: null,
				
				S3PCMKey: null,
				S3PCMHTTPSURI: null,
				S3CMDPCMURI: null,
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteRecordings', ids);
		
	}
	
	public static UpdateIds(payload: { [id: string]: IRecording; }): void {
		store.commit('UpdateRecordings', payload);
	}
	
	public static ForId(id: string | null): IRecording | null {
		
		if (!id) {
			return null;
		}
		
		const statuses = store.state.Database.recordings as { [id: string]: IRecording; };
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
	
	
	
	
	public static ValidateObject(o: IRecording): IRecording {
		
		
		
		return o;
	}
	
	
	
	
	public static NameForId(id: guid): string | null {
		const recording = Recording.ForId(id);
		if (!recording || !recording.json) {
			return null;
		}
		
		return recording.json.name || null;
	}
	
	
	
	
	
	public static PermRecordingsCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.recordings.push-any') !== -1 ||
			perms.indexOf('crm.recordings.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermRecordingsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.recordings.request-any') !== -1 ||
			perms.indexOf('crm.recordings.request-company') !== -1 ||
			perms.indexOf('crm.recordings.request-self') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermRecordingsCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.recordings.delete-any') !== -1 ||
			perms.indexOf('crm.recordings.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
	
}



 

export default {};

