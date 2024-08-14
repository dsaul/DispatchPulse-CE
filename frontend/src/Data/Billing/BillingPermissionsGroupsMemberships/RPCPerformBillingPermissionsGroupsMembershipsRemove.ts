import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformBillingPermissionsGroupsMembershipsRemove extends IIdempotencyRequest {
	billingContactId: string | null;
	permissionsGroupIds: string[] | null;
}

export interface IPerformBillingPermissionsGroupsMembershipsRemoveCB extends IIdempotencyResponse {
	removed: string[];
}

export class RPCPerformBillingPermissionsGroupsMembershipsRemove extends RPCMethod {
	public Send(payload: IPerformBillingPermissionsGroupsMembershipsRemove): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformBillingPermissionsGroupsMembershipsRemove';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformBillingPermissionsGroupsMembershipsRemoveCB';
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest, 
		payload: IPerformBillingPermissionsGroupsMembershipsRemoveCB,
		): boolean {
		
		// Default action
		const newPayload: string[] = [];
		
		for (const e of payload.removed) {
			newPayload.push(e);
		}

		store.commit('DeleteBillingPermissionsGroupsMembershipsRemote', newPayload);
		
		return true;
	}
}

export default {};
