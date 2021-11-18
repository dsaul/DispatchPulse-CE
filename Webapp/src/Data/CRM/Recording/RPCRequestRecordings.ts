import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { guid } from '@/Utility/GlobalTypes';
import { IRecording } from './Recording';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestRecordingsPayload extends IIdempotencyRequest {
	limitToIds?: guid[] | null;
	limitToProjectId?: string | null;
	showChildrenOfProjectIdAsWell?: boolean | null;
}

export interface IRequestRecordingsCB extends IIdempotencyResponse {
	recordings: { [id: string]: IRecording; };
}

export class RPCRequestRecordings extends RPCMethod {
	public Send(payload: IRequestRecordingsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestRecordings';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestRecordingsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestRecordingsCB): boolean {
		
		if (!payload.recordings) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting recordings #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateRecordingsRemote', payload.recordings);
		
		return true;
	}
}

export default {};
