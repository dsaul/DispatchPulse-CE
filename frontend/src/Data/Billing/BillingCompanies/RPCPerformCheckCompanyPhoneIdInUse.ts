import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformCheckCompanyPhoneIdInUse extends IIdempotencyRequest {
	companyId: string | null;
}

export interface IPerformCheckCompanyPhoneIdInUseCB extends IIdempotencyResponse {
	inUse: boolean;
}

export class RPCPerformCheckCompanyPhoneIdInUse extends RPCMethod {
	public Send(payload: IPerformCheckCompanyPhoneIdInUse): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformCheckCompanyPhoneIdInUse';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformCheckCompanyPhoneIdInUseCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformCheckCompanyPhoneIdInUseCB): boolean {
		
		// Default action
	
	
		
		return true;
	}
}

export default {};
