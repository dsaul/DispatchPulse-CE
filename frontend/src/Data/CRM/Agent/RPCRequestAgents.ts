import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IAgent } from "./Agent";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestAgentsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestAgentsCB extends IIdempotencyResponse {
	agents: Record<string, IAgent>;
}

export class RPCRequestAgents extends RPCMethod {
	public Send(payload: IRequestAgentsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestAgents";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestAgentsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestAgentsCB
	): boolean {
		if (!payload.agents) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting agents #2.`)
				);
			}

			return false;
		}

		// Default action
		store.commit("UpdateAgentsRemote", payload.agents);

		return true;
	}
}

export default {};
