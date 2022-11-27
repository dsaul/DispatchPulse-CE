import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import { RPCRequestBillingIndustriesForCurrentSession } from '@/Data/Billing/BillingIndustries/RPCRequestBillingIndustriesForCurrentSession';
import { RPCMethod } from '@/RPC/RPCMethod';


export interface IBillingIndustries {
	uuid: string;
	value: string | null;
	json: Record<string, any>;
}

export class BillingIndustries {
	// RPC Methods
	public static RPCRequestBillingIndustriesForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingIndustriesForCurrentSession>(new RPCRequestBillingIndustriesForCurrentSession());
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingIndustries {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingIndustries {
		const id = GenerateID();
		const ret: IBillingIndustries = {
			uuid: id,
			value: null,
			json: {},
		};
		
		return ret;
	}
	
	public static ValidateObject(o: IBillingIndustries): IBillingIndustries {
		
		
		
		return o;
	}
	
}







 

export default {};

