import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformDeleteBillingContact extends IIdempotencyRequest {
	billingContacts: guid[];
}

export interface IPerformDeleteBillingContactCB extends IIdempotencyResponse {
	billingContactsDelete: guid[];
}

export class RPCPerformDeleteBillingContact extends RPCMethod {
	public Send(payload: IPerformDeleteBillingContact): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformDeleteBillingContact';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformDeleteBillingContactCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformDeleteBillingContactCB): boolean {
		
		// Default action
		store.commit('DeleteBillingContactsRemote', payload.billingContactsDelete);
		
		return true;
	}
}

export default {};
