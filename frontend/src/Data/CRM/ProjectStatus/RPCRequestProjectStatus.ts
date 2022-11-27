import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { IProjectStatus } from './ProjectStatus';


export interface IRequestProjectStatusPayload extends IIdempotencyRequest {
	limitToIds?: string[] | null;
}

export interface IRequestProjectStatusCB extends IIdempotencyResponse {
	projectStatus: Record<string, IProjectStatus>;
}

export class RPCRequestProjectStatus extends RPCMethod {
	public Send(payload: IRequestProjectStatusPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestProjectStatus';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestProjectStatusCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestProjectStatusCB): boolean {
		
		if (!payload.projectStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting project status #2.`));
			}
			return false;
		}
	
		// Default action
		store.commit('UpdateProjectStatusRemote', payload.projectStatus);
		
		return true;
	}
}

export default {};
