import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import store from '@/plugins/store/store';
import { IBillingJournalEntriesType } from './BillingJournalEntriesType';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';


export type IRequestBillingJournalEntriesTypeForCurrentSessionPayload = IIdempotencyRequest
	
export interface IRequestBillingJournalEntriesTypeForCurrentSessionCB extends IIdempotencyResponse {
	billingJournalEntriesType: IBillingJournalEntriesType[];
}

export class RPCRequestBillingJournalEntriesTypeForCurrentSession extends RPCMethod {
	public Send(payload: IRequestBillingJournalEntriesTypeForCurrentSessionPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestBillingJournalEntriesTypeForCurrentSession';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestBillingJournalEntriesTypeForCurrentSessionCB';
	}
	public RecieveDefaultAction(
		rtr: IRoundTripRequest, 
		payload: IRequestBillingJournalEntriesTypeForCurrentSessionCB,
		): boolean {
		
		if (!payload.billingJournalEntriesType) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting journal entries type #2.`));
			}
			return false;
		}
	
		// Default action
		const newPayload: Record<string, IBillingJournalEntriesType> = {};
		
		for (const e of payload.billingJournalEntriesType) {
			newPayload[e.uuid] = e;
		}
		
		store.commit('UpdateBillingJournalEntriesTypeRemote', newPayload);
	
		return true;
	}
}

export default {};
