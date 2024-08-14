import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushBillingSessionContactIdPayload
	extends IIdempotencyRequest {
	contactId: string | null;
}

export interface IPushBillingSessionContactIdCB extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPushBillingSessionContactId extends RPCMethod {
	public Send(
		payload: IPushBillingSessionContactIdPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushBillingSessionContactId";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushBillingSessionContactIdCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushBillingSessionContactIdCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error saving session contact id #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
