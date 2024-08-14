import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { ISettingsProvisioning } from "./SettingsProvisioning";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestSettingsProvisioningPayload
	extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestSettingsProvisioningCB extends IIdempotencyResponse {
	settingsProvisioning: Record<string, ISettingsProvisioning>;
}

export class RPCRequestSettingsProvisioning extends RPCMethod {
	public Send(
		payload: IRequestSettingsProvisioningPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestSettingsProvisioning";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestSettingsProvisioningCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestSettingsProvisioningCB
	): boolean {
		if (!payload.settingsProvisioning) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting settings provisioning #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit(
			"UpdateSettingsProvisioningRemote",
			payload.settingsProvisioning
		);

		return true;
	}
}

export default {};
