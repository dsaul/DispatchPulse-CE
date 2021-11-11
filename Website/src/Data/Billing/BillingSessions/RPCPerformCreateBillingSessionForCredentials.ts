import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformCreateBillingSessionForCredentialsPayload extends IIdempotencyRequest {
	companyAbbreviation: string;
	contactEMail: string;
	contactPassword: string;
	tzIANA?: string;
}
interface IPerformCreateBillingSessionForCredentialsCB extends IIdempotencyResponse {
	sessionId: string;
}

export class RPCPerformCreateBillingSessionForCredentials extends RPCMethod {
	public Send(payload: IPerformCreateBillingSessionForCredentialsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformCreateBillingSessionForCredentials';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformCreateBillingSessionForCredentialsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformCreateBillingSessionForCredentialsCB): boolean {
		
		if (payload.sessionId && !IsNullOrEmpty(payload.sessionId)) {
			store.commit('SetSession', payload.sessionId);
			localStorage.setItem('SessionUUID', payload.sessionId);
		}
		
		return true;
	}
}

export default {};
