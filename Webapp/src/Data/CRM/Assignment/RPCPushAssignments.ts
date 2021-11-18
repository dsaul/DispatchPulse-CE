import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IAssignment } from './Assignment';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPushAssignmentsPayload extends IIdempotencyRequest {
	assignments: Record<string, IAssignment>;
}

export interface IPushAssignmentsCB extends IIdempotencyResponse {
	assignments: string[];
}

export class RPCPushAssignments extends RPCMethod {
	public Send(payload: IPushAssignmentsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushAssignments';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushAssignmentsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushAssignmentsCB): boolean {
		
		if (!payload.assignments) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying assignments #2.`));
			}
			return false;
		}
	
		// Default action
		
		return true;
	}
}

export default {};
