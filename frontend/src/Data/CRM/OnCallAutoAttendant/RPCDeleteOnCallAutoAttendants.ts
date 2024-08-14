import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteOnCallAutoAttendantsPayload
	extends IIdempotencyRequest {
	onCallAutoAttendantsDelete: string[];
}

export interface IDeleteOnCallAutoAttendantsCB extends IIdempotencyResponse {
	onCallAutoAttendantsDelete: string[];
}

export class RPCDeleteOnCallAutoAttendants extends RPCMethod {
	public Send(
		payload: IDeleteOnCallAutoAttendantsPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteOnCallAutoAttendants";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteOnCallAutoAttendantsCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteOnCallAutoAttendantsCB
	): boolean {
		if (
			payload.onCallAutoAttendantsDelete &&
			payload.onCallAutoAttendantsDelete.length > 0
		) {
			// Default action
			store.commit(
				"DeleteOnCallAutoAttendantsRemote",
				payload.onCallAutoAttendantsDelete
			);
		}

		return true;
	}
}

export default {};
