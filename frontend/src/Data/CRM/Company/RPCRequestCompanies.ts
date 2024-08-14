import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { ICompany } from './Company';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IRequestCompaniesPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestCompaniesCB extends IIdempotencyResponse {
	companies: Record<string, ICompany>;
}

export class RPCRequestCompanies extends RPCMethod {
	public Send(payload: IRequestCompaniesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestCompanies';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestCompaniesCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestCompaniesCB): boolean {
		
		if (!payload.companies) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting companies #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateCompaniesRemote', payload.companies);
		
		return true;
	}
}

export default {};
