import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformRegisterMainUser extends IIdempotencyRequest {
	contactId: string | null;

	fullName: string | null;
	passwordHash: string | null;
	eMailMarketing: boolean;
	eMailTutorials: boolean;
}

export interface IPerformRegisterMainUserCB extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPerformRegisterMainUser extends RPCMethod {
	public Send(payload: IPerformRegisterMainUser): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformRegisterMainUser";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformRegisterMainUserCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformRegisterMainUserCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error registering main user #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
