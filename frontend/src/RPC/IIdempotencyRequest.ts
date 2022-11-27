import IHubResponse from '@/RPC/IHubRequest';
import { guid } from '@/Utility/GlobalTypes';

export default interface IIdempotencyRequest extends IHubResponse {
	sessionId: guid | null | undefined;
	idempotencyToken?: guid;
	roundTripRequestId?: guid;
}
