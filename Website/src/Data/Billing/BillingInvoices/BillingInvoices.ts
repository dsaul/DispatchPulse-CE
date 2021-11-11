import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import store from '@/plugins/store/store';
import { BillingContacts } from '../BillingContacts/BillingContacts';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestBillingInvoicesForCurrentSession } from '@/Data/Billing/BillingInvoices/RPCRequestBillingInvoicesForCurrentSession';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';

export interface IBillingInvoices {
	uuid: string;
	timestampStartUtc: string;
	timestampEndUtc: string;
	invoiceNumber: string;
	currency: string;
	amountDue: number;
	amountPaid: number;
	amountRemaining: number;
	timestampCreatedUtc: string;
	timestampDueUtc: string;
	companyId: string;
	json: Record<string, any>;
}

export class BillingInvoices {
	// RPC Methods
	public static RequestBillingInvoicesForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingInvoicesForCurrentSession>(new RPCRequestBillingInvoicesForCurrentSession());
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingInvoices {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingInvoices {
		const id = GenerateID();
		const ret: IBillingInvoices = {
			uuid: id,
			timestampStartUtc: '',
			timestampEndUtc: '',
			invoiceNumber: '',
			currency: '',
			amountDue: 0,
			amountPaid: 0,
			amountRemaining: 0,
			timestampCreatedUtc: '',
			timestampDueUtc: '',
			companyId: '',
			json: {},
		};
		
		return ret;
	}
	
	public static GetAll(): IBillingInvoices[] {
		
		const billingContact = BillingContacts.ForCurrentSession();
		if (!billingContact) {
			return [];
		}
		
		const billingCompanyId = billingContact.companyId;
		
		//const billingCompanies: Record<string, IBillingCompanies> = store.state.Database.billingCompanies;
		
		//const company = billingCompanies[billingCompanyId];
		
		const billingInvoices: Record<string, IBillingInvoices> = 
		store.state.Database.billingInvoices;
		
		let filtered = [] as IBillingInvoices[];
		
		filtered = _.filter(
			billingInvoices,
			(o: IBillingInvoices) => {
				return o.companyId === billingCompanyId;
			},
		);
		
		return filtered;
		
	}
	
	public static ValidateObject(o: IBillingInvoices): IBillingInvoices {
		
		
		
		return o;
	}
	
	public static PermCRMCanRequestInvoices(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.invoices.read-any') !== -1 ||
			perms.indexOf('billing.invoices.read-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
	}
	
	public static PermBillingInvoicesCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.invoices.read-any') !== -1 ||
			perms.indexOf('billing.invoices.read-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
}


 

export default {};

