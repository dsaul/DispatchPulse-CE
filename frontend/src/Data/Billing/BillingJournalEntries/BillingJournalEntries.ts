import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestBillingJournalEntriesForCurrentSession } from '@/Data/Billing/BillingJournalEntries/RPCRequestBillingJournalEntriesForCurrentSession';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';

export interface IBillingJournalEntries {
	uuid: string;
	timestamp: string;
	type: string;
	otherEntryId: string | null;
	description: string | null;
	quantity: number;
	unitPrice: number;
	currency: string;
	taxPercentage: number;
	taxActual: number;
	invoiceId: string;
	packageId: string;
	companyId: string;
	json: Record<string, any>;
}


export class BillingJournalEntries {
	
	// RPC Methods
	public static RequestBillingJournalEntriesForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingJournalEntriesForCurrentSession>(
			new RPCRequestBillingJournalEntriesForCurrentSession());
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingJournalEntries {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingJournalEntries {
		const id = GenerateID();
		const ret: IBillingJournalEntries = {
			uuid: id,
			timestamp: DateTime.utc().toISO(),
			type: '',
			otherEntryId: null,
			description: null,
			quantity: 0,
			unitPrice: 0,
			currency: '',
			taxPercentage: 0,
			taxActual: 0,
			invoiceId: '',
			packageId: '',
			companyId: '',
			json: {},
		};
		
		return ret;
	}
	
	public static GetAll(): IBillingJournalEntries[] {
		
		const billingContact = BillingContacts.ForCurrentSession();
		if (!billingContact) {
			return [];
		}
		
		const billingCompanyId = billingContact.companyId;
		
		//const billingCompanies: Record<string, IBillingCompanies> = store.state.Database.billingCompanies;
		
		//const company = billingCompanies[billingCompanyId];
		
		const billingJournalEntries: Record<string, IBillingJournalEntries> = 
			store.state.Database.billingJournalEntries;
		
		let filtered = [] as IBillingJournalEntries[];
		
		filtered = _.filter(
			billingJournalEntries,
			(o: IBillingJournalEntries) => {
				return o.companyId === billingCompanyId;
			},
		);
		
		return filtered;
		
	}
	
	public static ValidateObject(o: IBillingJournalEntries): IBillingJournalEntries {
		
		
		
		return o;
	}
	
	
	public static PermCRMCanRequestJournalEntries(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.journal-entries.read-any') !== -1 ||
			perms.indexOf('billing.journal-entries.read-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	public static PermBillingCanMakeOneTimeCompanyCreditCardPayments(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.credit-card-payments.one-time') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	public static PermBillingCanSetupPreAuthorizedCreditCardPayments(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.credit-card-payments.setup-pre-authorized') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	
}


 

export default {};

