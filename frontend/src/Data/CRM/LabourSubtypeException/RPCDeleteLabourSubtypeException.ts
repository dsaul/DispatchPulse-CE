import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteLabourSubtypeExceptionPayload extends IIdempotencyRequest {
	labourSubtypeExceptionDelete: string[];
}

export interface IDeleteLabourSubtypeExceptionCB extends IIdempotencyResponse {
	labourSubtypeExceptionDelete: string[];
}

export class RPCDeleteLabourSubtypeException extends RPCMethod {
	public Send(payload: IDeleteLabourSubtypeExceptionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteLabourSubtypeException';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteLabourSubtypeExceptionCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteLabourSubtypeExceptionCB): boolean {
		
		if (payload.labourSubtypeExceptionDelete && payload.labourSubtypeExceptionDelete.length > 0) {
			// Default action
			store.commit('DeleteLabourSubtypeHolidaysRemote', payload.labourSubtypeExceptionDelete);
		}
		
		return true;
	}
}

export default {};
