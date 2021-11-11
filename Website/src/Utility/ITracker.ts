import { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { DateTime } from 'luxon';


export default interface ITracker {
	lastRequestTimeUtc: DateTime;
	isComplete: boolean;
	rtr: IRoundTripRequest;
}
