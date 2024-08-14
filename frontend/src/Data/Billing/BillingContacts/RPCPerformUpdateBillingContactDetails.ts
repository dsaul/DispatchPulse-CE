import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import { guid } from "@/Utility/GlobalTypes";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";
import { IBillingContacts } from "./BillingContacts";

export interface IPerformUpdateBillingContactDetails
	extends IIdempotencyRequest {
	billingContacts: Record<string, IBillingContacts>;
}

export interface IPerformUpdateBillingContactDetailsCB
	extends IIdempotencyResponse {
	billingContacts: guid[];
}

export class RPCPerformUpdateBillingContactDetails extends RPCMethod {
	public Send(
		payload: IPerformUpdateBillingContactDetails
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PerformUpdateBillingContactDetails";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PerformUpdateBillingContactDetailsCB";
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPerformUpdateBillingContactDetailsCB
	): boolean {
		return true;
	}
}

export default {};
