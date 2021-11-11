import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushBillingSessionAgentIdPayload extends IIdempotencyRequest {
	agentId: string | null;
}

export interface IPushBillingSessionAgentIdCB extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPushBillingSessionAgentId extends RPCMethod {
	public Send(payload: IPushBillingSessionAgentIdPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushBillingSessionAgentId';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushBillingSessionAgentIdCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushBillingSessionAgentIdCB): boolean {
		
		if (!payload.hasOwnProperty('saved')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error updating session agent id #2.`));
			}
			return false;
		}
	
		// Default action
		
	
		
		return true;
	}
}

export default {};
