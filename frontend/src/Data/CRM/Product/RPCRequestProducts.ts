import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IProduct } from './Product';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestProductsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestProductsCB extends IIdempotencyResponse {
	products: Record<string, IProduct>;
}
export class RPCRequestProducts extends RPCMethod {
	public Send(payload: IRequestProductsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestProducts';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestProductsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestProductsCB): boolean {
		
		if (!payload.products) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting products #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateProductsRemote', payload.products);
		
		return true;
	}
}

export default {};
