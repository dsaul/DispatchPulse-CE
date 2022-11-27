import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformCheckDIDPBXRegisteredPayload extends IIdempotencyRequest {
	did: string | null;
	billingCompanyId: guid | null;
}

export interface IPerformCheckDIDPBXRegisteredCB extends IIdempotencyResponse {
	isRegistered: boolean | null;
	isRegisteredToDifferentCompany: boolean | null;
	isRegisteredToUnknownCompany: boolean | null;
}

export class RPCPerformCheckDIDPBXRegistered extends RPCMethod {
	public Send(payload: IPerformCheckDIDPBXRegisteredPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformCheckDIDPBXRegistered';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformCheckDIDPBXRegisteredCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformCheckDIDPBXRegisteredCB): boolean {
		
		if (!payload.hasOwnProperty('isRegistered')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error PerformCheckDIDPBXRegisteredCB RecieveDefaultAction`));
			}
			return false;
		}

		// Default action
		//store.commit('UpdateDIDRemote', payload.dids);
		
		return true;
	}
}

export default {};
