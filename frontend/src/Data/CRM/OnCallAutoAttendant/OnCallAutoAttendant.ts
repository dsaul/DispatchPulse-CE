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
import { RPCRequestOnCallAutoAttendants } from '@/Data/CRM/OnCallAutoAttendant/RPCRequestOnCallAutoAttendants';
import { RPCDeleteOnCallAutoAttendants } from '@/Data/CRM/OnCallAutoAttendant/RPCDeleteOnCallAutoAttendants';
import { RPCPushOnCallAutoAttendants } from '@/Data/CRM/OnCallAutoAttendant/RPCPushOnCallAutoAttendants';
import '@/Data/CRM/MessagePrompt/MessagePrompt';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { IMessagePrompt } from '@/Data/CRM/MessagePrompt/MessagePrompt';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IOnCallAutoAttendant extends ICRMTable {
	json: {
		lastModifiedBillingId: string | null;
		name: string | null;
		agentOnCallPriorityCalendars: Array<guid | null>;
		noAgentResponseNotificationNumber: string | null;
		noAgentResponseNotificationEMail: string | null;
		markedHandledNotificationEMail: string | null;
		failoverNumber: string | null;
		recordings: {
			intro: IMessagePrompt,
			askForCallbackNumber: IMessagePrompt,
			askForMessage: IMessagePrompt,
			thankYouAfter: IMessagePrompt,
		},
		callAttemptsToEachCalendarBeforeGivingUp: number | null;
		minutesBetweenCallAttempts: number | null;
	};
}


export class OnCallAutoAttendant {
	// RPC Methods
	public static RequestOnCallAutoAttendants = 
		RPCMethod.Register<RPCRequestOnCallAutoAttendants>(new RPCRequestOnCallAutoAttendants());
	public static DeleteOnCallAutoAttendants = 
		RPCMethod.Register<RPCDeleteOnCallAutoAttendants>(new RPCDeleteOnCallAutoAttendants());
	public static PushOnCallAutoAttendants = 
		RPCMethod.Register<RPCPushOnCallAutoAttendants>(new RPCPushOnCallAutoAttendants());
	
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
		
		
		const aa = OnCallAutoAttendant.ForId(id);
		if (aa) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(aa);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = OnCallAutoAttendant.RequestOnCallAutoAttendants.Send({
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
					ret._completeRequestPromiseResolve(OnCallAutoAttendant.ForId(id));
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
	
	
	
	
	
	
	public static ValidateObject(o: IOnCallAutoAttendant): IOnCallAutoAttendant {
		
		
		
		return o;
	}
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): IOnCallAutoAttendant {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IOnCallAutoAttendant {
		const id = GenerateID();
		const ret: IOnCallAutoAttendant = {
			id,
			json: {
				name: null,
				agentOnCallPriorityCalendars: [],
				noAgentResponseNotificationNumber: null,
				noAgentResponseNotificationEMail: null,
				markedHandledNotificationEMail: null,
				failoverNumber: '',
				recordings: {
					intro: {
						type: 'polly',
						text: 'Thanks for calling, you\'ve reached our after hours on call service.\n\n\n\n If you wish to leave a message for our on call representative, please press 1.',
						recordingId: null,
					},
					askForCallbackNumber: {
						type: 'polly',
						text: 'Please provide a callback number that we can reach you at. Followed by the pound key.',
						recordingId: null,
					},
					askForMessage: {
						type: 'polly',
						text: 'At the tone, leave a message detailing the nature of the request, as well as any other contact numbers.',
						recordingId: null,
					},
					thankYouAfter: {
						type: 'polly',
						text: 'Thank you, we will dispatch this to the on call representative right away.',
						recordingId: null,
					},
				},
				lastModifiedBillingId: null,
				callAttemptsToEachCalendarBeforeGivingUp: 2,
				minutesBetweenCallAttempts: 5,
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): IOnCallAutoAttendant | null {
		if (!id) {
			return null;
		}
		
		const autoAttendants = store.state.Database.onCallAutoAttendants as { [id: string]: IOnCallAutoAttendant; };
		if (!autoAttendants || Object.keys(autoAttendants).length === 0) {
			return null;
		}
		
		let aa = autoAttendants[id];
		if (!aa) {
			aa = CaseInsensitivePropertyGet(autoAttendants, id);
		}
		if (!aa) {
			return null;
		}
		
		return aa;
	}
	
	public static UpdateIds(payload: { [id: string]: IOnCallAutoAttendant; }): void {
		store.commit('UpdateOnCallAutoAttendants', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteOnCallAutoAttendants', ids);
		
	}
	
	public static PermOnCallAutoAttendantsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.on-call-auto-attendants.request-any') !== -1 ||
			perms.indexOf('crm.on-call-auto-attendants.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
	}
	
	public static PermOnCallAutoAttendantsCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.on-call-auto-attendants.push-any') !== -1 ||
			perms.indexOf('crm.on-call-auto-attendants.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermOnCallAutoAttendantsCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.on-call-auto-attendants.delete-any') !== -1 ||
			perms.indexOf('crm.on-call-auto-attendants.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static NameForId(id: string | null): string | null {
		const aa = OnCallAutoAttendant.ForId(id);
		if (!aa || !aa.json) {
			return null;
		}
		
		return aa.json.name || null;
	}
	
	
}

(window as any).DEBUG_OnCallAutoAttendant = OnCallAutoAttendant;

export default {};
