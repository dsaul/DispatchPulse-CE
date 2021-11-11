import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteLabourSubtypeHolidaysPayload extends IIdempotencyRequest {
	labourSubtypeHolidaysDelete: string[];
}

export interface IDeleteLabourSubtypeHolidaysCB extends IIdempotencyResponse {
	labourSubtypeHolidaysDelete: string[];
}

export class RPCDeleteLabourSubtypeHolidays extends RPCMethod {
	public Send(payload: IDeleteLabourSubtypeHolidaysPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteLabourSubtypeHolidays';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteLabourSubtypeHolidaysCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteLabourSubtypeHolidaysCB): boolean {
		
		if (payload.labourSubtypeHolidaysDelete && payload.labourSubtypeHolidaysDelete.length > 0) {
			// Default action
			store.commit('DeleteLabourSubtypeHolidaysRemote', payload.labourSubtypeHolidaysDelete);
		}
		
		return true;
	}
}

export default {};
