import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { ILabourSubtypeHoliday } from "./LabourSubtypeHoliday";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestLabourSubtypeHolidaysPayload
	extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestLabourSubtypeHolidaysCB extends IIdempotencyResponse {
	labourSubtypeHolidays: Record<string, ILabourSubtypeHoliday>;
}

export class RPCRequestLabourSubtypeHolidays extends RPCMethod {
	public Send(
		payload: IRequestLabourSubtypeHolidaysPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestLabourSubtypeHolidays";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestLabourSubtypeHolidaysCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestLabourSubtypeHolidaysCB
	): boolean {
		if (!payload.labourSubtypeHolidays) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting labour subtype holidays`)
				);
			}
			return false;
		}

		// Default action
		store.commit(
			"UpdateLabourSubtypeHolidaysRemote",
			payload.labourSubtypeHolidays
		);

		return true;
	}
}

export default {};
