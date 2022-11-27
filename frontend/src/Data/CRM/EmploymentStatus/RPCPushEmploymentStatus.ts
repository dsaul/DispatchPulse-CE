import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { IEmploymentStatus } from './EmploymentStatus';


export interface IPushAgentsEmploymentStatusPayload extends IIdempotencyRequest {
	agentsEmploymentStatus: Record<string, IEmploymentStatus>;
}

export interface IPushAgentsEmploymentStatusCB extends IIdempotencyResponse {
	agentsEmploymentStatus: string[];
}

export class RPCPushAgentsEmploymentStatus extends RPCMethod {
	public Send(payload: IPushAgentsEmploymentStatusPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushAgentsEmploymentStatus';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushAgentsEmploymentStatusCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushAgentsEmploymentStatusCB): boolean {
		
		if (!payload.agentsEmploymentStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying agents employment status #2.`));
			}
			return false;
		}
	
		// Default action
		
		return true;
	}
}

export default {};
