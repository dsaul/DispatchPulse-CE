import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";
import { RPCRequestBillingPaymentFrequenciesForCurrentSession } from "@/Data/Billing/BillingPaymentFrequencies/RPCRequestBillingPaymentFrequenciesForCurrentSession";
import { RPCMethod } from "@/RPC/RPCMethod";

export interface IBillingPaymentFrequencies {
	uuid: string;
	value: string;
	displayName: string;
	monthsBetweenPayments: number;
	json: Record<string, any>;
}

export class BillingPaymentFrequencies {
	// RPC Methods
	public static RequestBillingPaymentFrequenciesForCurrentSession = RPCMethod.Register<
		RPCRequestBillingPaymentFrequenciesForCurrentSession
	>(new RPCRequestBillingPaymentFrequenciesForCurrentSession());

	public static GetMerged(
		mergeValues: Record<string, any>
	): IBillingPaymentFrequencies {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IBillingPaymentFrequencies {
		const id = GenerateID();
		const ret: IBillingPaymentFrequencies = {
			uuid: id,
			value: "",
			displayName: "",
			monthsBetweenPayments: 1,
			json: {}
		};

		return ret;
	}

	public static ValidateObject(
		o: IBillingPaymentFrequencies
	): IBillingPaymentFrequencies {
		return o;
	}
}

export default {};
