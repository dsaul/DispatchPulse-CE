import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";
import { IProjectStatus } from "./ProjectStatus";

export interface IPushProjectStatusPayload extends IIdempotencyRequest {
	projectStatus: Record<string, IProjectStatus>;
}

export interface IPushProjectStatusCB extends IIdempotencyResponse {
	projectStatus: string[];
}

export class RPCPushProjectStatus extends RPCMethod {
	public Send(payload: IPushProjectStatusPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushProjectStatus";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushProjectStatusCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushProjectStatusCB
	): boolean {
		if (!payload.projectStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying project status #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
