import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteProjectsPayload extends IIdempotencyRequest {
	projectsDelete: string[];
}

export interface IDeleteProjectsCB extends IIdempotencyResponse {
	projectsDelete: string[];
}

export class RPCDeleteProjects extends RPCMethod {
	public Send(payload: IDeleteProjectsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteProjects";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteProjectsCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteProjectsCB
	): boolean {
		if (payload.projectsDelete && payload.projectsDelete.length > 0) {
			// Default action
			store.commit("DeleteProjectsRemote", payload.projectsDelete);
		}

		return true;
	}
}

export default {};
