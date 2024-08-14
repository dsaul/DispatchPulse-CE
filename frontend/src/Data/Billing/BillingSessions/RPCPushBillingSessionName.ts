import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushBillingSessionNamePayload extends IIdempotencyRequest {
	fullName: string | null;
}

export interface IPushBillingSessionNameCB extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPushBillingSessionName extends RPCMethod {
	public Send(payload: IPushBillingSessionNamePayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushBillingSessionName";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushBillingSessionNameCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushBillingSessionNameCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error saving session name #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
