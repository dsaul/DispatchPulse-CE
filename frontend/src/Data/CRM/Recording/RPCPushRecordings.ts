import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRecording } from './Recording';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushRecordingsPayload extends IIdempotencyRequest {
	recordings: { [id: string]: IRecording; };
}

export interface IPushRecordingsCB extends IIdempotencyResponse {
	recordings: string[];
}

export class RPCPushRecordings extends RPCMethod {
	public Send(payload: IPushRecordingsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushRecordings';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushRecordingsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushRecordingsCB): boolean {
		
		if (!payload.recordings) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying recordings #2.`));
			}
			return false;
		}
	
		// Default action
		
	
		
		return true;
	}
}

export default {};
