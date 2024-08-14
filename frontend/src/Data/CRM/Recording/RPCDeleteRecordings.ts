import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteRecordingsPayload extends IIdempotencyRequest {
	recordingsDelete: string[];
}

export interface IDeleteRecordingsCB extends IIdempotencyResponse {
	recordingsDelete: string[];
}

export class RPCDeleteRecordings extends RPCMethod {
	public Send(payload: IDeleteRecordingsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteRecordings';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteRecordingsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteRecordingsCB): boolean {
		
		if (payload.recordingsDelete && payload.recordingsDelete.length > 0) {
			// Default action
			store.commit('DeleteRecordingsRemote', payload.recordingsDelete);
		}
		
		return true;
	}
}

export default {};
