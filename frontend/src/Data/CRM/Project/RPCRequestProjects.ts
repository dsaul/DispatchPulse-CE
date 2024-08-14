import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IProject } from "./Project";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestProjectsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
	showChildrenOfProjectIdAsWell?: boolean | null;
}

export interface IRequestProjectsCB extends IIdempotencyResponse {
	projects: Record<string, IProject>;
}

export class RPCRequestProjects extends RPCMethod {
	public Send(payload: IRequestProjectsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestProjects";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestProjectsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestProjectsCB
	): boolean {
		if (!payload.projects) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting projects #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit("UpdateProjectsRemote", payload.projects);

		return true;
	}
}

export default {};
