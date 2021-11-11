import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import { BillingSessions, IBillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import { guid } from '@/Utility/GlobalTypes';
import ITracker from '@/Utility/ITracker';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestAgents } from '@/Data/CRM/Agent/RPCRequestAgents';
import { RPCDeleteAgents } from '@/Data/CRM/Agent/RPCDeleteAgents';
import { RPCPushAgents } from '@/Data/CRM/Agent/RPCPushAgents';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';

export interface IAgent extends ICRMTable {
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
		name: string | null;
		title: string | null;
		employmentStatusId: string | null;
		hourlyWage: number | null;
		notificationSMSNumber: string | null;
		phoneId: string | null;
		phonePasscode: string | null;
	};
}

export class Agent {
	// RPC Methods
	public static RequestAgents = RPCMethod.Register<RPCRequestAgents>(new RPCRequestAgents());
	public static DeleteAgents = RPCMethod.Register<RPCDeleteAgents>(new RPCDeleteAgents());
	public static PushAgents = RPCMethod.Register<RPCPushAgents>(new RPCPushAgents());
	
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
		
		
		const agent = Agent.ForId(id);
		if (agent) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(agent);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = Agent.RequestAgents.Send({
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
					ret._completeRequestPromiseResolve(Agent.ForId(id));
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
	
	
	
	
	
	
	
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): IAgent {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IAgent {
		const id = GenerateID();
		const ret: IAgent = {
			id,
			json: {
				id,
				name: '',
				lastModifiedISO8601: DateTime.utc().toISO(),
				lastModifiedBillingId: null,
				title: '',
				employmentStatusId: null,
				hourlyWage: null,
				notificationSMSNumber: null,
				phoneId: null,
				phonePasscode: null,
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): IAgent | null {
		if (!id) {
			return null;
		}
		
		const agents = store.state.Database.agents as Record<string, IAgent>;
		if (!agents || Object.keys(agents).length === 0) {
			return null;
		}
		
		let agent = agents[id];
		if (!agent) {
			agent = CaseInsensitivePropertyGet(agents, id);
		}
		if (!agent) {
			return null;
		}
		
		return agent;
	}
	
	public static UpdateIds(payload: Record<string, IAgent>): void {
		store.commit('UpdateAgents', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteAgents', ids);
		
	}
	
	public static FetchLoggedInAgent(): IRoundTripRequest | null {
		
		const id = Agent.LoggedInAgentId();
		if (!id) {
			return null;
		}
		
		return Agent.FetchForId(id);
	}
	
	
	public static LoggedInAgentId(): string | null {
		const sessionId = BillingSessions.CurrentSessionId();
		if (!sessionId || IsNullOrEmpty(sessionId)) {
			return null;
		}
		
		
		//console.debug('sessionId', sessionId);
		
		const sessions: Record<string, IBillingSessions> = store.state.Database.billingSessions;
		if (!sessions) {
			return null;
		}
		
		const session = sessions[sessionId];
		if (!session) {
			return null;
		}
		
		//console.debug('session', session);
		
		const billingContactId = session.contactId;
		if (!billingContactId || IsNullOrEmpty(billingContactId)) {
			return null;
		}
		
		const billingContacts: Record<string, IBillingContacts> = store.state.Database.billingContacts;
		if (!billingContacts) {
			return null;
		}
		
		const billingContact = billingContacts[billingContactId];
		if (!billingContact) {
			return null;
		}
		
		//console.debug('billingContact', billingContact);
		
		const applicationData = billingContact.applicationData;
		if (!applicationData) {
			return null;
		}
		
		const agentId = applicationData.dispatchPulseAgentId;
		if (!agentId || IsNullOrEmpty(agentId)) {
			return null;
		}
		
		//console.debug('agentId', agentId);
		
		return agentId;
	}
	
	public static DescriptionForAgentList(agents: IAgent[], addPeriod = true): string {
		
		
		let ret = '';
			
		for (let i = 0; i < agents.length; i++) {
		
			if (i !== 0) {
				ret += ', ';
			}
			if (i !== 0 && i === agents.length - 1) {
				ret += ' and ';
			}
			
			ret += (IsNullOrEmpty(agents[i].json.name) ? 'No Name' : agents[i].json.name);
			
		}
		
		if (addPeriod) {
			ret += '.';
		}
		
		return ret;
		
	}
	
	public static ForAssignmentId(assignmentId: string | null): IAgent[] {
		
		const assignment = Assignment.ForId(assignmentId);
		if (!assignment) {
			return [];
		}
		
		if (!assignment ||
			!assignment.json ||
			!assignment.json.agentIds) {
			return [];
		}
		
		const ret: IAgent[] = [];
		
		for (const agentId of assignment.json.agentIds) {
			if (!agentId) {
				continue;
			}
			
			const agent = Agent.ForId(agentId);
			if (!agent) {
				continue;
			}
			
			ret.push(agent);
		}
		
		return ret;
		
	}
	
	public static ForLoggedInAgent(): IAgent | null {
		
		const agentId = Agent.LoggedInAgentId();
		
		//console.debug('ForLoggedInAgent', agentId);
		
		
		
		if (!agentId || IsNullOrEmpty(agentId)) {
			//console.debug('!agentId || IsNullOrEmpty(agentId)');
			return null;
		}
		
		return Agent.ForId(agentId);
	}
	
	public static NameForId(id: string | null): string | null {
		
		const agent = Agent.ForId(id);
		if (!agent) {
			return null;
		}
		
		return agent.json.name;
	}
	
	
	public static ValidateObject(o: IAgent): IAgent {
		
		
		
		return o;
	}
	
	
	
	
	
	
	
	
	
	
	public static PermAgentDisplayOwn(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf('crm.agents.display-own') !== -1;
		return ret;
	}
	
	public static PermAgentsCanRequest(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.agents.request-any') !== -1 ||
			perms.indexOf('crm.agents.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
	}
	
	public static PermAgentsCanPush(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.agents.push-any') !== -1 ||
			perms.indexOf('crm.agents.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermAgentsCanDelete(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.agents.delete-any') !== -1 ||
			perms.indexOf('crm.agents.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
	public static PermCRMExportAgentsCSV(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.export.agents-csv') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	
	
	
	
}

(window as any).DEBUG_AGENT = Agent;

export default {};

