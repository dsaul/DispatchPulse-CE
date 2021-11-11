import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { BillingSessions, IBillingSessions } from '../BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestBillingContactsForCurrentSession } from '@/Data/Billing/BillingContacts/RPCRequestBillingContactsForCurrentSession';
import { RPCPerformChangeSessionPassword } from '@/Data/Billing/BillingContacts/RPCPerformChangeSessionPassword';
import { RPCPerformDeleteBillingContact } from '@/Data/Billing/BillingContacts/RPCPerformDeleteBillingContact';
import { RPCPerformRegisterAdditionalUsers } from '@/Data/Billing/BillingContacts/RPCPerformRegisterAdditionalUsers';
import { RPCPerformRegisterMainUser } from '@/Data/Billing/BillingContacts/RPCPerformRegisterMainUser';
import { RPCPerformUpdateBillingContactDetails } from '@/Data/Billing/BillingContacts/RPCPerformUpdateBillingContactDetails';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { IJSONTable } from '@/Data/Models/JSONTable/JSONTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IBillingContacts extends IJSONTable {
	fullName: string | null;
	email: string | null;
	emailListMarketing: boolean | null;
	emailListTutorials: boolean | null;
	passwordHash?: string | null;
	marketingCampaign: string | null;
	phone: string | null;
	uuid: guid;
	companyId: guid | null;
	applicationData: { [id: string]: string | null; } | null;
	json: {
		lastModifiedISO8601?: string | null,
		lastModifiedBillingId?: guid | null,
		licenseAssignedProjectsSchedulingTime?: boolean | null,
		licenseAssignedOnCall?: boolean | null,
	};
}

export class BillingContacts {
	// RPC Methods
	public static RequestBillingContactsForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingContactsForCurrentSession>(new RPCRequestBillingContactsForCurrentSession());
	public static PerformChangeSessionPassword = 
		RPCMethod.Register<RPCPerformChangeSessionPassword>(new RPCPerformChangeSessionPassword());
	public static PerformDeleteBillingContact = 
		RPCMethod.Register<RPCPerformDeleteBillingContact>(new RPCPerformDeleteBillingContact());
	public static PerformRegisterAdditionalUsers = 
		RPCMethod.Register<RPCPerformRegisterAdditionalUsers>(new RPCPerformRegisterAdditionalUsers());
	public static PerformRegisterMainUser = 
		RPCMethod.Register<RPCPerformRegisterMainUser>(new RPCPerformRegisterMainUser());
	public static PerformUpdateBillingContactDetails = 
		RPCMethod.Register<RPCPerformUpdateBillingContactDetails>(new RPCPerformUpdateBillingContactDetails());
		
	// public static _RefreshTracker: { [id: string]: ITracker } = {};
	
	// public static FetchForId(id: guid): IRoundTripRequest {
		
	// 	const ret: IRoundTripRequest = {
	// 		roundTripRequestId: GenerateID(),
	// 		outboundRequestPromise: null,
	// 		completeRequestPromise: null,
	// 		_completeRequestPromiseResolve: null,
	// 		_completeRequestPromiseReject: null,
	// 	};
		
		
	// 	// If we have no id, reject.
	// 	if (!id || IsNullOrEmpty(id)) {
	// 		ret.outboundRequestPromise = Promise.reject();
	// 		ret.completeRequestPromise = Promise.reject();
	// 		return ret;
	// 	}
		
	// 	// Remove all trackers that are complete and older than 5 seconds.
	// 	const keys = Object.keys(this._RefreshTracker);
	// 	for (const key of keys) {
	// 		const tracker: ITracker = this._RefreshTracker[key];
	// 		if (!tracker.isComplete) {
	// 			continue;
	// 		}
			
	// 		if (DateTime.utc() > tracker.lastRequestTimeUtc.plus({seconds: 5})) {
	// 			delete this._RefreshTracker[key];
	// 		}
	// 	}
		
	// 	// Check and see if we already have a request pending.
	// 	const existing = this._RefreshTracker[id];
	// 	if (existing) {
	// 		return existing.rtr;
	// 	}
		
		
	// 	const billingContacts = BillingContacts.ForId(id);
	// 	if (billingContacts) {
	// 		ret.outboundRequestPromise = Promise.resolve();
	// 		ret.completeRequestPromise = Promise.resolve(billingContacts);
	// 		return ret;
	// 	}
		
	// 	// We'll need to request this.
	// 	const rtrNew = BillingContacts.RequestBillingContactsForCurrentSession.Send({
	//		sessionId: BillingSessions.CurrentSessionId(),
	// 		limitToIds: [id],
	// 	});
		
	// 	ret.outboundRequestPromise = rtrNew.outboundRequestPromise;
		
	// 	ret.completeRequestPromise = new Promise((resolve, reject) => {
	// 		ret._completeRequestPromiseResolve = resolve;
	// 		ret._completeRequestPromiseReject = reject;
	// 	});
		
		
	// 	// Handlers once we get a response.
	// 	if (rtrNew.completeRequestPromise) {
			
	// 		rtrNew.completeRequestPromise.then(() => {
	// 			if (ret._completeRequestPromiseResolve) {
	// 				ret._completeRequestPromiseResolve(Agent.ForId(id));
	// 			}
	// 		});
			
	// 		rtrNew.completeRequestPromise.catch((e: Error) => {
	// 			if (ret._completeRequestPromiseReject) {
	// 				ret._completeRequestPromiseReject(e);
	// 			}
	// 		});
			
	// 		rtrNew.completeRequestPromise.finally(() => {
	// 			this._RefreshTracker[id].isComplete = true;
	// 		});
	// 	}
		
		
	// 	this._RefreshTracker[id] = {
	// 		lastRequestTimeUtc: DateTime.utc(),
	// 		isComplete: false,
	// 		rtr: rtrNew,
	// 	};
		
	// 	return ret;
	// }
	
	
	
	
	
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingContacts {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingContacts {
		const uuid = GenerateID();
		const ret: IBillingContacts = {
			fullName: null,
			email: null,
			emailListMarketing: null,
			emailListTutorials: null,
			marketingCampaign: null,
			phone: null,
			uuid,
			companyId: null,
			applicationData: {},
			json: {},
		};
		
		return ret;
	}
	
	public static All(): Record<string, IBillingContacts> | null {
		const billingContacts = store.state.Database.billingContacts as Record<string, IBillingContacts>;
		if (!billingContacts || Object.keys(billingContacts).length === 0) {
			return null;
		}
		return billingContacts;
	}
	
	public static ForId(id: guid | null): IBillingContacts | null {
		
		if (!id) {
			return null;
		}
		
		const billingContacts = store.state.Database.billingContacts as Record<string, IBillingContacts>;
		if (!billingContacts || Object.keys(billingContacts).length === 0) {
			return null;
		}
		
		let billingContact = billingContacts[id];
		if (!billingContact) {
			billingContact = CaseInsensitivePropertyGet(billingContacts, id);
		}
		if (!billingContact) {
			return null;
		}
		
		return billingContact;
		
	}
	
	public static DeleteId(id: guid | null): IRoundTripRequest | null {
		
		if (!id) {
			return null;
		}
		
		return BillingContacts.PerformDeleteBillingContact.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			billingContacts: [id],
		});
		
	}
	
	public static Add(contact: IBillingContacts | null): IRoundTripRequest | null {
		
		if (!contact) {
			return null;
		}
		
		const payload = {
			sessionId: BillingSessions.CurrentSessionId(),
			billingContacts: {
				[contact.uuid as string]: contact,
			},
		};
		
		console.debug('BillingContacts.Add', payload);
		
		return BillingContacts.PerformUpdateBillingContactDetails.Send(payload);
		
	}
	
	
	public static CurrentBillingContactId(): string | null {
		
		const sessionId = BillingSessions.CurrentSessionId();
		if (!sessionId || IsNullOrEmpty(sessionId)) {
			return null;
		}
		
		const sessions: Record<string, IBillingSessions> = store.state.Database.billingSessions;
		if (!sessions) {
			return null;
		}
		
		const session = sessions[sessionId];
		if (!session) {
			return null;
		}
		
		const billingContactId = session.contactId;
		if (!billingContactId || IsNullOrEmpty(billingContactId)) {
			return null;
		}
		
		return billingContactId;
		
	}
	
	public static ForCurrentSession(): IBillingContacts | null {
		return BillingContacts.ForId(BillingContacts.CurrentBillingContactId());
	}
	
	public static EMailForCurrentSession(): string | null {
		const contact: IBillingContacts | null = BillingContacts.ForCurrentSession();
		if (!contact) {
			return null;
		}
		
		if (!contact.email || IsNullOrEmpty(contact.email)) {
			return null;
		}
		
		return contact.email;
	}
	
	public static NameForCurrentSession(): string | null {
		
		const contact: IBillingContacts | null = BillingContacts.ForCurrentSession();
		if (!contact) {
			return null;
		}
		
		if (!contact.fullName || IsNullOrEmpty(contact.fullName)) {
			return null;
		}
		
		return contact.fullName;
		
	}
	
	public static ValidateObject(o: IBillingContacts): IBillingContacts {
		
		
		
		return o;
	}
	
	public static PermBillingContactsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.contacts.read-any') !== -1 ||
			perms.indexOf('billing.contacts.read-company') !== -1 ||
			perms.indexOf('billing.contacts.read-self') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	public static PermCRMCanModifySelf(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.contacts.modify-any') !== -1 ||
			perms.indexOf('billing.contacts.modify-self') !== -1 ||
			perms.indexOf('billing.contacts.modify-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	public static PermBillingContactsCanModify(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.contacts.modify-any') !== -1 ||
			perms.indexOf('billing.contacts.modify-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	
}


(window as any).DEBUG_BillingContacts = BillingContacts;

export default {};

