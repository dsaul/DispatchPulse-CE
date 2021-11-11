import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IEstimatingManHours } from './EstimatingManHours';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushEstimatingManHoursPayload extends IIdempotencyRequest {
	estimatingManHours: Record<string, IEstimatingManHours>;
}

export interface IPushEstimatingManHoursCB extends IIdempotencyResponse {
	estimatingManHours: string[];
}

export class RPCPushEstimatingManHours extends RPCMethod {
	public Send(payload: IPushEstimatingManHoursPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushEstimatingManHours';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushEstimatingManHoursCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushEstimatingManHoursCB): boolean {
		
		if (!payload.estimatingManHours) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying man hours #2.`));
			}
			return false;
		}
	
		// Default action
		
		return true;
	}
}

export default {};
