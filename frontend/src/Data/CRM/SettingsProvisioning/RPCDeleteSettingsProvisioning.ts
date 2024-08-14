import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteSettingsProvisioningPayload
	extends IIdempotencyRequest {
	settingsProvisioningDelete: string[];
}

export interface IDeleteSettingsProvisioningCB extends IIdempotencyResponse {
	settingsProvisioningDelete: string[];
}

export class RPCDeleteSettingsProvisioning extends RPCMethod {
	public Send(
		payload: IDeleteSettingsProvisioningPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteSettingsProvisioning";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteSettingsProvisioningCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteSettingsProvisioningCB
	): boolean {
		if (
			payload.settingsProvisioningDelete &&
			payload.settingsProvisioningDelete.length > 0
		) {
			// Default action
			store.commit(
				"DeleteSettingsProvisioningRemote",
				payload.settingsProvisioningDelete
			);
		}

		return true;
	}
}

export default {};
