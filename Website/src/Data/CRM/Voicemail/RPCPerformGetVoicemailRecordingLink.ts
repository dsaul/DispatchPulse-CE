import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';
import { guid } from '@/Utility/GlobalTypes';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export interface IPerformGetVoicemailRecordingLinkPayload extends IIdempotencyRequest {
	sessionId: guid;
	voicemailId: guid;
	billingCompanyId: guid;
}

export interface IPerformGetVoicemailRecordingLinkCB extends IIdempotencyResponse {
	voicemailURI: string;
}

export class RPCPerformGetVoicemailRecordingLink extends RPCMethod {
	public Send(payload: IPerformGetVoicemailRecordingLinkPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'PerformGetVoicemailRecordingLink';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'PerformGetVoicemailRecordingLinkCB';
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IPerformGetVoicemailRecordingLinkCB): boolean {
		return true;
	}
}

export default {};
