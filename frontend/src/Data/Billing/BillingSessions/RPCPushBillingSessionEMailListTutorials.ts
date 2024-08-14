import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushBillingSessionEMailListTutorialsPayload
	extends IIdempotencyRequest {
	eMailListTutorials: boolean | null;
}

export interface IPushBillingSessionEMailListTutorialsCB
	extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPushBillingSessionEMailListTutorials extends RPCMethod {
	public Send(
		payload: IPushBillingSessionEMailListTutorialsPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushBillingSessionEMailListTutorials";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushBillingSessionEMailListTutorialsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushBillingSessionEMailListTutorialsCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error saving email list tutorials #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
