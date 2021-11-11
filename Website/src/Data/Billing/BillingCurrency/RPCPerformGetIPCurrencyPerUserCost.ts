import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export type IPerformGetIPCurrencyPerUserCost = IIdempotencyRequest
	
	export interface IPerformGetIPCurrencyPerUserCostCB extends IIdempotencyResponse {
		ip: string | null;
		currency: string | null;
		perUserCost: number | null;
	}

export class RPCPerformGetIPCurrencyPerUserCost extends RPCMethod {
	public Send(payload: IPerformGetIPCurrencyPerUserCost): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformGetIPCurrencyPerUserCost';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformGetIPCurrencyPerUserCostCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformGetIPCurrencyPerUserCostCB): boolean {
		
		if (!payload.ip) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`IP and currency information didn't include ip.`));
			}
			return false;
		}
	
		if (!payload.currency) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`IP and currency information didn't include currency.`));
			}
			return false;
		}
	
		if (!payload.perUserCost) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`IP and currency information didn't include per user cost.`));
			}
			return false;
		}
	
		// Default action
	
		
		return true;
	}
}

export default {};
