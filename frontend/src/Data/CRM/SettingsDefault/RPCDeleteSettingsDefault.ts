import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteSettingsDefaultPayload extends IIdempotencyRequest {
	settingsDefaultDelete: string[];
}

export interface IDeleteSettingsDefaultCB extends IIdempotencyResponse {
	settingsDefaultDelete: string[];
}

export class RPCDeleteSettingsDefault extends RPCMethod {
	public Send(payload: IDeleteSettingsDefaultPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteSettingsDefault";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteSettingsDefaultCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteSettingsDefaultCB
	): boolean {
		if (
			payload.settingsDefaultDelete &&
			payload.settingsDefaultDelete.length > 0
		) {
			// Default action
			store.commit(
				"DeleteSettingsDefaultRemote",
				payload.settingsDefaultDelete
			);
		}

		return true;
	}
}

export default {};
