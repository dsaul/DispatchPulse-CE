import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IVoicemail } from './Voicemail';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestVoicemailsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestVoicemailsCB extends IIdempotencyResponse {
	voicemails: { [id: string]: IVoicemail; };
}

export class RPCRequestVoicemails extends RPCMethod {
	public Send(payload: IRequestVoicemailsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestVoicemails';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestVoicemailsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestVoicemailsCB): boolean {
		
		if (!payload.hasOwnProperty('voicemails')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(new Error(`Error requesting autoAttendants #2.`));
				
			}
			
			return false;
		}

		// Default action
		store.commit('UpdateVoicemailsRemote', payload.voicemails);
		
		return true;
	}
}

export default {};
