
import store from '@/plugins/store/store';
import GenerateID from '@/Utility/GenerateID';
import { guid } from '@/Utility/GlobalTypes';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { DateTime } from 'luxon';

export interface INotification {
	id?: guid;
	severity: 'error' | 'info' | 'warning' | 'success';
	message: string;
	timestamp?: DateTime;
	autoClearInSeconds?: number;
}

export class Notifications {
	
	public static AddNotification(notification: INotification): void {
		
		if (!notification.id) {
			notification.id = GenerateID();
		}
		if (!notification.timestamp) {
			notification.timestamp = DateTime.local();
		}
		if (notification.autoClearInSeconds === undefined) {
			notification.autoClearInSeconds = -1;
		}
		
		if (notification.autoClearInSeconds >= 0) {
			setTimeout(() => {
				store.commit('ClearId', { id: notification.id });
			}, notification.autoClearInSeconds * 1000);
		}
		
		store.commit('AddNotification', notification);
	}
	
	public static ClearAllNotifications(): void {
		store.commit('ClearAllNotifications', {});
	}
	
	public static ClearIndex(index: number): void {
		store.commit('ClearIndex', { index });
	}
	
	public static ClearId(id: guid | null | undefined): void {
		if (!id || IsNullOrEmpty(id)) {
			return;
		}
		
		store.commit('ClearId', { id });
	}
	
	
}


(window as any).DEBUG_Notifications = Notifications;

export default {};
