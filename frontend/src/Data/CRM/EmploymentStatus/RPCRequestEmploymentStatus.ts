import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IEmploymentStatus } from './EmploymentStatus';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestAgentsEmploymentStatusPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestAgentsEmploymentStatusCB extends IIdempotencyResponse {
	agentsEmploymentStatus: Record<string, IEmploymentStatus>;
}

export class RPCRequestAgentsEmploymentStatus extends RPCMethod {
	public Send(payload: IRequestAgentsEmploymentStatusPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestAgentsEmploymentStatus';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestAgentsEmploymentStatusCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestAgentsEmploymentStatusCB): boolean {
		
		if (!payload.agentsEmploymentStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error deleting agents employment status #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateAgentsEmploymentStatusRemote', payload.agentsEmploymentStatus);
		
		return true;
	}
}

export default {};
