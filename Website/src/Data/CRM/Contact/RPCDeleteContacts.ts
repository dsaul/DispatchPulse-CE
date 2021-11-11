import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IDeleteContactsPayload extends IIdempotencyRequest {
	contactsDelete: string[];
}

export interface IDeleteContactsCB extends IIdempotencyResponse {
	contactsDelete: string[];
}

export class RPCDeleteContacts extends RPCMethod {
	public Send(payload: IDeleteContactsPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'DeleteContacts';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'DeleteContactsCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IDeleteContactsCB): boolean {
		
		if (payload.contactsDelete && payload.contactsDelete.length > 0) {
			// Default action
			store.commit('DeleteContactsRemote', payload.contactsDelete);
		}
		
		return true;
	}
}

export default {};
