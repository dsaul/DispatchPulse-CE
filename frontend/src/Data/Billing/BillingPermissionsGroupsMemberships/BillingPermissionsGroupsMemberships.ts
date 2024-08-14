import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestBillingPermissionsGroupsMembershipsForCurrentSession } from "@/Data/Billing/BillingPermissionsGroupsMemberships/RPCRequestBillingPermissionsGroupsMembershipsForCurrentSession";
import { RPCPerformBillingPermissionsGroupsMembershipsAdd } from "@/Data/Billing/BillingPermissionsGroupsMemberships/RPCPerformBillingPermissionsGroupsMembershipsAdd";
import { RPCPerformBillingPermissionsGroupsMembershipsRemove } from "@/Data/Billing/BillingPermissionsGroupsMemberships/RPCPerformBillingPermissionsGroupsMembershipsRemove";
import { RPCMethod } from "@/RPC/RPCMethod";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IBillingPermissionsGroupsMemberships {
	id: string;
	groupId: string;
	contactId: string;
	json: Record<string, any>;
}

export class BillingPermissionsGroupsMemberships {
	// RPC Methods
	public static RequestBillingPermissionsGroupsMembershipsForCurrentSession = RPCMethod.Register<
		RPCRequestBillingPermissionsGroupsMembershipsForCurrentSession
	>(new RPCRequestBillingPermissionsGroupsMembershipsForCurrentSession());
	public static PerformBillingPermissionsGroupsMembershipsAdd = RPCMethod.Register<
		RPCPerformBillingPermissionsGroupsMembershipsAdd
	>(new RPCPerformBillingPermissionsGroupsMembershipsAdd());
	public static PerformBillingPermissionsGroupsMembershipsRemove = RPCMethod.Register<
		RPCPerformBillingPermissionsGroupsMembershipsRemove
	>(new RPCPerformBillingPermissionsGroupsMembershipsRemove());

	public static GetMerged(
		mergeValues: Record<string, any>
	): IBillingPermissionsGroupsMemberships {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IBillingPermissionsGroupsMemberships {
		const id = GenerateID();
		const ret: IBillingPermissionsGroupsMemberships = {
			id,
			groupId: "",
			contactId: "",
			json: {}
		};

		return ret;
	}

	public static All(): Record<
		string,
		IBillingPermissionsGroupsMemberships
	> | null {
		const memberships = store.state.Database
			.billingPermissionsGroupsMemberships as {
			[id: string]: IBillingPermissionsGroupsMemberships;
		};
		if (!memberships || Object.keys(memberships).length === 0) {
			return null;
		}

		return memberships;
	}

	public static ForId(
		id: string | null
	): IBillingPermissionsGroupsMemberships | null {
		if (!id) {
			return null;
		}

		const memberships = this.All();
		if (!memberships || Object.keys(memberships).length === 0) {
			return null;
		}

		let group = memberships[id];
		if (!status || !group.json) {
			group = CaseInsensitivePropertyGet(memberships, id);
		}
		if (!group || !group.json) {
			return null;
		}

		return group;
	}

	public static ForBillingContactId(
		contactId: string | null
	): {
		[id: string]: IBillingPermissionsGroupsMemberships;
	} {
		//console.log('ForBillingContactId', contactId);

		const all = this.All();
		//console.log('ForBillingContactId all', all);
		if (null === all) {
			return {};
		}

		const ret: Record<string, IBillingPermissionsGroupsMemberships> = {};

		for (const [key, value] of Object.entries(all)) {
			if (value.contactId === contactId) {
				ret[key] = value;
			}
		}

		return ret;
	}

	public static ValidateObject(
		o: IBillingPermissionsGroupsMemberships
	): IBillingPermissionsGroupsMemberships {
		return o;
	}

	public static PerformAddMemberships(
		billingContactId: string,
		permissionsGroupIds: string[]
	): IRoundTripRequest {
		return BillingPermissionsGroupsMemberships.PerformBillingPermissionsGroupsMembershipsAdd.Send(
			{
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId,
				permissionsGroupIds
			}
		);
	}

	public static PerformRemoveMemberships(
		billingContactId: string,
		permissionsGroupIds: string[]
	): IRoundTripRequest {
		return BillingPermissionsGroupsMemberships.PerformBillingPermissionsGroupsMembershipsRemove.Send(
			{
				sessionId: BillingSessions.CurrentSessionId(),
				billingContactId,
				permissionsGroupIds
			}
		);
	}

	public static PermBillingPermissionsGroupsMembershipsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("billing.permissions-groups-memberships.read-any") !==
				-1 ||
			perms.indexOf(
				"billing.permissions-groups-memberships.read-company"
			) !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

export default {};
