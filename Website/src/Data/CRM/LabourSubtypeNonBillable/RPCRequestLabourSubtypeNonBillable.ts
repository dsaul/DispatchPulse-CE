import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { ILabourSubtypeNonBillable } from './LabourSubtypeNonBillable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestLabourSubtypeNonBillablePayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestLabourSubtypeNonBillableCB extends IIdempotencyResponse {
	labourSubtypeNonBillable: Record<string, ILabourSubtypeNonBillable>;
}

export class RPCRequestLabourSubtypeNonBillable extends RPCMethod {
	public Send(payload: IRequestLabourSubtypeNonBillablePayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestLabourSubtypeNonBillable';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestLabourSubtypeNonBillableCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestLabourSubtypeNonBillableCB): boolean {
		
		if (!payload.labourSubtypeNonBillable) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting labour subtype non-billable #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateLabourSubtypeNonBillableRemote', payload.labourSubtypeNonBillable);
		
		return true;
	}
}

export default {};
