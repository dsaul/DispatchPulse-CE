import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IBillingPackages } from './BillingPackages';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestBillingPackagesForCurrentSessionPayload = IIdempotencyRequest
	
export interface IRequestBillingPackagesForCurrentSessionCB extends IIdempotencyResponse {
	billingPackages: IBillingPackages[];
}


export class RPCRequestBillingPackagesForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingPackagesForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingPackagesForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingPackagesForCurrentSessionCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestBillingPackagesForCurrentSessionCB): boolean {
		
		if (!payload.billingPackages) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting packages #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingPackages> = {};
		
		for (const e of payload.billingPackages) {
			newPayload[e.uuid] = e;
		}
		
		store.commit('UpdateBillingPackagesRemote', newPayload);
		
		return true;
	}
}

export default {};
