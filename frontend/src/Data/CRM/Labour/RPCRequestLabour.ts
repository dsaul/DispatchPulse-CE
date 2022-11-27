import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { ILabour } from './Labour';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestLabourPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
	
	limitToSessionAgent?: boolean;
	limitToProjectId?: string | null;
	limitToAgentId?: string | null;
	limitToAssignmentId?: string | null;
	limitToActiveAndToday?: boolean | null;
	showChildrenOfProjectIdAsWell?: boolean | null;
}

export interface IRequestLabourCB extends IIdempotencyResponse {
	labour: Record<string, ILabour>;
}

export class RPCRequestLabour extends RPCMethod {
	public Send(payload: IRequestLabourPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestLabour';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestLabourCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestLabourCB): boolean {
		
		if (!payload.labour) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting labour #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateLabourRemote', payload.labour);
		
		return true;
	}
}

export default {};
