import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestBillingPaymentMethodForCurrentSession } from "@/Data/Billing/BillingPaymentMethod/RPCRequestBillingPaymentMethodForCurrentSession";
import { RPCPerformRegisterPaymentInformation } from "@/Data/Billing/BillingPaymentMethod/RPCPerformRegisterPaymentInformation";
import { RPCMethod } from "@/RPC/RPCMethod";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";

export interface IBillingPaymentMethod {
	uuid: string;
	value: string;
	json: Record<string, any>;
}

export class BillingPaymentMethod {
	// RPC Methods
	public static RequestBillingPaymentMethodForCurrentSession = RPCMethod.Register<
		RPCRequestBillingPaymentMethodForCurrentSession
	>(new RPCRequestBillingPaymentMethodForCurrentSession());
	public static PerformRegisterPaymentInformation = RPCMethod.Register<
		RPCPerformRegisterPaymentInformation
	>(new RPCPerformRegisterPaymentInformation());

	public static GetMerged(
		mergeValues: Record<string, any>
	): IBillingPaymentMethod {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IBillingPaymentMethod {
		const id = GenerateID();
		const ret: IBillingPaymentMethod = {
			uuid: id,
			value: "",
			json: {}
		};

		return ret;
	}

	public static ValidateObject(
		o: IBillingPaymentMethod
	): IBillingPaymentMethod {
		return o;
	}

	public static PermBillingPaymentMethodCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("billing.payment-method.read-any") !== -1 ||
			perms.indexOf("billing.payment-method.read-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

export default {};
