import IHubResponse from '@/RPC/IHubResponse';

export default interface IIdempotencyResponse extends IHubResponse {
	idempotencyToken: string;
	roundTripRequestId: string;
}
