import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";
import { RPCRequestBillingPackagesTypeForCurrentSession } from "@/Data/Billing/BillingPackagesType/RPCRequestBillingPackagesTypeForCurrentSession";
import { RPCMethod } from "@/RPC/RPCMethod";

export interface IBillingPackagesType {
	uuid: string;
	type: string;
	json: Record<string, any>;
}

export class BillingPackagesType {
	// RPC Methods
	public static RequestBillingPackagesTypeForCurrentSession = RPCMethod.Register<
		RPCRequestBillingPackagesTypeForCurrentSession
	>(new RPCRequestBillingPackagesTypeForCurrentSession());

	public static GetMerged(
		mergeValues: Record<string, any>
	): IBillingPackagesType {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IBillingPackagesType {
		const id = GenerateID();
		const ret: IBillingPackagesType = {
			uuid: id,
			type: "",
			json: {}
		};

		return ret;
	}

	public static ValidateObject(
		o: IBillingPackagesType
	): IBillingPackagesType {
		return o;
	}
}

export default {};
