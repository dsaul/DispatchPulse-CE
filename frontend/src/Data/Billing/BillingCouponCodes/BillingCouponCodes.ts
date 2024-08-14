import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";
import { RPCRequestBillingCouponCodesForCurrentSession } from "@/Data/Billing/BillingCouponCodes/RPCRequestBillingCouponCodesForCurrentSession";
import { RPCMethod } from "@/RPC/RPCMethod";

export interface IBillingCouponCodes {
	uuid: string;
	discount: number | null;
	displayName: string | null;
	months: number | null;
	couponCode: string | null;
	forbidNewApplications: boolean;
	json: Record<string, any>;
}

export class BillingCouponCodes {
	// RPC Methods
	public static RequestBillingCouponCodesForCurrentSession = RPCMethod.Register<
		RPCRequestBillingCouponCodesForCurrentSession
	>(new RPCRequestBillingCouponCodesForCurrentSession());

	public static GetMerged(
		mergeValues: Record<string, any>
	): IBillingCouponCodes {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IBillingCouponCodes {
		const id = GenerateID();
		const ret: IBillingCouponCodes = {
			uuid: id,
			discount: null,
			displayName: null,
			months: null,
			couponCode: null,
			forbidNewApplications: false,
			json: {}
		};

		return ret;
	}

	public static ValidateObject(o: IBillingCouponCodes): IBillingCouponCodes {
		return o;
	}
}

export default {};
