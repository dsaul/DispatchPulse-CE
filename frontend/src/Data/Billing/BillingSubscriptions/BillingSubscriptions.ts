import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";
import store from "@/plugins/store/store";
import { BillingContacts } from "../BillingContacts/BillingContacts";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestBillingSubscriptionsForCurrentSession } from "@/Data/Billing/BillingSubscriptions/RPCRequestBillingSubscriptionsForCurrentSession";
import { RPCMethod } from "@/RPC/RPCMethod";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";

export interface IBillingSubscriptions {
	uuid: string;
	companyId: string | null;
	packageId: string | null;
	timestampAddedUtc: string;
	provisioningActual: string;
	provisioningDesired: string;
	provisioningDatabaseName: string | null;
	timestampLastSettingsPushUtc: string | null;
	json: Record<string, any>;
}

export class BillingSubscriptions {
	// RPC Methods
	public static RequestBillingSubscriptionsForCurrentSession = RPCMethod.Register<
		RPCRequestBillingSubscriptionsForCurrentSession
	>(new RPCRequestBillingSubscriptionsForCurrentSession());

	public static GetMerged(
		mergeValues: Record<string, any>
	): IBillingSubscriptions {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IBillingSubscriptions {
		const id = GenerateID();
		const ret: IBillingSubscriptions = {
			uuid: id,
			companyId: null,
			packageId: null,
			timestampAddedUtc: "",
			provisioningActual: "",
			provisioningDesired: "",
			provisioningDatabaseName: null,
			timestampLastSettingsPushUtc: null,
			json: {}
		};

		return ret;
	}

	public static GetAll(): IBillingSubscriptions[] {
		const billingContact = BillingContacts.ForCurrentSession();
		if (!billingContact) {
			return [];
		}

		const billingCompanyId = billingContact.companyId;

		//const billingCompanies: Record<string, IBillingCompanies> = store.state.Database.billingCompanies;

		//const company = billingCompanies[billingCompanyId];

		const billingSubscriptions: Record<string, IBillingSubscriptions> =
			store.state.Database.billingSubscriptions;

		let filtered = [] as IBillingSubscriptions[];

		filtered = _.filter(
			billingSubscriptions,
			(o: IBillingSubscriptions) => {
				return o.companyId === billingCompanyId;
			}
		);

		return filtered;
	}

	public static ValidateObject(
		o: IBillingSubscriptions
	): IBillingSubscriptions {
		return o;
	}

	public static PermCRMCanRequestSubscriptions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("billing.subscription.read-any") !== -1 ||
			perms.indexOf("billing.subscription.read-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

export default {};
