import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { ILabourType } from "./LabourType";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IRequestLabourTypesPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestLabourTypesCB extends IIdempotencyResponse {
	labourTypes: { [id: string]: ILabourType };
}

export class RPCRequestLabourTypes extends RPCMethod {
	public Send(payload: IRequestLabourTypesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestLabourTypes";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestLabourTypesCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestLabourTypesCB
	): boolean {
		if (!payload.labourTypes) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting labour types #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit("UpdateLabourTypesRemote", payload.labourTypes);

		return true;
	}
}

export default {};
