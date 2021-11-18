import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRunReportOnCallResponder30DayPayload = IIdempotencyRequest
	
export interface IRunReportOnCallResponder30DayCB extends IIdempotencyResponse {
	taskId: guid | null;
}

export class RPCRunReportOnCallResponder30Day extends RPCMethod {
	public Send(payload: IRunReportOnCallResponder30DayPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RunReportOnCallResponder30Day';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RunReportOnCallResponder30DayCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRunReportOnCallResponder30DayCB): boolean {
		
		
		return true;
	}
}

export default {};
