import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IBillingPermissionsBool } from "./BillingPermissionsBool";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingPermissionsBoolForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingPermissionsBoolForCurrentSessionCB
	extends IIdempotencyResponse {
	billingPermissionsBool: IBillingPermissionsBool[];
}

export class RPCRequestBillingPermissionsBoolForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingPermissionsBoolForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingPermissionsBoolForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingPermissionsBoolForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingPermissionsBoolForCurrentSessionCB
	): boolean {
		if (!payload.billingPermissionsBool) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting permissions #2.`)
				);
			}
			return false;
		}

		// Default action
		const newPayload: Record<string, IBillingPermissionsBool> = {};

		for (const e of payload.billingPermissionsBool) {
			newPayload[e.uuid] = e;
		}

		store.commit("UpdateBillingPermissionsBoolRemote", newPayload);

		return true;
	}
}

export default {};
