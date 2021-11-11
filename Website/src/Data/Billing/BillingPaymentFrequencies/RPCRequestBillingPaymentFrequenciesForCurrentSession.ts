import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IBillingPaymentFrequencies } from './BillingPaymentFrequencies';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestBillingPaymentFrequenciesForCurrentSessionPayload = IIdempotencyRequest
	
export interface IRequestBillingPaymentFrequenciesForCurrentSessionCB extends IIdempotencyResponse {
	billingPaymentFrequencies: IBillingPaymentFrequencies[];
}

export class RPCRequestBillingPaymentFrequenciesForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingPaymentFrequenciesForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingPaymentFrequenciesForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingPaymentFrequenciesForCurrentSessionCB';
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest, 
		payload: IRequestBillingPaymentFrequenciesForCurrentSessionCB,
		): boolean {
		
		if (!payload.billingPaymentFrequencies) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting payment frequencies #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingPaymentFrequencies> = {};
		
		for (const e of payload.billingPaymentFrequencies) {
			newPayload[e.uuid] = e;
		}
		
		
		store.commit('UpdateBillingPaymentFrequenciesRemote', newPayload);
		
		return true;
	}
}

export default {};
