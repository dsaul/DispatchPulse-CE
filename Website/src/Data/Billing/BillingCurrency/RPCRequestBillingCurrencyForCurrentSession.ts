import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IBillingCurrency } from './BillingCurrency';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestBillingCurrencyForCurrentSessionPayload = IIdempotencyRequest
	
	export interface IRequestBillingCurrencyForCurrentSessionCB extends IIdempotencyResponse {
		billingCurrency: IBillingCurrency[];
	}

export class RPCRequestBillingCurrencyForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingCurrencyForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingCurrencyForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingCurrencyForCurrentSessionCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestBillingCurrencyForCurrentSessionCB): boolean {
		
		if (!payload.billingCurrency) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting currency #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingCurrency> = {};
		
		for (const e of payload.billingCurrency) {
			newPayload[e.uuid] = e;
		}
		
		store.commit('UpdateBillingCurrencyRemote', newPayload);
		
		return true;
	}
}

export default {};
