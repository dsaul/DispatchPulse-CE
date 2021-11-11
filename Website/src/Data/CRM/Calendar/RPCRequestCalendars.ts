import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { ICalendar } from './Calendar';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestCalendarsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestCalendarsCB extends IIdempotencyResponse {
	calendars: Record<string, ICalendar>;
}

export class RPCRequestCalendars extends RPCMethod {
	public Send(payload: IRequestCalendarsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestCalendars';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestCalendarsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestCalendarsCB): boolean {
		
		if (!payload.hasOwnProperty('calendars')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(new Error(`Error requesting calendars #2.`));
				
			}
			
			return false;
		}

		// Default action
		store.commit('UpdateCalendarsRemote', payload.calendars);
		
		return true;
	}
}

export default {};
