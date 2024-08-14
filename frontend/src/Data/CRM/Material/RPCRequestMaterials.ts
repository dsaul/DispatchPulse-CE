import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { guid } from "@/Utility/GlobalTypes";
import { IMaterial } from "./Material";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestMaterialsPayload extends IIdempotencyRequest {
	limitToIds?: guid[] | null;
	limitToProjectId?: string | null;
	showChildrenOfProjectIdAsWell?: boolean | null;
}

export interface IRequestMaterialsCB extends IIdempotencyResponse {
	materials: Record<string, IMaterial>;
}

export class RPCRequestMaterials extends RPCMethod {
	public Send(payload: IRequestMaterialsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestMaterials";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestMaterialsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestMaterialsCB
	): boolean {
		if (!payload.materials) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting materials #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit("UpdateMaterialsRemote", payload.materials);

		return true;
	}
}

export default {};
