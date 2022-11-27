import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { ISettingsUser } from './SettingsUser';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestSettingsUserPayload = IIdempotencyRequest
	
	export interface IRequestSettingsUserCB extends IIdempotencyResponse {
		settingsUser: Record<string, ISettingsUser>;
	}

export class RPCRequestSettingsUser extends RPCMethod {
	public Send(payload: IRequestSettingsUserPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestSettingsUser';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestSettingsUserCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestSettingsUserCB): boolean {
		
		if (!payload.settingsUser) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting settings user #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateSettingsUserRemote', payload.settingsUser);
		
		return true;
	}
}

export default {};
