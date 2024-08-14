import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import { RPCRequestBillingJournalEntriesTypeForCurrentSession } from '@/Data/Billing/BillingJournalEntriesType/RPCRequestBillingJournalEntriesTypeForCurrentSession';
import { RPCMethod } from '@/RPC/RPCMethod';

export interface IBillingJournalEntriesType {
	uuid: string;
	type: string;
	json: Record<string, any>;
}

export class BillingJournalEntriesType {
	// RPC Methods
	public static RequestBillingJournalEntriesTypeForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingJournalEntriesTypeForCurrentSession>(
			new RPCRequestBillingJournalEntriesTypeForCurrentSession());
	
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingJournalEntriesType {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingJournalEntriesType {
		const id = GenerateID();
		const ret: IBillingJournalEntriesType = {
			uuid: id,
			type: '',
			json: {},
		};
		
		return ret;
	}
	
	public static ValidateObject(o: IBillingJournalEntriesType): IBillingJournalEntriesType {
		
		
		
		return o;
	}
	
}

export default {};

