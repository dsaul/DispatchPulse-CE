
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import GenerateID from '@/Utility/GenerateID';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection, { IRoundTripRequest } from '@/RPC/SignalRConnection';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import PerformLocalLogout from '@/Utility/PerformLocalLogout';

export class RPCMethod {
	
	public static Register<T extends RPCMethod>(method: T): T {
		
		// If this is called too early, wait.
		if (SignalRConnection === undefined || SignalRConnection.Connection == null) {
			setTimeout(() => this.Register<T>(method), 500);
			return method;
		}
		
		
		const callbackMethodName = method.GetClientCallbackMethodName();
		
		if (!callbackMethodName || IsNullOrEmpty(callbackMethodName)) {
			console.error('!callbackMethodName || IsNullOrEmpty(callbackMethodName)');
			return method;
		}
		
		SignalRConnection.Connection.on(callbackMethodName, (params) => method.Recieve(params));
		
		return method;
	}
	
	public GetServerMethodName(): string | null {
		return null;
	}
	
	public GetClientCallbackMethodName(): string | null {
		return null;
	}
	
	public Send(payload: IIdempotencyRequest): IRoundTripRequest {
		
		

		const serverMethodName = this.GetServerMethodName();
		
		if ((window as any).DEBUG_PRINT_REQUESTS) {
			console.debug(`$RPC Send: ${serverMethodName}`, payload);
		}
		
		
		if (!payload.hasOwnProperty('roundTripRequestId')) {
			payload.roundTripRequestId = GenerateID();
		}
	
		const ret: IRoundTripRequest = {
			roundTripRequestId: payload.roundTripRequestId as string,
			outboundRequestPromise: null,
			completeRequestPromise: null,
			_completeRequestPromiseResolve: null,
			_completeRequestPromiseReject: null,
		};

		if (!serverMethodName || IsNullOrEmpty(serverMethodName)) {
			ret.outboundRequestPromise = Promise.reject(new Error('No Server Method Name!'));
			ret.completeRequestPromise = Promise.reject(new Error('No Server Method Name!'));
			return ret;
		}
		
		if (SignalRConnection.Connection == null) {
			ret.outboundRequestPromise = Promise.reject(new Error('Not connected!'));
			ret.completeRequestPromise = Promise.reject(new Error('Not connected!'));
			return ret;
		}
	
		if (GetDemoMode()) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve();
			return ret;
		}
	
		// if (!payload.hasOwnProperty('sessionId')) {
		// 	payload.sessionId = BillingSessions.CurrentSessionId();
		// }
	
		// if (IsNullOrEmpty(payload.sessionId)) {
		// 	ret.outboundRequestPromise = Promise.reject(Error('No session.'));
		// 	ret.completeRequestPromise = Promise.reject(Error('No session.'));
		// 	return ret;
		// }
	
		if (!payload.hasOwnProperty('idempotencyToken')) {
			payload.idempotencyToken = GenerateID();
		}
	
		ret.outboundRequestPromise = SignalRConnection.Connection.invoke(
			serverMethodName, payload);
	
	
		ret.completeRequestPromise = new Promise((resolve, reject) => {
			ret._completeRequestPromiseResolve = resolve;
			ret._completeRequestPromiseReject = reject;
		});
	
		ret.outboundRequestPromise.catch((err) => {
			Notifications.AddNotification({
				severity: 'error',
				message: err.message,
				autoClearInSeconds: 10,
			});
		});
		ret.outboundRequestPromise.then(() => {
			//
		});
	
		ret.outboundRequestPromise.catch((err) => {
			console.error(`${serverMethodName} Network Error`, 
				SignalRConnection.Connection?.state , err.toString());
			
			if (ret && ret._completeRequestPromiseReject) {
				ret._completeRequestPromiseReject(err);
			}
		});
	
		ret.completeRequestPromise.catch((err) => {
			Notifications.AddNotification({
				severity: 'error',
				message: err.message,
				autoClearInSeconds: 10,
			});
			
			throw err;
		});
		
		SignalRConnection.Requests[ret.roundTripRequestId] = ret;
	
		return ret;
		
		
	}
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IIdempotencyResponse): boolean { 
		return false;
	}
	
	public Recieve(payload: IIdempotencyResponse): void {
		
		const cbName = this.GetClientCallbackMethodName();
		
		if ((window as any).DEBUG_PRINT_REQUESTS) {
			console.debug(`$RPC Recieve: ${cbName}`, payload);
		}
		
		if (GetDemoMode()) {
			return;
		}
	
		if (payload.forceLogout) {
			PerformLocalLogout();
			return;
		}
	
		//console.log('RequestAgentsCB', payload);
	
		const request = SignalRConnection.Requests[payload.roundTripRequestId];
	
		if (payload.isError) {
			
			if (request && request._completeRequestPromiseReject) {
				request._completeRequestPromiseReject(
					new Error(`Error: ${payload.errorMessage}`));
			}
			return;
		}
		
		// Default action.
		if (this.RecieveDefaultAction) {
			if (false === this.RecieveDefaultAction(request, payload)) {
				return;
			}
		}
		
		if (request && request._completeRequestPromiseResolve) {
			request._completeRequestPromiseResolve(payload);
		}
	
		delete SignalRConnection.Requests[payload.roundTripRequestId];
		
		
	}
	
	
	
	
	
	
	
	
}




export default {};
