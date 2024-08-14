import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteAssignmentStatusPayload extends IIdempotencyRequest {
	assignmentStatusDelete: string[];
}

export interface IDeleteAssignmentStatusCB extends IIdempotencyResponse {
	assignmentStatusDelete: string[];
}

export class RPCDeleteAssignmentStatus extends RPCMethod {
	public Send(payload: IDeleteAssignmentStatusPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteAssignmentStatus";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteAssignmentStatusCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteAssignmentStatusCB
	): boolean {
		if (
			payload.assignmentStatusDelete &&
			payload.assignmentStatusDelete.length > 0
		) {
			// Default action
			store.commit(
				"DeleteAssignmentStatusRemote",
				payload.assignmentStatusDelete
			);
		}

		return true;
	}
}

export default {};
