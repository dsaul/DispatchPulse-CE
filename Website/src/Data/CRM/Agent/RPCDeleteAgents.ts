import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteAgentsPayload extends IIdempotencyRequest {
	agentsDelete: string[];
}

export interface IDeleteAgentsCB extends IIdempotencyResponse {
	agentsDelete: string[];
}

export class RPCDeleteAgents extends RPCMethod {
	public Send(payload: IDeleteAgentsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteAgents';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteAgentsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteAgentsCB): boolean {
		
		
		if (payload.agentsDelete && payload.agentsDelete.length > 0) {
			// Default action
			store.commit('DeleteAgentsRemote', payload.agentsDelete);
		}
		
		
		return true;
	}
}

export default {};
