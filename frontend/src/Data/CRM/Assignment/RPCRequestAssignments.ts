import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IAssignment } from "./Assignment";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestAssignmentsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;

	limitToSessionAgent?: boolean | null;
	limitToOpen?: boolean | null;
	limitToClosed?: boolean | null;
	limitToTodayOrEarlier?: boolean | null;
	limitToTasksWithNoStartTime?: boolean | null;
	limitToProjectId?: string | null;
	limitToAgentId?: string | null;
	limitToPastDue?: boolean | null;
	limitToUnassigned?: boolean | null;
	limitToDueWithNoLabour?: boolean | null;
	limitToBillableReview?: boolean | null;

	filterAssignmentsWithNoStartTime?: boolean | null;
	showChildrenOfProjectIdAsWell?: boolean | null;
}

export interface IRequestAssignmentsCB extends IIdempotencyResponse {
	assignments: Record<string, IAssignment>;
}

export class RPCRequestAssignments extends RPCMethod {
	public Send(payload: IRequestAssignmentsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestAssignments";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestAssignmentsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestAssignmentsCB
	): boolean {
		if (!payload.assignments) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting assignments #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit("UpdateAssignmentsRemote", payload.assignments);

		return true;
	}
}

export default {};
