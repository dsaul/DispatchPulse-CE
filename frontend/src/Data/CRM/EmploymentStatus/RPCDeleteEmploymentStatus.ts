import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteAgentsEmploymentStatusPayload
	extends IIdempotencyRequest {
	agentsEmploymentStatusDelete: string[];
}

export interface IDeleteAgentsEmploymentStatusCB extends IIdempotencyResponse {
	agentsEmploymentStatusDelete: string[];
}

export class RPCDeleteAgentsEmploymentStatus extends RPCMethod {
	public Send(
		payload: IDeleteAgentsEmploymentStatusPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteAgentsEmploymentStatus";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteAgentsEmploymentStatusRemote";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteAgentsEmploymentStatusCB
	): boolean {
		if (
			payload.agentsEmploymentStatusDelete &&
			payload.agentsEmploymentStatusDelete.length > 0
		) {
			// Default action
			store.commit(
				"DeleteAgentsEmploymentStatusRemote",
				payload.agentsEmploymentStatusDelete
			);
		}

		return true;
	}
}

export default {};
