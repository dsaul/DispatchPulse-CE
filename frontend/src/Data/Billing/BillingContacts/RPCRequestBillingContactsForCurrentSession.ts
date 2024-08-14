import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IBillingContacts } from "./BillingContacts";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingContactsForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingContactsForCurrentSessionCB
	extends IIdempotencyResponse {
	billingContacts: IBillingContacts[];
}

export class RPCRequestBillingContactsForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingContactsForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingContactsForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingContactsForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingContactsForCurrentSessionCB
	): boolean {
		if (!payload.billingContacts) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting billing contacts #2.`)
				);
			}
			return false;
		}

		// Default action
		const newPayload: Record<string, IBillingContacts> = {};

		for (const e of payload.billingContacts) {
			newPayload[e.uuid] = e;
		}

		store.commit("UpdateBillingContactsRemote", newPayload);

		return true;
	}
}

export default {};
