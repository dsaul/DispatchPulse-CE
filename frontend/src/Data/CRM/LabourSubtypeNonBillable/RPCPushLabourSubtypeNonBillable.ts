import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { ILabourSubtypeNonBillable } from './LabourSubtypeNonBillable';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushLabourSubtypeNonBillablePayload extends IIdempotencyRequest {
	labourSubtypeNonBillable: Record<string, ILabourSubtypeNonBillable>;
}

export interface IPushLabourSubtypeNonBillableCB extends IIdempotencyResponse {
	labourSubtypeNonBillable: string[];
}


export class RPCPushLabourSubtypeNonBillable extends RPCMethod {
	public Send(payload: IPushLabourSubtypeNonBillablePayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushLabourSubtypeNonBillable';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushLabourSubtypeNonBillableCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushLabourSubtypeNonBillableCB): boolean {
		
		if (!payload.labourSubtypeNonBillable) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modify labour subtype non-billable #2.`));
			}
			return false;
		}
	
		// Default action
		
		return true;
	}
}

export default {};
