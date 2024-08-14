import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IProject } from "./Project";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushProjectsPayload extends IIdempotencyRequest {
	projects: Record<string, IProject>;
}

export interface IPushProjectsCB extends IIdempotencyResponse {
	projects: string[];
}

export class RPCPushProjects extends RPCMethod {
	public Send(payload: IPushProjectsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushProjects";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushProjectsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushProjectsCB
	): boolean {
		if (!payload.projects) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying projects #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
