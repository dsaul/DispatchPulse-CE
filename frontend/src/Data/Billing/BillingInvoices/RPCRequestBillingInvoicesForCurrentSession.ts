import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IBillingInvoices } from "./BillingInvoices";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingInvoicesForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingInvoicesForCurrentSessionCB
	extends IIdempotencyResponse {
	billingInvoices: IBillingInvoices[];
}

export class RPCRequestBillingInvoicesForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingInvoicesForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingInvoicesForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingInvoicesForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingInvoicesForCurrentSessionCB
	): boolean {
		if (!payload.billingInvoices) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting invoices #2.`)
				);
			}
			return false;
		}

		// Default action
		const newPayload: Record<string, IBillingInvoices> = {};

		for (const e of payload.billingInvoices) {
			newPayload[e.uuid] = e;
		}

		store.commit("UpdateBillingInvoicesRemote", newPayload);

		return true;
	}
}

export default {};
