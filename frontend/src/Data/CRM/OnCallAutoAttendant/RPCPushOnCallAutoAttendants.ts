import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IOnCallAutoAttendant } from './OnCallAutoAttendant';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushOnCallAutoAttendantsPayload extends IIdempotencyRequest {
	onCallAutoAttendants: { [id: string]: IOnCallAutoAttendant; };
}

export interface IPushOnCallAutoAttendantsCB extends IIdempotencyResponse {
	onCallAutoAttendants: string[];
}

export class RPCPushOnCallAutoAttendants extends RPCMethod {
	public Send(payload: IPushOnCallAutoAttendantsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushOnCallAutoAttendants';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushOnCallAutoAttendantsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushOnCallAutoAttendantsCB): boolean {
		
		if (!payload.hasOwnProperty('onCallAutoAttendants')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying onCallAutoAttendants #2.`));
			}
			return false;
		}

		// Default action
		//store.commit('UpdateOnCallAutoAttendantRemote', payload.onCallAutoAttendants);
		
		return true;
	}
}

export default {};
