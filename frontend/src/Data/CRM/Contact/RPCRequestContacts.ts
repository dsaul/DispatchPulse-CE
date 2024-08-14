import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IContact } from "./Contact";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestContactsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestContactsCB extends IIdempotencyResponse {
	contacts: Record<string, IContact>;
}

export class RPCRequestContacts extends RPCMethod {
	public Send(payload: IRequestContactsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestContacts";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestContactsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestContactsCB
	): boolean {
		if (!payload.contacts) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting contacts #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit("UpdateContactsRemote", payload.contacts);

		return true;
	}
}

export default {};
