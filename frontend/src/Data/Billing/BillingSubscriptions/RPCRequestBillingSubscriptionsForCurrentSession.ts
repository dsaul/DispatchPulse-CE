import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IBillingSubscriptions } from "./BillingSubscriptions";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingSubscriptionsForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingSubscriptionsForCurrentSessionCB
	extends IIdempotencyResponse {
	billingSubscriptions: IBillingSubscriptions[];
}

export class RPCRequestBillingSubscriptionsForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingSubscriptionsForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingSubscriptionsForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingSubscriptionsForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingSubscriptionsForCurrentSessionCB
	): boolean {
		if (!payload.billingSubscriptions) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting subscriptions #2.`)
				);
			}
			return false;
		}

		// Default action
		const newPayload: Record<string, IBillingSubscriptions> = {};

		for (const e of payload.billingSubscriptions) {
			newPayload[e.uuid] = e;
		}

		store.commit("UpdateBillingSubscriptionsRemote", newPayload);

		return true;
	}
}

export default {};
