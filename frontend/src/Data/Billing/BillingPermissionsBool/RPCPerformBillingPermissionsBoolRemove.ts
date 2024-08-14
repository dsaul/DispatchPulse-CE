import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformBillingPermissionsBoolRemove extends IIdempotencyRequest {
	billingContactId: guid | null;
	permissionKeys: string[] | null;
}

export interface IPerformBillingPermissionsBoolRemoveCB extends IIdempotencyResponse {
	removed: guid[];
}

export class RPCPerformBillingPermissionsBoolRemove extends RPCMethod {
	public Send(payload: IPerformBillingPermissionsBoolRemove): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformBillingPermissionsBoolRemove';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformBillingPermissionsBoolRemoveCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformBillingPermissionsBoolRemoveCB): boolean {
		
		// Default action
		const newPayload: guid[] = [];
		
		for (const e of payload.removed) {
			newPayload.push(e);
		}

		store.commit('DeleteBillingPermissionsBoolRemote', newPayload);
		
		return true;
	}
}

export default {};
