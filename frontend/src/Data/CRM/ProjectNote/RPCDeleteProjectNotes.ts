import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteProjectNotesPayload extends IIdempotencyRequest {
	projectNotesDelete: string[];
}

export interface IDeleteProjectNotesCB extends IIdempotencyResponse {
	projectNotesDelete: string[];
}

export class RPCDeleteProjectNotes extends RPCMethod {
	public Send(payload: IDeleteProjectNotesPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteProjectNotes';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteProjectNotesCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteProjectNotesCB): boolean {
		
		if (payload.projectNotesDelete && payload.projectNotesDelete.length > 0) {
			// Default action
			store.commit('DeleteProjectNotesRemote', payload.projectNotesDelete);
		}
		
		return true;
	}
}

export default {};
