import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import { guid } from "@/Utility/GlobalTypes";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformPBXDeRegisterDIDPayload extends IIdempotencyRequest {
	did: string | null;
	billingCompanyId: guid | null;
}

export interface IPerformPBXDeRegisterDIDCB extends IIdempotencyResponse {
	complete: boolean | null;
}

export class RPCPerformPBXDeRegisterDID extends RPCMethod {
	public Send(payload: IPerformPBXDeRegisterDIDPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformPBXDeRegisterDID";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformPBXDeRegisterDIDCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformPBXDeRegisterDIDCB
	): boolean {
		if (!payload.complete) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(
						`Error PerformPBXDeRegisterDIDCB RecieveDefaultAction`
					)
				);
			}
			return false;
		}

		// Default action
		//store.commit('UpdateDIDRemote', payload.dids);

		return true;
	}
}

export default {};
