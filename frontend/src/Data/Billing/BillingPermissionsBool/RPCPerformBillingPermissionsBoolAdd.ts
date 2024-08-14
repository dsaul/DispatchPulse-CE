import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPerformBillingPermissionsBoolAdd extends IIdempotencyRequest {
	billingContactId: string | null;
	permissionKeys: string[] | null;
}

export interface IPerformBillingPermissionsBoolAddCB
	extends IIdempotencyResponse {
	billingPermissionsBool: string | null;
}

export class RPCPerformBillingPermissionsBoolAdd extends RPCMethod {
	public Send(payload: IPerformBillingPermissionsBoolAdd): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformBillingPermissionsBoolAdd";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformBillingPermissionsBoolAddCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformBillingPermissionsBoolAddCB
	): boolean {
		// Default action

		return true;
	}
}

export default {};
