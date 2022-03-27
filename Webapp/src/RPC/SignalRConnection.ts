import * as signalR from '@microsoft/signalr';
import store from '@/plugins/store/store';
import { DateTime } from 'luxon';
import DownloadJSON from '@/Utility/DownloadJSON';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import { Notifications } from '@/Data/Models/Notifications/Notifications';

interface ICreateBackupTask {
	id: string;
	sessionId: string;
	tzIANA: string;
}

interface ICreateBackupTaskCB {
	id: string;
	backupContent: string;
}

export interface IRoundTripRequest {
	roundTripRequestId: string;
	outboundRequestPromise: Promise<any> | null;
	completeRequestPromise: Promise<any> | null;
	_completeRequestPromiseResolve: ((value: any) => void) | null;
	_completeRequestPromiseReject: ((reason: Error) => void) | null;
}

class SignalRConnection {
	
	public LatexTasks: any = {};
	public BackupTasks: any = {};
	
	
	public Requests: { [id: string]: IRoundTripRequest } = {};
	
	
	
	
	
	//public apiRoot: string = 'https://localhost:44320';
	//public apiRoot: string = 'https://api-staging.dispatchpulse.com';
	//public apiRoot = 'https://api-v2021-07-10.dispatchpulse.com';
	public apiRoot: string | null = null;
	
	private connection: signalR.HubConnection | null = null;
	
	constructor() {
		//super();

		window.fetch('/env/API_ROOT')
			.then((response) => {
				console.log(response);

				// once we have the api root we can connect to signalr

				this.connection = new signalR.HubConnectionBuilder()
					.withUrl(`${this.apiRoot}/APIHub`)
					.configureLogging(signalR.LogLevel.Information)
					.withAutomaticReconnect({
						nextRetryDelayInMilliseconds: (retryContext) => {
							if (retryContext.elapsedMilliseconds < (1000 * 60 * 60 * 60 * 24)) {
								// If we've been reconnecting for less than 1 day so far,
								// wait between 0 and 10 seconds before the next reconnect attempt.
								const waitMilis = Math.random() * 10000;
								
								// Wait an amount of time equal to the amount we've waited so far/2.
								// Allows ever increasing wait time. Min wait time 1s, max 60s.
								// const waitMilis = Clamp(retryContext.elapsedMilliseconds / 2, 1000, 1000 * 60); 
								
								
								// console.debug(`Waiting ${Math.round( waitMilis / 1000 * 10 ) / 10} seconds before reconnecting.`);
								
								Notifications.AddNotification({
									severity: 'warning',
									message: `Disconnected. Waiting ${Math.round( waitMilis / 1000 * 10 ) / 10} seconds before reconnecting.`,
									autoClearInSeconds: 10,
								});
								
								return waitMilis;
							} else {
								console.error(`We've tried reconnecting for a day. Reload the app to try connecting again.`);
								
								Notifications.AddNotification({
									severity: 'error',
									message: `We've tried reconnecting for a day. Reload the app to try connecting again.`,
								});
								
								
								// If we've been reconnecting for more than 1 day so far, stop reconnecting.
								return null;
							}
						},
					})
					.build();

				this.Start();
				
				this.connection.onreconnecting(() => {
					console.debug('onreconnecting');
				});
				
				this.connection.onclose(async () => {
					await this.Start();
				});
				
				
				
				
				this.connection.on('CreateBackupTaskCB', (params: any) => this.CreateBackupTaskCB(params));

				//console.log(this.connection);
			})
			.catch((error) => {
				console.error('error');
			})
		
		
		
		
	}
	
	public Start(): Promise<void> {
		if (null == this.connection) {
			console.error('null == this.connection');
			return Promise.reject();
		}

		const promise = this.connection.start().then(() => {
			//console.log('connected');
			

		}).catch((err: Error) => {
			
			let errorText = 'Unknown Error';
			
			if (err && err.toString() === 'Error') {
				errorText = 'Unable to connect to API. Retrying in 5 seconds...';
			} else if (err) {
				errorText = `API Connection Error: ${err.toString()}`;
			}
			
			Notifications.AddNotification({
				severity: 'warning',
				message: errorText,
				autoClearInSeconds: 10,
			});
			
			
			setTimeout(() => this.Start(), 5000);
			
			//console.error(store, err, err.toString());
		});
		
		
		return promise;
	}
	
	public get Connection() {
		return this.connection;
	} 
	
	
	public Ready(fn: () => void): void {
		
		if (null == this.connection || this.connection.state !== 'Connected') {
			setTimeout(() => this.Ready(fn), 500);
			return;
		}
		
		fn();
	}
	
	
	
	
	
	
	
	
	
	// 
	// Get Server Backup
	// 
	
	//BackupTasks
	
	public CreateBackupTask(payload: ICreateBackupTask): Promise<any> {
		
		if (null == this.connection) {
			console.error('null == this.connection');
			return Promise.reject();
		}

		if (GetDemoMode()) {
			return Promise.resolve();
		}
		
		//console.debug('CreateBackupTask', (payload as any).texcontent);
		
		if (!payload) {
			console.error('!payload');
			return Promise.reject('!payload');
		}
		if (!payload.hasOwnProperty('sessionId')) {
			payload.sessionId = store.state.Sessions.sessionId;
		}
		if (!payload.hasOwnProperty('tzIANA')) {
			payload.tzIANA = DateTime.local().zoneName;
		}
		
		return this.connection.invoke('CreateBackupTask', payload)
			.then(() => {
				
				// 
				
			})
			.catch((err: Error) => {
				return console.error('CreateBackupTask Network Error', this.connection?.state , err.toString());
			});
	}
	
	public CreateBackupTaskCB(payload: ICreateBackupTaskCB): void {
		if (GetDemoMode()) {
			return;
		}
		
		console.debug('CreateBackupTaskCB', payload);
		
		
		const json = JSON.stringify(payload.backupContent, null, '\t');
		
		
		DownloadJSON(json, `${payload.id}.json`);
		
		
		
	}
	
	
	
	
	
	
	
	
	
	
	
}

const singleton = new SignalRConnection();

export default singleton;
