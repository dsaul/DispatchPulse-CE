import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { ILabourSubtypeException } from './LabourSubtypeException';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestLabourSubtypeExceptionPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestLabourSubtypeExceptionCB extends IIdempotencyResponse {
	labourSubtypeException: Record<string, ILabourSubtypeException>;
}

export class RPCRequestLabourSubtypeException extends RPCMethod {
	public Send(payload: IRequestLabourSubtypeExceptionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestLabourSubtypeException';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestLabourSubtypeExceptionCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestLabourSubtypeExceptionCB): boolean {
		
		if (!payload.labourSubtypeException) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting labour subtype exception #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateLabourSubtypeExceptionRemote', payload.labourSubtypeException);
		
		return true;
	}
}

export default {};
