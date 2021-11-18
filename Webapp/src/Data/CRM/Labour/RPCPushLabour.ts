import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { ILabour } from './Labour';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushLabourPayload extends IIdempotencyRequest {
	labour: Record<string, ILabour>;
}

export interface IPushLabourCB extends IIdempotencyResponse {
	labour: string[];
}

export class RPCPushLabour extends RPCMethod {
	public Send(payload: IPushLabourPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushLabour';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushLabourCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushLabourCB): boolean {
		
		if (!payload.labour) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying labour #2.`));
			}
			return false;
		}
	
		// Default action
		
		return true;
	}
}

export default {};
