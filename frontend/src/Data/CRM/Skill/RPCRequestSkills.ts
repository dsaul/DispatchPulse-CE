import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";

import store from "@/plugins/store/store";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";
import { ISkill } from "./Skill";

export interface IRequestSkillsPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestSkillsCB extends IIdempotencyResponse {
	skills: Record<string, ISkill>;
}

export class RPCRequestSkills extends RPCMethod {
	public Send(payload: IRequestSkillsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "RequestSkills";
	}
	public GetClientCallbackMethodName(): string | null {
		return "RequestSkillsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IRequestSkillsCB
	): boolean {
		if (!payload.skills) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting skills #2.`)
				);
			}
			return false;
		}

		// Default action
		store.commit("UpdateSkillsRemote", payload.skills);

		return true;
	}
}

export default {};
