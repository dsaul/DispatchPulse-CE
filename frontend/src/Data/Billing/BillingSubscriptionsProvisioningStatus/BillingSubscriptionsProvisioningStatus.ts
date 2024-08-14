import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestBillingSubscriptionsProvisioningStatusForCurrentSession } from '@/Data/Billing/BillingSubscriptionsProvisioningStatus/RPCRequestBillingSubscriptionsProvisioningStatusForCurrentSession';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';

export interface IBillingSubscriptionsProvisioningStatus {
	uuid: string;
	status: string;
	json: Record<string, any>;
}

export class BillingSubscriptionsProvisioningStatus {
	// RPC Methods
	public static RequestBillingSubscriptionsProvisioningStatusForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingSubscriptionsProvisioningStatusForCurrentSession>(
			new RPCRequestBillingSubscriptionsProvisioningStatusForCurrentSession());
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingSubscriptionsProvisioningStatus {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingSubscriptionsProvisioningStatus {
		const id = GenerateID();
		const ret: IBillingSubscriptionsProvisioningStatus = {
			uuid: id,
			status: '',
			json: {},
		};
		
		return ret;
	}
	
	public static ValidateObject(o: IBillingSubscriptionsProvisioningStatus): IBillingSubscriptionsProvisioningStatus {
		
		
		
		return o;
	}
	
	
	public static PermBillingSubscriptionsProvisioningStatusCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.subscription-provisioning-status.request-any') !== -1 ||
			perms.indexOf('billing.subscription-provisioning-status.request-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
}



 

export default {};

