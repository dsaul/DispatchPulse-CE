import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { guid } from "@/Utility/GlobalTypes";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRunReportMaterialsPayload extends IIdempotencyRequest {
	runOnAllMaterials: boolean;
	projectId: guid | null;
}

export interface IRunReportMaterialsCB extends IIdempotencyResponse {
	taskId: guid | null;
}

export class RPCRunReportMaterials extends RPCMethod {
	public Send(payload: IRunReportMaterialsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RunReportMaterials";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RunReportMaterialsCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRunReportMaterialsCB
	): boolean {
		return true;
	}
}

export default {};
