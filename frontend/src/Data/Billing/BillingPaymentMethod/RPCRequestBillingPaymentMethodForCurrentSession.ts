import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IBillingPaymentMethod } from './BillingPaymentMethod';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export type IRequestBillingPaymentMethodForCurrentSessionPayload = IIdempotencyRequest
	
export interface IRequestBillingPaymentMethodForCurrentSessionCB extends IIdempotencyResponse {
	billingPaymentMethod: IBillingPaymentMethod[];
}

export class RPCRequestBillingPaymentMethodForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingPaymentMethodForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingPaymentMethodForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingPaymentMethodForCurrentSessionCB';
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest, 
		payload: IRequestBillingPaymentMethodForCurrentSessionCB,
		): boolean {
		
		if (!payload.billingPaymentMethod) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting payment methods #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingPaymentMethod> = {};
		
		for (const e of payload.billingPaymentMethod) {
			newPayload[e.uuid] = e;
		}
	
		
		store.commit('UpdateBillingPaymentMethodRemote', newPayload);
	
		
		return true;
	}
}

export default {};
