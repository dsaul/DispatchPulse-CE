import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { ICompany } from "./Company";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushCompaniesPayload extends IIdempotencyRequest {
	companies: Record<string, ICompany>;
}

export interface IPushCompaniesCB extends IIdempotencyResponse {
	companies: string[];
}

export class RPCPushCompanies extends RPCMethod {
	public Send(payload: IPushCompaniesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushCompanies";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushCompaniesCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushCompaniesCB
	): boolean {
		if (!payload.companies) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying companies #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
