import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteProductsPayload extends IIdempotencyRequest {
	productsDelete: string[];
}

export interface IDeleteProductsCB extends IIdempotencyResponse {
	productsDelete: string[];
}

export class RPCDeleteProducts extends RPCMethod {
	public Send(payload: IDeleteProductsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteProducts";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteProductsCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteProductsCB
	): boolean {
		if (payload.productsDelete && payload.productsDelete.length > 0) {
			// Default action
			store.commit("DeleteProductsRemote", payload.productsDelete);
		}

		return true;
	}
}

export default {};
