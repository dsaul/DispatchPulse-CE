import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IAssignmentStatus } from "./AssignmentStatus";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestAssignmentStatusPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestAssignmentStatusCB extends IIdempotencyResponse {
	assignmentStatus: Record<string, IAssignmentStatus>;
}

export class RPCRequestAssignmentStatus extends RPCMethod {
	public Send(payload: IRequestAssignmentStatusPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestAssignmentStatus";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestAssignmentStatusCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestAssignmentStatusCB
	): boolean {
		if (!payload.assignmentStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting assignment status #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit("UpdateAssignmentStatusRemote", payload.assignmentStatus);

		return true;
	}
}

export default {};
