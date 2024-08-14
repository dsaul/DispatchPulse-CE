import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformPBXRegisterDIDPayload extends IIdempotencyRequest {
	did: string | null;
	billingCompanyId: guid | null;
	didPassword: string;
}

export interface IPerformPBXRegisterDIDCB extends IIdempotencyResponse {
	complete: boolean | null;
}

export class RPCPerformPBXRegisterDID extends RPCMethod {
	public Send(payload: IPerformPBXRegisterDIDPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformPBXRegisterDID';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformPBXRegisterDIDCB';
	}
	public RecievefaultAction(rtr: IRoundTripRequest, payload: IPerformPBXRegisterDIDCB): boolean {
		
		if (!payload.complete) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error PerformPBXRegisterDIDCB RecieveDefaultAction`));
			}
			return false;
		}

		// Default action
		//store.commit('UpdateDIDRemote', payload.dids);
		
		return true;
	}
}

export default {};
