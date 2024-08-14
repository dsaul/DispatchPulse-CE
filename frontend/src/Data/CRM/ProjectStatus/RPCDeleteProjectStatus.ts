import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteProjectStatusPayload extends IIdempotencyRequest {
	projectStatusDelete: string[];
}

export interface IDeleteProjectStatusCB extends IIdempotencyResponse {
	projectStatusDelete: string[];
}

export class RPCDeleteProjectStatus extends RPCMethod {
	public Send(payload: IDeleteProjectStatusPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteProjectStatus";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteProjectStatusCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteProjectStatusCB
	): boolean {
		if (
			payload.projectStatusDelete &&
			payload.projectStatusDelete.length > 0
		) {
			// Default action
			store.commit(
				"DeleteProjectStatusRemote",
				payload.projectStatusDelete
			);
		}

		return true;
	}
}

export default {};
