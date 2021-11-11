import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteEstimatingManHoursPayload extends IIdempotencyRequest {
	estimatingManHoursDelete: string[];
}

export interface IDeleteEstimatingManHoursCB extends IIdempotencyResponse {
	estimatingManHoursDelete: string[];
}

export class RPCDeleteEstimatingManHours extends RPCMethod {
	public Send(payload: IDeleteEstimatingManHoursPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteEstimatingManHours';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteEstimatingManHoursCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteEstimatingManHoursCB): boolean {
		
		if (payload.estimatingManHoursDelete && payload.estimatingManHoursDelete.length > 0) {
			// Default action
			store.commit('DeleteEstimatingManHoursRemote', payload.estimatingManHoursDelete);
		}
		
		return true;
	}
}

export default {};
