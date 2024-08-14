import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteDIDsPayload extends IIdempotencyRequest {
	didsDelete: string[];
}

export interface IDeleteDIDsCB extends IIdempotencyResponse {
	didsDelete: string[];
}

export class RPCDeleteDIDs extends RPCMethod {
	public Send(payload: IDeleteDIDsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteDIDs";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteDIDsCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteDIDsCB
	): boolean {
		if (payload.didsDelete && payload.didsDelete.length > 0) {
			// Default action
			store.commit("DeleteDIDsRemote", payload.didsDelete);
		}

		return true;
	}
}

export default {};
