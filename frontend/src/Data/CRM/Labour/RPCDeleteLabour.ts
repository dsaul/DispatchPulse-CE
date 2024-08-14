import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteLabourPayload extends IIdempotencyRequest {
	labourDelete: string[];
}

export interface IDeleteLabourCB extends IIdempotencyResponse {
	labourDelete: string[];
}

export class RPCDeleteLabour extends RPCMethod {
	public Send(payload: IDeleteLabourPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteLabour";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteLabourCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteLabourCB
	): boolean {
		if (payload.labourDelete && payload.labourDelete.length > 0) {
			// Default action
			store.commit("DeleteLabourRemote", payload.labourDelete);
		}

		return true;
	}
}

export default {};
