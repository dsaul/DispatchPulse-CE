import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import '@/Data/Models/PhoneNumber/PhoneNumber';
import '@/Data/Models/EMail/EMail';
import '@/Data/Models/Address/Address';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import { guid } from '@/Utility/GlobalTypes';
import ITracker from '@/Utility/ITracker';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestContacts } from '@/Data/CRM/Contact/RPCRequestContacts';
import { RPCDeleteContacts } from '@/Data/CRM/Contact/RPCDeleteContacts';
import { RPCPushContacts } from '@/Data/CRM/Contact/RPCPushContacts';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IPhoneNumber } from '@/Data/Models/PhoneNumber/PhoneNumber';
import { IEMail } from '@/Data/Models/EMail/EMail';
import { IAddress } from '@/Data/Models/Address/Address';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IContact extends ICRMTable {
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
		companyId: string | null;
		notes: string | null;
		phoneNumbers: IPhoneNumber[];
		emails: IEMail[];
		addresses: IAddress[];
	};
}

export class Contact {
	// RPC Methods
	public static RequestContacts = RPCMethod.Register<RPCRequestContacts>(new RPCRequestContacts());
	public static DeleteContacts = RPCMethod.Register<RPCDeleteContacts>(new RPCDeleteContacts());
	public static PushContacts = RPCMethod.Register<RPCPushContacts>(new RPCPushContacts());
	
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
		
		
		const contact = Contact.ForId(id);
		if (contact) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(contact);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = Contact.RequestContacts.Send({
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
					ret._completeRequestPromiseResolve(Contact.ForId(id));
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
	
	
	
	
	
	public static GetMerged(mergeValues: Record<string, any>): IContact {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IContact {
		const id = GenerateID();
		const ret: IContact = {
			id,
			json: {
				id,
				lastModifiedISO8601: DateTime.utc().toISO(),
				lastModifiedBillingId: null,
				name: null,
				title: null,
				companyId: null,
				notes: null,
				phoneNumbers: [],
				emails: [],
				addresses: [],
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): IContact | null {
		if (!id) {
			return null;
		}
		
		const contacts = store.state.Database.contacts as Record<string, IContact>;
		if (!contacts || Object.keys(contacts).length === 0) {
			return null;
		}
		
		let contact = contacts[id];
		if (!contact) {
			contact = CaseInsensitivePropertyGet(contact, id);
		}
		if (!contact) {
			return null;
		}
		
		return contact;
	}
	
	public static UpdateIds(payload: Record<string, IContact>): void {
		store.commit('UpdateContacts', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteContacts', ids);
		
	}
	
	public static ForCompanyId(companyId: string): IContact[] {
		
		let ret: IContact[] = [];
		
		if (companyId === null) {
			return ret;
		}
		
		ret = _.filter(
			store.state.Database.contacts,
			(o: IContact) => {
				return o.json.companyId === companyId;
			});
		
		ret = _.sortBy(ret, (o: IContact) => {
			return o.json.name;
		});
		
		//console.debug(ret);
		
		return ret;
		
	}
	
	public static NameForId(id: string | null): string | null {
		const contact = Contact.ForId(id);
		if (!contact || !contact.json) {
			return null;
		}
		
		return contact.json.name || null;
	}
	
	public static ValidateObject(o: IContact): IContact {
		
		
		
		return o;
	}
	
	
	
	public static PermContactsCanRequest(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.contacts.request-any') !== -1 ||
			perms.indexOf('crm.contacts.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermContactsCanPush(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.contacts.push-any') !== -1 ||
			perms.indexOf('crm.contacts.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	public static PermContactsCanDelete(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.contacts.delete-any') !== -1 ||
			perms.indexOf('crm.contacts.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
	public static PermCRMReportContactsPDF(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.report.contacts-pdf') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	public static PermCRMExportContactsCSV(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.export.contacts-csv') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
}




export default {};
