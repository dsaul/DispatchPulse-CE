import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRunReportAssignmentsPayload extends IIdempotencyRequest {
	runOnAllAssignments: boolean;
	assignmentIds: guid[];
}

export interface IRunReportAssignmentsCB extends IIdempotencyResponse {
	taskId: guid | null;
}

export class RPCRunReportAssignments extends RPCMethod {
	public Send(payload: IRunReportAssignmentsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RunReportAssignments';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RunReportAssignmentsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRunReportAssignmentsCB): boolean {
		
		
		return true;
	}
}

export default {};
