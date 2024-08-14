import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IEstimatingManHours } from "./EstimatingManHours";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestEstimatingManHoursPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestEstimatingManHoursCB extends IIdempotencyResponse {
	estimatingManHours: Record<string, IEstimatingManHours>;
}

export class RPCRequestEstimatingManHours extends RPCMethod {
	public Send(payload: IRequestEstimatingManHoursPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestEstimatingManHours";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestEstimatingManHoursCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestEstimatingManHoursCB
	): boolean {
		if (!payload.estimatingManHours) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting man hours #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit(
			"UpdateEstimatingManHoursRemote",
			payload.estimatingManHours
		);

		return true;
	}
}

export default {};
