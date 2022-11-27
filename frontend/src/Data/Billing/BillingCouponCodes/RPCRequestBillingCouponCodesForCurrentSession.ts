import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IBillingCouponCodes } from './BillingCouponCodes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestBillingCouponCodesForCurrentSessionPayload = IIdempotencyRequest
	
	export interface IRequestBillingCouponCodesForCurrentSessionCB extends IIdempotencyResponse {
		billingCouponCodes: IBillingCouponCodes[];
	}
export class RPCRequestBillingCouponCodesForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingCouponCodesForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingCouponCodesForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingCouponCodesForCurrentSessionCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestBillingCouponCodesForCurrentSessionCB): boolean {
		
		if (!payload.billingCouponCodes) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting billing coupon codes #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingCouponCodes> = {};
		
		for (const e of payload.billingCouponCodes) {
			newPayload[e.uuid] = e;
		}
		
		store.commit('UpdateBillingCouponCodesRemote', newPayload);
		
		return true;
	}
}

export default {};
