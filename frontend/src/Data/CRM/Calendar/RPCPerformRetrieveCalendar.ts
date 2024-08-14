import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { guid } from "@/Utility/GlobalTypes";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformRetrieveCalendarPayload extends IIdempotencyRequest {
	calendarId: guid | null;
}

export interface IPerformRetrieveCalendarCB extends IIdempotencyResponse {
	complete: boolean | null;
}

export class RPCPerformRetrieveCalendar extends RPCMethod {
	public Send(payload: IPerformRetrieveCalendarPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformRetrieveCalendar";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformRetrieveCalendarCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformRetrieveCalendarCB
	): boolean {
		if (!payload.complete) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(
						`Error PerformRetrieveCalendarCB RecieveDefaultAction`
					)
				);
			}
			return false;
		}

		// Default action
		//store.commit('UpdateDIDRemote', payload.dids);

		return true;
	}
}

export default {};
