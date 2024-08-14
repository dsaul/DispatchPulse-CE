import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { ISettingsDefault } from './SettingsDefault';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestSettingsDefaultPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestSettingsDefaultCB extends IIdempotencyResponse {
	settingsDefault: Record<string, ISettingsDefault>;
}

export class RPCRequestSettingsDefault extends RPCMethod {
	public Send(payload: IRequestSettingsDefaultPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestSettingsDefault';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestSettingsDefaultCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestSettingsDefaultCB): boolean {
		
		if (!payload.settingsDefault) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting settings default #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateSettingsDefaultRemote', payload.settingsDefault);
		
		return true;
	}
}

export default {};
