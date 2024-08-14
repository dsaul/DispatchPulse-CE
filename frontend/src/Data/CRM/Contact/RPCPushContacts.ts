import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IContact } from "./Contact";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushContactsPayload extends IIdempotencyRequest {
	contacts: Record<string, IContact>;
}

export interface IPushContactsCB extends IIdempotencyResponse {
	contacts: string[];
}

export class RPCPushContacts extends RPCMethod {
	public Send(payload: IPushContactsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushContacts";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushContactsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushContactsCB
	): boolean {
		if (!payload.contacts) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying contacts #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
