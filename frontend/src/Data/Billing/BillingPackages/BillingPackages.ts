import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import store from '@/plugins/store/store';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestBillingPackagesForCurrentSession } from '@/Data/Billing/BillingPackages/RPCRequestBillingPackagesForCurrentSession';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';

export interface IBillingPackages {
	uuid: string;
	packageName: string;
	displayName: string;
	currency: string;
	costPerMonth: number;
	provisionDispatchPulse: boolean;
	provisionDispatchPulseUsers: number;
	allowNewAssignment: boolean;
	type: string;
	provisionEmail: boolean;
	provisionEmailUsers: number;
	provisionWebsites: boolean;
	provisionWebsitesStaticCount: number;
	isDemo: boolean;
	json: {
		ProvisionS3StorageMB?: number,
		ProvisionOnCallAutoAttendants?: boolean,
		ProvisionOnCallResponders?: number,
		ProvisionOnCallUsers?: number,
	};
}

export class BillingPackages {
	// RPC Methods
	public static RequestBillingPackagesForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingPackagesForCurrentSession>(new RPCRequestBillingPackagesForCurrentSession());
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingPackages {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingPackages {
		const id = GenerateID();
		const ret: IBillingPackages = {
			uuid: id,
			packageName: '',
			displayName: '',
			currency: '',
			costPerMonth: 0,
			provisionDispatchPulse: false,
			provisionDispatchPulseUsers: 0,
			allowNewAssignment: false,
			type: '',
			provisionEmail: false,
			provisionEmailUsers: 0,
			provisionWebsites: false,
			provisionWebsitesStaticCount: 0,
			isDemo: false,
			json: {},
		};
		
		return ret;
	}
	
	public static ForId(packageId: string): IBillingPackages | null {
		if (!packageId || IsNullOrEmpty(packageId)) {
			return null;
		}
		
		const billingPackages: Record<string, IBillingPackages> = store.state.Database.billingPackages;
		if (!billingPackages) {
			return null;
		}
		
		const pkg = billingPackages[packageId];
		if (!pkg) {
			return null;
		}
		
		return pkg;
	}
	
	public static CostForId(packageId: string): string {
		
		const pkg = BillingPackages.ForId(packageId);
		if (!pkg) {
			return '?';
		}
		
		const cost = `$${pkg.costPerMonth.toFixed(2)} ${pkg.currency}`;
		
		return  IsNullOrEmpty(cost) ? '?' : cost;
		
	}
	
	public static DisplayNameForId(packageId: string): string {
		
		const pkg = BillingPackages.ForId(packageId);
		if (!pkg) {
			return '?';
		}
		
		const displayName = `${pkg.displayName}`;
		
		return  IsNullOrEmpty(displayName) ? '?' : displayName;
		
	}
	
	public static NameForId(packageId: string): string {
		
		if (!packageId || IsNullOrEmpty(packageId)) {
			return '';
		}
		
		const pkg = BillingPackages.ForId(packageId);
		if (!pkg) {
			return packageId;
		}
		
		return pkg.packageName || packageId;
	}
	
	public static ValidateObject(o: IBillingPackages): IBillingPackages {
		
		
		
		return o;
	}
	
	public static PermBillingPackagesCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.packages.read-any') !== -1 ||
			perms.indexOf('billing.packages.read-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
	
	
}



export default {};

