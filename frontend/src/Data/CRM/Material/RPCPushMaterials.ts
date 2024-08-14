import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IMaterial } from './Material';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPushMaterialsPayload extends IIdempotencyRequest {
	materials: Record<string, IMaterial>;
}

export interface IPushMaterialsCB extends IIdempotencyResponse {
	materials: string[];
}
export class RPCPushMaterials extends RPCMethod {
	public Send(payload: IPushMaterialsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PushMaterials';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PushMaterialsCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPushMaterialsCB): boolean {
		
		if (!payload.materials) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error modifying materials #2.`));
			}
			return false;
		}
	
		// Default action
		
	
		
		return true;
	}
}

export default {};
