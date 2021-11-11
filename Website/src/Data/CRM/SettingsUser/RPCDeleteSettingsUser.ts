import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteSettingsUserPayload extends IIdempotencyRequest {
	settingsUserDelete: string[];
}

export interface IDeleteSettingsUserCB extends IIdempotencyResponse {
	settingsUserDelete: string[];
}

export class RPCDeleteSettingsUser extends RPCMethod {
	public Send(payload: IDeleteSettingsUserPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteSettingsUser';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteSettingsUserCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteSettingsUserCB): boolean {
		
		if (payload.settingsUserDelete && payload.settingsUserDelete.length > 0) {
			// Default action
			store.commit('DeleteSettingsUserRemote', payload.settingsUserDelete);
		}
		
		return true;
	}
}

export default {};
