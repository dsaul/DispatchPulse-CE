import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IBillingJournalEntries } from "./BillingJournalEntries";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingJournalEntriesForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingJournalEntriesForCurrentSessionCB
	extends IIdempotencyResponse {
	billingJournalEntries: IBillingJournalEntries[];
}

export class RPCRequestBillingJournalEntriesForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingJournalEntriesForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingJournalEntriesForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingJournalEntriesForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingJournalEntriesForCurrentSessionCB
	): boolean {
		if (!payload.billingJournalEntries) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting journal entries #2.`)
				);
			}
			return false;
		}

		// Default action
		const newPayload: Record<string, IBillingJournalEntries> = {};

		for (const e of payload.billingJournalEntries) {
			newPayload[e.uuid] = e;
		}

		store.commit("UpdateBillingJournalEntriesRemote", newPayload);

		return true;
	}
}

export default {};
