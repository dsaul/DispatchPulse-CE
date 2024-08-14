import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import { RPCRequestBillingCurrencyForCurrentSession } from '@/Data/Billing/BillingCurrency/RPCRequestBillingCurrencyForCurrentSession';
import { RPCMethod } from '@/RPC/RPCMethod';


export interface IBillingCurrency {
	uuid: string;
	currency: string;
	json: Record<string, any>;
}


export class BillingCurrency {
	// RPC Methods
	public static RequestBillingCurrencyForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingCurrencyForCurrentSession>(
			new RPCRequestBillingCurrencyForCurrentSession());
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingCurrency {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingCurrency {
		const id = GenerateID();
		const ret: IBillingCurrency = {
			uuid: id,
			currency: '',
			json: {},
		};
		
		return ret;
	}
	
	public static ValidateObject(o: IBillingCurrency): IBillingCurrency {
		
		
		
		return o;
	}
	
}
 

export default {};

