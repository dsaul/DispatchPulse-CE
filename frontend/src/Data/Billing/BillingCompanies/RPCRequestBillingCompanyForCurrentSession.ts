import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IBillingCompanies } from "./BillingCompanies";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingCompanyForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingCompanyForCurrentSessionCB
	extends IIdempotencyResponse {
	billingCompany: IBillingCompanies;
}
export class RPCRequestBillingCompanyForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingCompanyForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingCompanyForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingCompanyForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingCompanyForCurrentSessionCB
	): boolean {
		if (!payload.billingCompany) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting billing companies #2.`)
				);
			}
			return false;
		}

		// Default action
		const id = payload.billingCompany.uuid;

		const newPayload: Record<string, IBillingCompanies> = {};
		newPayload[id] = payload.billingCompany;

		store.commit("UpdateBillingCompaniesRemote", newPayload);

		return true;
	}
}

export default {};
