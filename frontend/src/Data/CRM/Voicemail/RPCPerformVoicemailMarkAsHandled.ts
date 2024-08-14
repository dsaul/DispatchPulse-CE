import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { guid } from "@/Utility/GlobalTypes";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformVoicemailMarkAsHandledPayload
	extends IIdempotencyRequest {
	sessionId: guid;
	voicemailId: guid;
}

export type IPerformVoicemailMarkAsHandledCB = IIdempotencyResponse;

export class RPCPerformVoicemailMarkAsHandled extends RPCMethod {
	public Send(
		payload: IPerformVoicemailMarkAsHandledPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformVoicemailMarkAsHandled";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformVoicemailMarkAsHandledCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformVoicemailMarkAsHandledCB
	): boolean {
		return true;
	}
}

export default {};
