import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IBillingPackagesType } from "./BillingPackagesType";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingPackagesTypeForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingPackagesTypeForCurrentSessionCB
	extends IIdempotencyResponse {
	billingPackagesType: IBillingPackagesType[];
}

export class RPCRequestBillingPackagesTypeForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingPackagesTypeForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingPackagesTypeForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingPackagesTypeForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingPackagesTypeForCurrentSessionCB
	): boolean {
		if (!payload.billingPackagesType) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting package types #2.`)
				);
			}
			return false;
		}

		// Default action
		const newPayload: Record<string, IBillingPackagesType> = {};

		for (const e of payload.billingPackagesType) {
			newPayload[e.uuid] = e;
		}

		store.commit("UpdateBillingPackagesTypeRemote", newPayload);

		return true;
	}
}

export default {};
