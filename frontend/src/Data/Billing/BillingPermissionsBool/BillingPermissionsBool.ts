import GenerateID from '@/Utility/GenerateID';
import _ from 'lodash';
import store from '@/plugins/store/store';
import CaseInsensitivePropertyGet from '@/Utility/CaseInsensitivePropertyGet';
import { BillingPermissionsGroupsMemberships } from '@/Data/Billing/BillingPermissionsGroupsMemberships/BillingPermissionsGroupsMemberships';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { RPCRequestBillingPermissionsBoolForCurrentSession } from '@/Data/Billing/BillingPermissionsBool/RPCRequestBillingPermissionsBoolForCurrentSession';
import { RPCPerformBillingPermissionsBoolAdd } from '@/Data/Billing/BillingPermissionsBool/RPCPerformBillingPermissionsBoolAdd';
import { RPCPerformBillingPermissionsBoolRemove } from '@/Data/Billing/BillingPermissionsBool/RPCPerformBillingPermissionsBoolRemove';
import { RPCMethod } from '@/RPC/RPCMethod';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';

export interface IBillingPermissionsBool {
	uuid: string;
	key: string;
	value: boolean;
	contactId: string | null;
	groupId: string | null;
	json: Record<string, any>;
}

export class BillingPermissionsBool {
	// RPC Methods
	public static RequestBillingPermissionsBoolForCurrentSession = 
		RPCMethod.Register<RPCRequestBillingPermissionsBoolForCurrentSession>(
			new RPCRequestBillingPermissionsBoolForCurrentSession());
	public static PerformBillingPermissionsBoolAdd = 
		RPCMethod.Register<RPCPerformBillingPermissionsBoolAdd>(
			new RPCPerformBillingPermissionsBoolAdd());
	public static PerformBillingPermissionsBoolRemove = 
		RPCMethod.Register<RPCPerformBillingPermissionsBoolRemove>(
			new RPCPerformBillingPermissionsBoolRemove());
	
	public static GetMerged(mergeValues: Record<string, any>): IBillingPermissionsBool {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IBillingPermissionsBool {
		const id = GenerateID();
		const ret: IBillingPermissionsBool = {
			uuid: id,
			key: '',
			value: false,
			contactId: null,
			groupId: null,
			json: {},
		};
		
		return ret;
	}
	
	public static HasAnyPermissions(): boolean {
		const all = BillingPermissionsBool.All();
		if (!all) {
			return false;
		}
		return Object.keys(all).length !== 0;
	}
	
	public static Ready(fn: () => void): void {
		
		if (BillingPermissionsBool.AllForBillingContactId().length === 0) {
			setTimeout(() => this.Ready(fn), 500);
			return;
		}
		
		fn();
	}
	
	public static AllForBillingContactId(): string[] {
		const contactId = BillingContacts.CurrentBillingContactId();
		
		//console.debug('ComponentBase.Permissions, ', contactId);
		
		if (!contactId) {
			return [];
		}
		
		const ret = BillingPermissionsBool.KeysForBillingContactIdAmalgamated(contactId);
		//console.debug('ret', ret);
		
		
		return ret;
	}
	
	public static All(): Record<string, IBillingPermissionsBool> | null {
		
		const all = store.state.Database.billingPermissionsBool as { 
			[id: string]: IBillingPermissionsBool; };
		if (!all || Object.keys(all).length === 0) {
			return null;
		}
		
		return all;
	}
	
	public static ForId(id: string | null): IBillingPermissionsBool | null {
		
		if (!id) {
			return null;
		}
		
		const all = this.All();
		if (!all || Object.keys(all).length === 0) {
			return null;
		}
		
		let obj = all[id];
		if (!status || !obj.json) {
			obj = CaseInsensitivePropertyGet(all, id);
		}
		if (!obj || !obj.json) {
			return null;
		}
		
		return obj;
		
	}
	
	public static ForGroupIds(groupIds: string[]): IBillingPermissionsBool[] {
		
		//console.debug('ForGroupIds', groupIds, 'all', this.All());
		
		return _.filter(this.All(), (value) => {
			
			if (null === value.groupId) {
				return false;
			}
			
			// if ('456f60fe-774c-11ea-9784-02420a0000d8' === value.groupId) {
			// 	console.debug(value);
			// }
			
			
			if (groupIds.indexOf(value.groupId) !== -1) {
				return true;
			}
			
			return false;
		});
		
	}
	
	public static ForBillingContactId(contactId: string): IBillingPermissionsBool[] {
		
		//console.log('this.All()', this.All());
		
		return _.filter(this.All(), (value) => {
			
			if (null === value.contactId) {
				return false;
			}
			
			if (value.contactId === contactId) {
				return true;
			}
			
			return false;
		});
		
	}
	
	public static KeysForBillingContactIdAmalgamated(contactId: string): string[] {
		
		const ret: string[] = [];
		
		// First we add the user's specific positive permissions.
		
		const userPermissions = BillingPermissionsBool.ForBillingContactId(contactId);
		//console.debug('userPermissions', userPermissions);
		
		for (const perm of userPermissions) {
			if (perm.value === true &&
				ret.indexOf(perm.key) === -1
				) {
				ret.push(perm.key);
			}
		}
		
		// Next we find if they are in any groups.
		const groups = BillingPermissionsGroupsMemberships.ForBillingContactId(contactId);
		const groupIds = [];
		for (const group of Object.values(groups)) {
			groupIds.push(group.groupId);
		}
		//console.debug('groupIds', groupIds);
		
		const groupPerms = BillingPermissionsBool.ForGroupIds(groupIds);
		//console.debug('groupPerms', groupPerms);
		
		// Next we add all of the groups' positive permissions.
		for (const perm of groupPerms) {
			if (perm.value === true &&
				ret.indexOf(perm.key) === -1
				) {
				ret.push(perm.key);
			}
		}
		
		
		
		return ret;
	}
	
	
	public static ValidateObject(o: IBillingPermissionsBool): IBillingPermissionsBool {
		
		
		
		return o;
	}
	
	public static PermBillingPermissionsBoolCanRequest(): boolean {
		
		if (GetDemoMode()) {
			return true;
		}
		
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = 
			perms.indexOf('billing.permissions-bool.read-any') !== -1 ||
			perms.indexOf('billing.permissions-bool.read-company') !== -1 ||
			perms.indexOf('billing.permissions-bool.read-self') !== -1;
		//console.log(perms, ret);
		return ret;
		
		
	}
	
	
	
	
	
	
	
	
	
}


(window as any).DEBUG_BillingPermissionsBool = BillingPermissionsBool;

export default {};

