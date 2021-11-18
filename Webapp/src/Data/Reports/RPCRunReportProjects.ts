import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRunReportProjectsPayload extends IIdempotencyRequest {
	projectIds: guid[];
	includeCompanies: boolean;
	includeContacts: boolean;
	includeSchedule: boolean;
	includeNotes: boolean;
	includeLabour: boolean;
	includeMaterials: boolean;
}

export interface IRunReportProjectsCB extends IIdempotencyResponse {
	taskId: guid | null;
}

export class RPCRunReportProjects extends RPCMethod {
	public Send(payload: IRunReportProjectsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RunReportProjects';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RunReportProjectsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRunReportProjectsCB): boolean {
		
		
		return true;
	}
}

export default {};
