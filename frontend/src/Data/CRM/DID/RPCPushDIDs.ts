import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IDID } from "./DID";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushDIDsPayload extends IIdempotencyRequest {
	dids: Record<string, IDID>;
}

export interface IPushDIDsCB extends IIdempotencyResponse {
	dids: string[];
}

export class RPCPushDIDs extends RPCMethod {
	public Send(payload: IPushDIDsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushDIDs";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushDIDsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushDIDsCB
	): boolean {
		console.log("payload", payload);

		if (!payload.hasOwnProperty("dids")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying dids #2.`)
				);
			}
			return false;
		}

		// Default action
		//store.commit('UpdateDIDRemote', payload.dids);

		return true;
	}
}

export default {};
