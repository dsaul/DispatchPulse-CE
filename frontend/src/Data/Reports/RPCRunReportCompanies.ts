import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { guid } from "@/Utility/GlobalTypes";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRunReportCompaniesPayload extends IIdempotencyRequest {
	runOnAllCompanies: boolean;
	companyIds: guid[];
}

export interface IRunReportCompaniesCB extends IIdempotencyResponse {
	taskId: guid | null;
}

export class RPCRunReportCompanies extends RPCMethod {
	public Send(payload: IRunReportCompaniesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RunReportCompanies";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RunReportCompaniesCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRunReportCompaniesCB
	): boolean {
		return true;
	}
}

export default {};
