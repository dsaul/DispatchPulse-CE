import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IProduct } from "./Product";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushProductsPayload extends IIdempotencyRequest {
	products: Record<string, IProduct>;
}

export interface IPushProductsCB extends IIdempotencyResponse {
	products: string[];
}

export class RPCPushProducts extends RPCMethod {
	public Send(payload: IPushProductsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushProducts";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushProductsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushProductsCB
	): boolean {
		if (!payload.products) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying products #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
