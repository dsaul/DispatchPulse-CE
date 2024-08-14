import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { ICalendar } from './Calendar';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPushCalendarsPayload extends IIdempotencyRequest {
	calendars: Record<string, ICalendar>;
}

export interface IPushCalendarsCB extends IIdempotencyResponse {
	calendars: string[];
}

export class RPCPushCalendars extends RPCMethod {
	public Send(payload: IPushCalendarsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushCalendars';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushCalendarsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushCalendarsCB): boolean {
		
		if (!payload.hasOwnProperty('calendars')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying calendars #2.`));
			}
			return false;
		}

		// Default action
		//store.commit('UpdateCalendarRemote', payload.calendars);
		
		return true;
	}
}

export default {};
