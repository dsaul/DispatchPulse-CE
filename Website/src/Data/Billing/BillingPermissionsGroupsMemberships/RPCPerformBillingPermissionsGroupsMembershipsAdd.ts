import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformBillingPermissionsGroupsMembershipsAdd extends IIdempotencyRequest {
	billingContactId: guid | null;
	permissionsGroupIds: guid[] | null;
}

export type IPerformBillingPermissionsGroupsMembershipsAddCB = IIdempotencyResponse;

export class RPCPerformBillingPermissionsGroupsMembershipsAdd extends RPCMethod {
	public Send(payload: IPerformBillingPermissionsGroupsMembershipsAdd): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformBillingPermissionsGroupsMembershipsAdd';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformBillingPermissionsGroupsMembershipsAddCB';
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest, 
		payload: IPerformBillingPermissionsGroupsMembershipsAddCB,
		): boolean {
		
		if (payload.isError) {
	
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(new Error(`Error adding group membership: ${payload.errorMessage}`));
			}
			return false;
		}
	
		// Default action
		
		return true;
	}
}

export default {};
