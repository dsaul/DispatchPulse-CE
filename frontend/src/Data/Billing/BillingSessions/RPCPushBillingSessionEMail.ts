import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushBillingSessionEMailPayload extends IIdempotencyRequest {
	eMail: string | null;
}

export interface IPushBillingSessionEMailCB extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPushBillingSessionEMail extends RPCMethod {
	public Send(payload: IPushBillingSessionEMailPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushBillingSessionEMail";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushBillingSessionEMailCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushBillingSessionEMailCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error saving email #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
