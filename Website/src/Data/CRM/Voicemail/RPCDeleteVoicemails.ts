import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteVoicemailsPayload extends IIdempotencyRequest {
	voicemailsDelete: string[];
}

export interface IDeleteVoicemailsCB extends IIdempotencyResponse {
	voicemailsDelete: string[];
}

export class RPCDeleteVoicemails extends RPCMethod {
	public Send(payload: IDeleteVoicemailsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteVoicemails';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteVoicemailsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteVoicemailsCB): boolean {
		
		
		if (payload.voicemailsDelete && payload.voicemailsDelete.length > 0) {
			// Default action
			store.commit('DeleteVoicemailsRemote', payload.voicemailsDelete);
		}
		
		
		return true;
	}
}

export default {};
