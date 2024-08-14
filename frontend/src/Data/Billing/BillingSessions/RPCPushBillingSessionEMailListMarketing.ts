import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushBillingSessionEMailListMarketingPayload
	extends IIdempotencyRequest {
	eMailListMarketing: boolean | null;
}

export interface IPushBillingSessionEMailListMarketingCB
	extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPushBillingSessionEMailListMarketing extends RPCMethod {
	public Send(
		payload: IPushBillingSessionEMailListMarketingPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushBillingSessionEMailListMarketing";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushBillingSessionEMailListMarketingCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushBillingSessionEMailListMarketingCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error saving email list marketing #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
