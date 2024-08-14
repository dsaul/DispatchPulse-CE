import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IProjectNote } from "./ProjectNote";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestProjectNotesPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
	limitToProjectId?: string | null;
	showChildrenOfProjectIdAsWell?: boolean | null;
}

export interface IRequestProjectNotesCB extends IIdempotencyResponse {
	projectNotes: Record<string, IProjectNote>;
}

export class RPCRequestProjectNotes extends RPCMethod {
	public Send(payload: IRequestProjectNotesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestProjectNotes";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestProjectNotesCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestProjectNotesCB
	): boolean {
		if (!payload.projectNotes) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting project notes #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit("UpdateProjectNotesRemote", payload.projectNotes);

		return true;
	}
}

export default {};
