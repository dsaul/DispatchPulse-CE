import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";
import { ISettingsUser } from "./SettingsUser";

export interface IPushSettingsUserPayload extends IIdempotencyRequest {
	settingsUser: Record<string, ISettingsUser>;
}

export interface IPushSettingsUserCB extends IIdempotencyResponse {
	settingsUser: string[];
}

export class RPCPushSettingsUser extends RPCMethod {
	public Send(payload: IPushSettingsUserPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushSettingsUser";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushSettingsUserCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushSettingsUserCB
	): boolean {
		if (!payload.settingsUser) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error deleting settings user #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
