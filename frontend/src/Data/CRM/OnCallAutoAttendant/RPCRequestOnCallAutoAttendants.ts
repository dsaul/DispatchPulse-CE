import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import store from "@/plugins/store/store";
import { IOnCallAutoAttendant } from "./OnCallAutoAttendant";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestOnCallAutoAttendantsPayload
	extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestOnCallAutoAttendantsCB extends IIdempotencyResponse {
	onCallAutoAttendants: { [id: string]: IOnCallAutoAttendant };
}

export class RPCRequestOnCallAutoAttendants extends RPCMethod {
	public Send(
		payload: IRequestOnCallAutoAttendantsPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestOnCallAutoAttendants";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestOnCallAutoAttendantsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestOnCallAutoAttendantsCB
	): boolean {
		if (!payload.hasOwnProperty("onCallAutoAttendants")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting autoAttendants #2.`)
				);
			}

			return false;
		}

		// Default action
		store.commit(
			"UpdateOnCallAutoAttendantsRemote",
			payload.onCallAutoAttendants
		);

		return true;
	}
}

export default {};
