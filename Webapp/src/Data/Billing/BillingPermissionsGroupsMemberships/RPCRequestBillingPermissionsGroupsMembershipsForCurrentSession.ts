import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IBillingPermissionsGroupsMemberships } from './BillingPermissionsGroupsMemberships';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestBillingPermissionsGroupsMembershipsForCurrentSessionPayload = IIdempotencyRequest
	
export interface IRequestBillingPermissionsGroupsMembershipsForCurrentSessionCB extends IIdempotencyResponse {
	billingPermissionsGroupsMemberships: IBillingPermissionsGroupsMemberships[];
}

export class RPCRequestBillingPermissionsGroupsMembershipsForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingPermissionsGroupsMembershipsForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingPermissionsGroupsMembershipsForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingPermissionsGroupsMembershipsForCurrentSessionCB';
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest, 
		payload: IRequestBillingPermissionsGroupsMembershipsForCurrentSessionCB,
		): boolean {
		
		if (!payload.billingPermissionsGroupsMemberships) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting permissions groups memberships #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingPermissionsGroupsMemberships> = {};
		
		for (const e of payload.billingPermissionsGroupsMemberships) {
			newPayload[e.id] = e;
		}
	
		store.commit('UpdateBillingPermissionsGroupsMembershipsRemote', newPayload);
		
		return true;
	}
}

export default {};
