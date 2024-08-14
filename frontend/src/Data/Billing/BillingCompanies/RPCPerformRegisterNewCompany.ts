import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformRegisterNewCompany extends IIdempotencyRequest {
	name: string | null;
	abbreviation: string | null;
	industry: string | null;
	marketingCampaign: string | null;
	addressLine1: string | null;
	addressLine2: string | null;
	addressCity: string | null;
	addressProvince: string | null;
	addressPostalCode: string | null;
	addressCountry: string | null;

	mainContactEMail: string | null;
	mainContactPhoneNumber: string | null;
}

export interface IPerformRegisterNewCompanyCB extends IIdempotencyResponse {
	created: boolean;
	billingCompanyId: string | null;
	billingContactId: string | null;
	billingSessionId: string | null;
	stripeIntentClientSecret: string | null;
}

export class RPCPerformRegisterNewCompany extends RPCMethod {
	public Send(payload: IPerformRegisterNewCompany): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformRegisterNewCompany";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformRegisterNewCompanyCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformRegisterNewCompanyCB
	): boolean {
		if (!payload.hasOwnProperty("created")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error registering new company #2.`)
				);
			}
			return false;
		}
		if (!payload.hasOwnProperty("billingCompanyId")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error registering new company #3.`)
				);
			}
			return false;
		}
		if (!payload.hasOwnProperty("billingContactId")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error registering new company #4.`)
				);
			}
			return false;
		}
		if (!payload.hasOwnProperty("billingSessionId")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error registering new company #5.`)
				);
			}
			return false;
		}
		if (!payload.hasOwnProperty("stripeIntentClientSecret")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error registering new company #6.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
