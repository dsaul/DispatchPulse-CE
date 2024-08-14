import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDeleteCompaniesPayload extends IIdempotencyRequest {
	companiesDelete: string[];
}

export interface IDeleteCompaniesCB extends IIdempotencyResponse {
	companiesDelete: string[];
}

export class RPCDeleteCompanies extends RPCMethod {
	public Send(payload: IDeleteCompaniesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "DeleteCompanies";
	}
	public GetClientCallbackMethodName(): string | null {
		return "DeleteCompaniesCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IDeleteCompaniesCB
	): boolean {
		if (payload.companiesDelete && payload.companiesDelete.length > 0) {
			// Default action
			store.commit("DeleteCompaniesRemote", payload.companiesDelete);
		}

		return true;
	}
}

export default {};
