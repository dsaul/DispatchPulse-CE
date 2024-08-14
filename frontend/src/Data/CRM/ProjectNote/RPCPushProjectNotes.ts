import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";
import { IProjectNote } from "./ProjectNote";

export interface IPushProjectNotesPayload extends IIdempotencyRequest {
	projectNotes: Record<string, IProjectNote>;
}

export interface IPushProjectNotesCB extends IIdempotencyResponse {
	projectNotes: string[];
}

export class RPCPushProjectNotes extends RPCMethod {
	public Send(payload: IPushProjectNotesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushProjectNotes";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushProjectNotesCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushProjectNotesCB
	): boolean {
		if (!payload.projectNotes) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying project notes #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
