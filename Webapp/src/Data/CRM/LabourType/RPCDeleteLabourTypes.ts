import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteLabourTypesPayload extends IIdempotencyRequest {
	labourTypesDelete: string[];
}

export interface IDeleteLabourTypesCB extends IIdempotencyResponse {
	labourTypesDelete: string[];
}


export class RPCDeleteLabourTypes extends RPCMethod {
	public Send(payload: IDeleteLabourTypesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteLabourTypes';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteLabourTypesCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteLabourTypesCB): boolean {
		
		if (payload.labourTypesDelete && payload.labourTypesDelete.length > 0) {
			// Default action
			store.commit('DeleteLabourTypesRemote', payload.labourTypesDelete);
		}
		
		return true;
	}
}

export default {};
