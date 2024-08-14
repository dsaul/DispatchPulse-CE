import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IDID } from "./DID";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestDIDsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestDIDsCB extends IIdempotencyResponse {
	dids: Record<string, IDID>;
}

export class RPCRequestDIDs extends RPCMethod {
	public Send(payload: IRequestDIDsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestDIDs";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestDIDsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestDIDsCB
	): boolean {
		if (!payload.hasOwnProperty("dids")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting dids #2.`)
				);
			}

			return false;
		}

		// Default action
		store.commit("UpdateDIDsRemote", payload.dids);

		return true;
	}
}

export default {};
