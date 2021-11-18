import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteCalendarsPayload extends IIdempotencyRequest {
	calendarsDelete: string[];
}

export interface IDeleteCalendarsCB extends IIdempotencyResponse {
	calendarsDelete: string[];
}

export class RPCDeleteCalendars extends RPCMethod {
	public Send(payload: IDeleteCalendarsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteCalendars';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteCalendarsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteCalendarsCB): boolean {
		
		
		if (payload.calendarsDelete && payload.calendarsDelete.length > 0) {
			// Default action
			store.commit('DeleteCalendarsRemote', payload.calendarsDelete);
		}
		
		
		return true;
	}
}

export default {};
