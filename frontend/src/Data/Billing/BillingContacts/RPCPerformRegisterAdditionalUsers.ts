import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";
import { IRegisterAdditionalUser } from "@/Data/Models/RegisterDialogueModelState/RegisterDialogueModelState";

export interface IPerformRegisterAdditionalUsers extends IIdempotencyRequest {
	otherAccountsToAdd: IRegisterAdditionalUser[];
}

export interface IPerformRegisterAdditionalUsersCB
	extends IIdempotencyResponse {
	created: boolean;
}

export class RPCPerformRegisterAdditionalUsers extends RPCMethod {
	public Send(payload: IPerformRegisterAdditionalUsers): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformRegisterAdditionalUsers";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformRegisterAdditionalUsersCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformRegisterAdditionalUsersCB
	): boolean {
		if (payload.isError) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(
						`Error registering additional users: ${payload.errorMessage}`
					)
				);
			}
			return false;
		}

		if (!payload.hasOwnProperty("created")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error registering additional users #2.`)
				);
			}
			return false;
		}

		return true;
	}
}

export default {};
