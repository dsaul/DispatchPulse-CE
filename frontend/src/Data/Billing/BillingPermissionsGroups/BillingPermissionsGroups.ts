import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import { BillingPermissionsGroupsMemberships } from '@/Data/Billing/BillingPermissionsGroupsMemberships/BillingPermissionsGroupsMemberships';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { RPCRequestBillingPermissionsGroupsForCurrentSession } from '@/Data/Billing/BillingPermissionsGroups/RPCRequestBillingPermissionsGroupsForCurrentSession';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';

export interface IBillingPermissionsGroups {
	id: string;
	name: string | null;
	json: {
		hidden: boolean;
	};
}

export class BillingPermissionsGroups {
	// RPC Methods
	public static RequestBillingPermissionsGroupsForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingPermissionsGroupsForCurrentSession>(
			new RPCRequestBillingPermissionsGroupsForCurrentSession());
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingPermissionsGroups {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingPermissionsGroups {
		const id = GenerateID();
		const ret: IBillingPermissionsGroups = {
			id,
			name: null,
			json: {
				hidden: false,
			},
		};
		
		return ret;
	}
	
	public static All(): Record<string, IBillingPermissionsGroups> | null {
		
		const groups = store.state.Database.billingPermissionsGroups as Record<string, IBillingPermissionsGroups>;
		if (!groups || Object.keys(groups).length === 0) {
			return null;
		}
		
		return groups;
	}
	
	public static ForId(id: string | null): IBillingPermissionsGroups | null {
		
		if (!id) {
			return null;
		}
		
		const groups = store.state.Database.billingPermissionsGroups as Record<string, IBillingPermissionsGroups>;
		if (!groups || Object.keys(groups).length === 0) {
			return null;
		}
		
		let group = groups[id];
		if (!status || !group.json) {
			group = CaseInsensitivePropertyGet(groups, id);
		}
		if (!group || !group.json) {
			return null;
		}
		
		return group;
		
	}
	
	public static GroupIdsForBillingId(id: string | null): string[] {
		
		if (!id) {
			return [];
		}
		
		const ret = [];
		const memberships = BillingPermissionsGroupsMemberships.ForBillingContactId(id);
		for (const membership of Object.values(memberships)) {
			ret.push(membership.groupId);
		}
		
		return ret;
	}
	
	
	
	
	
	public static ValidateObject(o: IBillingPermissionsGroups): IBillingPermissionsGroups {
		
		
		
		return o;
	}
	
	public static PermBillingPermissionsGroupsCanRequest(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.permissions-groups.read-any') !== -1 ||
			perms.indexOf('billing.permissions-groups.read-company') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
}

 

export default {};

