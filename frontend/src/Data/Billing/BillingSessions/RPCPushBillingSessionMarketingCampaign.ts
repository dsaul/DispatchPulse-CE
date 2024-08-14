import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IPushBillingSessionMarketingCampaignPayload
	extends IIdempotencyRequest {
	marketingCampaign: string | null;
}

export interface IPushBillingSessionMarketingCampaignCB
	extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPushBillingSessionMarketingCampaign extends RPCMethod {
	public Send(
		payload: IPushBillingSessionMarketingCampaignPayload
	): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushBillingSessionMarketingCampaign";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushBillingSessionMarketingCampaignCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushBillingSessionMarketingCampaignCB
	): boolean {
		if (!payload.hasOwnProperty("saved")) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error saving marketing campaign #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
