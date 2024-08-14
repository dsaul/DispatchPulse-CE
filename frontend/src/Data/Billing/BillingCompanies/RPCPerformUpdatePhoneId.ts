import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IPerformUpdatePhoneId extends IIdempotencyRequest {
	newPhoneId: string | null;
}

export interface IPerformUpdatePhoneIdCB extends IIdempotencyResponse {
	saved: boolean;
}

export class RPCPerformUpdatePhoneId extends RPCMethod {
	public Send(payload: IPerformUpdatePhoneId): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformUpdatePhoneId';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformUpdatePhoneIdCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformUpdatePhoneIdCB): boolean {
		
		// Default action
		
		return true;
	}
}

export default {};
