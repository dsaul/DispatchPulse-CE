import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteSkillsPayload extends IIdempotencyRequest {
	skillsDelete: string[];
}

export interface IDeleteSkillsCB extends IIdempotencyResponse {
	skillsDelete: string[];
}

export class RPCDeleteSkills extends RPCMethod {
	public Send(payload: IDeleteSkillsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteSkills';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteSkillsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteSkillsCB): boolean {
		
		if (payload.skillsDelete && payload.skillsDelete.length > 0) {
			// Default action
			store.commit('DeleteSkillsRemote', payload.skillsDelete);
		}
		
		return true;
	}
}

export default {};
