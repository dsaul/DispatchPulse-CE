import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformRegisterSaveServiceAgreement
	extends IIdempotencyRequest {
	agreementText: string;
	signatureSVG: string;
}

export interface IPerformRegisterSaveServiceAgreementCB
	extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPerformRegisterSaveServiceAgreement extends RPCMethod {
	public Send(
		payload: IPerformRegisterSaveServiceAgreement
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformRegisterSaveServiceAgreement";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformRegisterSaveServiceAgreementCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformRegisterSaveServiceAgreementCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error saving service agreement #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
