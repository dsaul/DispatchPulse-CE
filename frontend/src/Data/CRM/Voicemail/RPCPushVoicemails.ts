import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IVoicemail } from './Voicemail';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushVoicemailsPayload extends IIdempotencyRequest {
	voicemails: { [id: string]: IVoicemail; };
}

export interface IPushVoicemailsCB extends IIdempotencyResponse {
	voicemails: string[];
}

export class RPCPushVoicemails extends RPCMethod {
	public Send(payload: IPushVoicemailsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushVoicemails';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushVoicemailsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushVoicemailsCB): boolean {
		
		if (!payload.hasOwnProperty('voicemails')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying voicemails #2.`));
			}
			return false;
		}

		// Default action
		//store.commit('UpdateVoicemailRemote', payload.voicemails);
		
		return true;
	}
}

export default {};
