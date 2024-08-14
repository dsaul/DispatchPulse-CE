import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushBillingSessionPhonePayload extends IIdempotencyRequest {
	phone: string | null;
}

export interface IPushBillingSessionPhoneCB extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPushBillingSessionPhone extends RPCMethod {
	public Send(payload: IPushBillingSessionPhonePayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushBillingSessionPhone";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushBillingSessionPhoneCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushBillingSessionPhoneCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error saving phone number #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
