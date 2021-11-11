import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IBillingPermissionsGroups } from './BillingPermissionsGroups';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestBillingPermissionsGroupsForCurrentSessionPayload = IIdempotencyRequest
	
export interface IRequestBillingPermissionsGroupsForCurrentSessionCB extends IIdempotencyResponse {
	billingPermissionsGroups: IBillingPermissionsGroups[];
}

export class RPCRequestBillingPermissionsGroupsForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingPermissionsGroupsForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingPermissionsGroupsForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingPermissionsGroupsForCurrentSessionCB';
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest, 
		payload: IRequestBillingPermissionsGroupsForCurrentSessionCB,
		): boolean {
		
		if (!payload.billingPermissionsGroups) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting permissions groups #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingPermissionsGroups> = {};
		
		for (const e of payload.billingPermissionsGroups) {
			newPayload[e.id] = e;
		}
		
		
		store.commit('UpdateBillingPermissionsGroupsRemote', newPayload);
		
		return true;
	}
}

export default {};
