import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { guid } from '@/Utility/GlobalTypes';
import ITracker from '@/Utility/ITracker';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestCompanies } from '@/Data/CRM/Company/RPCRequestCompanies';
import { RPCDeleteCompanies } from '@/Data/CRM/Company/RPCDeleteCompanies';
import { RPCPushCompanies } from '@/Data/CRM/Company/RPCPushCompanies';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { ILabeledCompanyId } from '@/Data/Models/LabeledCompanyId/LabeledCompanyId';

export interface ICompany extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string | null;
		logoURI: string | null;
		websiteURI: string | null;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}

export class Company {
	
	// RPC Methods
	public static RequestCompanies = RPCMethod.Register<RPCRequestCompanies>(new RPCRequestCompanies());
	public static DeleteCompanies = RPCMethod.Register<RPCDeleteCompanies>(new RPCDeleteCompanies());
	public static PushCompanies = RPCMethod.Register<RPCPushCompanies>(new RPCPushCompanies());
	
	
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
		
		
		const company = Company.ForId(id);
		if (company) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(company);
			return ret;
		}
		
		// We'll need to request this.
		const rtrNew = Company.RequestCompanies.Send({
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
					ret._completeRequestPromiseResolve(Company.ForId(id));
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
	
	public static GetMerged(mergeValues: Record<string, any>): ICompany {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): ICompany {
		const id = GenerateID();
		const ret: ICompany = {
			id,
			json: {
				id,
				name: null,
				logoURI: null,
				websiteURI: null,
				lastModifiedISO8601: DateTime.utc().toISO(),
				lastModifiedBillingId: null,
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static ForId(id: string | null): ICompany | null {
		
		if (!id) {
			return null;
		}
		
		const companies = store.state.Database.companies as Record<string, ICompany>;
		if (!companies || Object.keys(companies).length === 0) {
			return null;
		}
		
		let company = companies[id];
		if (!company) {
			company = CaseInsensitivePropertyGet(company, id);
		}
		if (!company) {
			return null;
		}
		
		return company;
		
	}
	
	public static UpdateIds(payload: Record<string, ICompany>): void {
		store.commit('UpdateCompanies', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteCompanies', ids);
		
	}
	
	public static NameForId(id: string | null): string | null {
		
		const company = Company.ForId(id);
		if (!company || !company.json) {
			return null;
		}
		
		return company.json.name || null;
		
	}
	
	public static CompanyListDescriptionForIds(companies: ILabeledCompanyId[]): null | string {
		
		const names = [];
			
		// Gather names first.
		for (const labelledCompany of companies) {
			
			if (null == labelledCompany) {
				continue;
			}
			
			const companyId: string | null = labelledCompany.value;
			if (IsNullOrEmpty(companyId)) {
				continue;
			}
			
			const thisName = Company.NameForId(companyId);
			if (IsNullOrEmpty(thisName)) {
				continue;
			}
			
			
			names.push(thisName);
		}
		
		
		
		
		let name = '';
		
		for (let i = 0; i < names.length; i++) {
			
			if (i !== 0) {
				name += ', ';
			}
			if (i !== 0 && i === names.length - 1) {
				name += ' and ';
			}
			
			name += names[i];
			
		}
		
		return IsNullOrEmpty(name) ? null : name;
	}
	
	public static ValidateObject(o: ICompany): ICompany {
		
		
		
		return o;
	}
	
	
	
	
	
	
	
	
	
	public static PermCompaniesCanRequest(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.companies.request-any') !== -1 ||
			perms.indexOf('crm.companies.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	public static PermCompaniesCanPush(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.companies.push-any') !== -1 ||
			perms.indexOf('crm.companies.push-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	public static PermCompaniesCanDelete(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.companies.delete-any') !== -1 ||
			perms.indexOf('crm.companies.delete-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
	public static PermCRMReportCompaniesPDF(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.report.companies-pdf') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	public static PermCRMExportCompaniesCSV(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('crm.export.companies-csv') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
}
 

export default {};

