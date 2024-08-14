import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { ILabourSubtypeHoliday } from "./LabourSubtypeHoliday";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushLabourSubtypeHolidaysPayload extends IIdempotencyRequest {
	labourSubtypeHolidays: Record<string, ILabourSubtypeHoliday>;
}

export interface IPushLabourSubtypeHolidaysCB extends IIdempotencyResponse {
	labourSubtypeHolidays: string[];
}

export class RPCPushLabourSubtypeHolidays extends RPCMethod {
	public Send(payload: IPushLabourSubtypeHolidaysPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushLabourSubtypeHolidays";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushLabourSubtypeHolidaysCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushLabourSubtypeHolidaysCB
	): boolean {
		if (!payload.labourSubtypeHolidays) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying labour subtype holidays`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
