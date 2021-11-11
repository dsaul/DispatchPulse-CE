import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { ISettingsDefault } from './SettingsDefault';


export interface IPushSettingsDefaultPayload extends IIdempotencyRequest {
	settingsDefault: Record<string, ISettingsDefault>;
}

export interface IPushSettingsDefaultCB extends IIdempotencyResponse {
	settingsDefault: string[];
}

export class RPCPushSettingsDefault extends RPCMethod {
	public Send(payload: IPushSettingsDefaultPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushSettingsDefault';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushSettingsDefaultCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushSettingsDefaultCB): boolean {
		
		if (!payload.settingsDefault) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying settings default #2.`));
			}
			return false;
		}
	
		// Default action
		
		
		return true;
	}
}

export default {};
