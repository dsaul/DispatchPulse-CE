import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteMaterialsPayload extends IIdempotencyRequest {
	materialsDelete: string[];
}

export interface IDeleteMaterialsCB extends IIdempotencyResponse {
	materialsDelete: string[];
}

export class RPCDeleteMaterials extends RPCMethod {
	public Send(payload: IDeleteMaterialsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteMaterials';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteMaterialsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteMaterialsCB): boolean {
		
		if (payload.materialsDelete && payload.materialsDelete.length > 0) {
			// Default action
			store.commit('DeleteMaterialsRemote', payload.materialsDelete);
		}
		
		return true;
	}
}

export default {};
