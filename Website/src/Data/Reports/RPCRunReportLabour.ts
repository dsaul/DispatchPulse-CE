import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRunReportLabourPayload extends IIdempotencyRequest {
	runOnAllLabour: boolean;
	agentId: guid | null;
	projectId: guid | null;
	startISO8601: string | null;
	endISO8601: string | null;
	includeLabourForOtherProjectsWithMatchingAddresses: boolean;
}

export interface IRunReportLabourCB extends IIdempotencyResponse {
	taskId: guid | null;
}

export class RPCRunReportLabour extends RPCMethod {
	public Send(payload: IRunReportLabourPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RunReportLabour';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RunReportLabourCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRunReportLabourCB): boolean {
		
		
		return true;
	}
}

export default {};
