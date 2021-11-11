import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IBillingSessions } from './BillingSessions';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestBillingSessionsForCurrentSessionPayload = IIdempotencyRequest
	
	export interface IRequestBillingSessionsForCurrentSessionCB extends IIdempotencyResponse {
		billingSessions: IBillingSessions[];
	}

export class RPCRequestBillingSessionsForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingSessionsForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingSessionsForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingSessionsForCurrentSessionCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestBillingSessionsForCurrentSessionCB): boolean {
		
		if (!payload.billingSessions) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting sessions #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingSessions> = {};
		
		for (const e of payload.billingSessions) {
			newPayload[e.uuid] = e;
		}
		
		store.commit('UpdateBillingSessionsRemote', newPayload);
		
		return true;
	}
}

export default {};
