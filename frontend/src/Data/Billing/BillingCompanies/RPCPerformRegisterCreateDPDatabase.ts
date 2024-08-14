import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export type IPerformRegisterCreateDPDatabase = IIdempotencyRequest
	
	export interface IPerformRegisterCreateDPDatabaseCB extends IIdempotencyResponse {
		created: boolean;
	}

export class RPCPerformRegisterCreateDPDatabase extends RPCMethod {
	public Send(payload: IPerformRegisterCreateDPDatabase): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformRegisterCreateDPDatabase';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformRegisterCreateDPDatabaseCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformRegisterCreateDPDatabaseCB): boolean {
		
		if (!payload.hasOwnProperty('created')) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error creating dispatch pulse database #2.`));
			}
			return false;
		}
		
		return true;
	}
}

export default {};
