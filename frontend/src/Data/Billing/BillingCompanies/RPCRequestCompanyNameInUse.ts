import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export interface IRequestCompanyNameInUse extends IIdempotencyRequest {
	abbreviation: string;
}

export interface IRequestCompanyNameInUseCB extends IIdempotencyResponse {
	inUse: boolean;
}

export class RPCRequestCompanyNameInUse extends RPCMethod {
	public Send(payload: IRequestCompanyNameInUse): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestCompanyNameInUse';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestCompanyNameInUseCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestCompanyNameInUseCB): boolean {
		
		if (!payload.hasOwnProperty('inUse')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting company name in use #2.`));
			}
			return false;
		}
	
		// Default action
	
	
		
		return true;
	}
}

export default {};
