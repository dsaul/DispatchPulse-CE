import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IAgent } from './Agent';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushAgentsPayload extends IIdempotencyRequest {
	agents: Record<string, IAgent>;
}

export interface IPushAgentsCB extends IIdempotencyResponse {
	agents: string[];
}

export class RPCPushAgents extends RPCMethod {
	public Send(payload: IPushAgentsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushAgents';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushAgentsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushAgentsCB): boolean {
		
		if (!payload.agents) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying agents #2.`));
			}
			return false;
		}

		// Default action
		//store.commit('UpdateAgentsRemote', payload.agents);
		
		return true;
	}
}

export default {};
