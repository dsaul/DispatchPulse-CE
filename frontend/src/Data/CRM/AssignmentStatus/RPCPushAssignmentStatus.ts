import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IAssignmentStatus } from './AssignmentStatus';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushAssignmentStatusPayload extends IIdempotencyRequest {
	assignmentStatus: Record<string, IAssignmentStatus>;
}

export interface IPushAssignmentStatusCB extends IIdempotencyResponse {
	assignmentStatus: string[];
}

export class RPCPushAssignmentStatus extends RPCMethod {
	public Send(payload: IPushAssignmentStatusPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushAssignmentStatus';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushAssignmentStatusCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushAssignmentStatusCB): boolean {
		
		if (!payload.assignmentStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying assignment status #2.`));
			}
			return false;
		}
	
		// Default action
		
		return true;
	}
}

export default {};
