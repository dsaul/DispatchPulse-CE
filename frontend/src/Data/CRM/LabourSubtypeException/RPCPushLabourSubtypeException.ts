import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { ILabourSubtypeException } from "./LabourSubtypeException";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushLabourSubtypeExceptionPayload
	extends IIdempotencyRequest {
	labourSubtypeException: Record<string, ILabourSubtypeException>;
}

export interface IPushLabourSubtypeExceptionCB extends IIdempotencyResponse {
	labourSubtypeException: string[];
}

export class RPCPushLabourSubtypeException extends RPCMethod {
	public Send(
		payload: IPushLabourSubtypeExceptionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushLabourSubtypeException";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushLabourSubtypeExceptionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushLabourSubtypeExceptionCB
	): boolean {
		if (!payload.labourSubtypeException) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying labour subtype exception #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
