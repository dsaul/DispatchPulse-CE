import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteAssignmentsPayload extends IIdempotencyRequest {
	assignmentsDelete: string[];
}

export interface IDeleteAssignmentsCB extends IIdempotencyResponse {
	assignmentsDelete: string[];
}

export class RPCDeleteAssignments extends RPCMethod {
	public Send(payload: IDeleteAssignmentsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteAssignments';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteAssignmentsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteAssignmentsCB): boolean {
		
		if (payload.assignmentsDelete && payload.assignmentsDelete.length > 0) {
			// Default action
			store.commit('DeleteAssignmentsRemote', payload.assignmentsDelete);
		}
		
		return true;
	}
}

export default {};
