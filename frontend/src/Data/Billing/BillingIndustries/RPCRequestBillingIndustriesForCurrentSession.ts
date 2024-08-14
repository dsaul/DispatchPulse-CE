import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IBillingIndustries } from "./BillingIndustries";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingIndustriesForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingIndustriesForCurrentSessionCB
	extends IIdempotencyResponse {
	billingIndustries: IBillingIndustries[];
}

export class RPCRequestBillingIndustriesForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingIndustriesForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingIndustriesForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingIndustriesForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingIndustriesForCurrentSessionCB
	): boolean {
		if (!payload.billingIndustries) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting billing industries #2.`)
				);
			}
			return false;
		}

		// Default action
		const newPayload: Record<string, IBillingIndustries> = {};

		for (const e of payload.billingIndustries) {
			newPayload[e.uuid] = e;
		}

		store.commit("UpdateBillingIndustriesRemote", newPayload);

		return true;
	}
}

export default {};
