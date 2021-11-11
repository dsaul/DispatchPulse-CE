import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteLabourSubtypeNonBillablePayload extends IIdempotencyRequest {
	labourSubtypeNonBillableDelete: string[];
}

export interface IDeleteLabourSubtypeNonBillableCB extends IIdempotencyResponse {
	labourSubtypeNonBillableDelete: string[];
}

export class RPCDeleteLabourSubtypeNonBillable extends RPCMethod {
	public Send(payload: IDeleteLabourSubtypeNonBillablePayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteLabourSubtypeNonBillable';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteLabourSubtypeNonBillableCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteLabourSubtypeNonBillableCB): boolean {
		
		if (payload.labourSubtypeNonBillableDelete && payload.labourSubtypeNonBillableDelete.length > 0) {
			// Default action
			store.commit('DeleteLabourSubtypeNonBillableRemote', payload.labourSubtypeNonBillableDelete);
		}
		
		return true;
	}
}

export default {};
