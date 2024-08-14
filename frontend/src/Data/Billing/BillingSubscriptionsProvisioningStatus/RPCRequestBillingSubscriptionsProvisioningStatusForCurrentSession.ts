import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import store from "@/plugins/store/store";
import { IBillingSubscriptionsProvisioningStatus } from "./BillingSubscriptionsProvisioningStatus";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export type IRequestBillingSubscriptionsProvisioningStatusForCurrentSessionPayload = IIdempotencyRequest;

export interface IRequestBillingSubscriptionsProvisioningStatusForCurrentSessionCB
	extends IIdempotencyResponse {
	billingSubscriptionsProvisioningStatus: IBillingSubscriptionsProvisioningStatus[];
}

export class RPCRequestBillingSubscriptionsProvisioningStatusForCurrentSession extends RPCMethod {
	public Send(
		payload: IRequestBillingSubscriptionsProvisioningStatusForCurrentSessionPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestBillingSubscriptionsProvisioningStatusForCurrentSession";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestBillingSubscriptionsProvisioningStatusForCurrentSessionCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestBillingSubscriptionsProvisioningStatusForCurrentSessionCB
	): boolean {
		if (!payload.billingSubscriptionsProvisioningStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(
						`Error requesting subscriptions provisioning status #2.`
					)
				);
			}
			return false;
		}

		// Default action
		const newPayload: Record<
			string,
			IBillingSubscriptionsProvisioningStatus
		> = {};

		for (const e of payload.billingSubscriptionsProvisioningStatus) {
			newPayload[e.uuid] = e;
		}

		store.commit(
			"UpdateBillingSubscriptionsProvisioningStatusRemote",
			newPayload
		);

		return true;
	}
}

export default {};
