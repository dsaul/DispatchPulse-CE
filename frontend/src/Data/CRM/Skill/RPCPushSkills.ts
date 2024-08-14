import { RPCMethod } from "@/RPC/RPCMethod";
import IIdempotencyResponse from "@/RPC/IIdempotencyResponse";
import IIdempotencyRequest from "@/RPC/IIdempotencyRequest";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";
import { ISkill } from "./Skill";

export interface IPushSkillsPayload extends IIdempotencyRequest {
	skills: Record<string, ISkill>;
}

export interface IPushSkillsCB extends IIdempotencyResponse {
	skills: string[];
}

export class RPCPushSkills extends RPCMethod {
	public Send(payload: IPushSkillsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return "PushSkills";
	}
	public GetClientCallbackMethodName(): string | null {
		return "PushSkillsCB";
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest,
		payload: IPushSkillsCB
	): boolean {
		if (!payload.skills) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error deleting skills #2.`)
				);
			}
			return false;
		}

		// Default action

		return true;
	}
}

export default {};
