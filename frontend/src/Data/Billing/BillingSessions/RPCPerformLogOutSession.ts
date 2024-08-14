import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import PerformLocalLogout from '@/Utility/PerformLocalLogout';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IPerformLogOutSessionPayload = IIdempotencyRequest

interface IPerformLogOutSessionCB extends IIdempotencyResponse {
	loggedOut: boolean;
}

export class RPCPerformLogOutSession extends RPCMethod {
	public Send(payload: IPerformLogOutSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformLogOutSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformLogOutSessionCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformLogOutSessionCB): boolean {
		
		
		// Default action
		
		if (payload.loggedOut) {
			PerformLocalLogout();
		}
		
		
		
		return true;
	}
}

export default {};
