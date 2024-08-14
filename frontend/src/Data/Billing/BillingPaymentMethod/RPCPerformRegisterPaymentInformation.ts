import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformRegisterPaymentInformation
	extends IIdempotencyRequest {
	paymentFrequency: string;
	numberOfSeats: number;
	currency: string;
}

export interface IPerformRegisterPaymentInformationCB
	extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPerformRegisterPaymentInformation extends RPCMethod {
	public Send(
		payload: IPerformRegisterPaymentInformation
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformRegisterPaymentInformation";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformRegisterPaymentInformationCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformRegisterPaymentInformationCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error registering payment information #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
