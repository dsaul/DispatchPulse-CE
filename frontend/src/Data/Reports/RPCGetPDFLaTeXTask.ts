import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IGetPDFLaTeXTaskPayload extends IIdempotencyRequest {
	taskId: guid;
}

export interface IGetPDFLaTeXTaskCB extends IIdempotencyResponse {
	isCompleted: boolean;
	status: string | null;
	tempLink: string | null;
}
export class RPCGetPDFLaTeXTask extends RPCMethod {
	public Send(payload: IGetPDFLaTeXTaskPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'GetPDFLaTeXTask';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'GetPDFLaTeXTaskCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IGetPDFLaTeXTaskCB): boolean {
		
		
		return true;
	}
}

export default {};
