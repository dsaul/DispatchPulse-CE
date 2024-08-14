import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { IRunReportAssignmentsCB } from './RPCRunReportAssignments';

export interface IRunReportContactsPayload extends IIdempotencyRequest {
	runOnAllContacts: boolean;
	contactIds: guid[];
}

export interface IRunReportContactsCB extends IIdempotencyResponse {
	taskId: guid | null;
}

export class RPCRunReportContacts extends RPCMethod {
	public Send(payload: IRunReportContactsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RunReportContacts';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RunReportContactsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRunReportAssignmentsCB): boolean {
		
		
		return true;
	}
}

export default {};
