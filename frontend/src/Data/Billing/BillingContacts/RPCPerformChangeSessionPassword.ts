import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformChangeSessionPassword extends IIdempotencyRequest {
	currentPassword: string;
	newHash: string;
}

export interface IPerformChangeSessionPasswordCB extends IIdempotencyResponse {
	passwordChanged: boolean;
}

export class RPCPerformChangeSessionPassword extends RPCMethod {
	public Send(payload: IPerformChangeSessionPassword): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformChangeSessionPassword";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformChangeSessionPasswordCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformChangeSessionPasswordCB
	): boolean {
		// Default action

		return true;
	}
}

export default {};
