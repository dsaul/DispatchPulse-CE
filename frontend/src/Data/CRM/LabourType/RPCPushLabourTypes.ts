import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { ILabourType } from "./LabourType";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushLabourTypesPayload extends IIdempotencyRequest {
	labourTypes: Record<string, ILabourType>;
}

export interface IPushLabourTypesCB extends IIdempotencyResponse {
	labourTypes: string[];
}

export class RPCPushLabourTypes extends RPCMethod {
	public Send(payload: IPushLabourTypesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushLabourTypes";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushLabourTypesCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushLabourTypesCB
	): boolean {
		if (!payload.labourTypes) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying labour types #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
