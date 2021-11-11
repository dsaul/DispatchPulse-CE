
import { Vue } from 'vue-property-decorator';
import { guid } from '@/Utility/GlobalTypes';
import _ from 'lodash';
import { INotification } from '@/Data/Models/Notifications/Notifications';

export default {
	state: {
		notifications: [],
	},
	getters: {
		
	},
	mutations: {
		// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
		AddNotification(state: any, payload: INotification) {
			
			console.debug('AddNotification', state, payload);
			
			const mod = state.notifications as INotification[];
			mod.push(payload);
			
			Vue.set(state, 'notifications', mod);
			
		},
		
		// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
		ClearAllNotifications(state: any) {
			//console.debug('ClearAllNotifications()');
			
			Vue.set(state, 'notifications', []);
		},
		
		// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
		ClearIndex(state: any, payload: { index: number }) {
			//console.debug('ClearIndex()', payload.index);
			
			const mod = state.notifications as INotification[];
			mod.splice(payload.index, 1);
			Vue.set(state, 'notifications', mod);
		},
		
		// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
		ClearId(state: any, payload: { id: guid}) {
			
			//console.debug('ClearId()', payload.id);
			
			const mod = state.notifications as INotification[];
			
			_.remove(mod, (value) => {
				return value.id === payload.id;
			});
			
			
			Vue.set(state, 'notifications', mod);
			
		},
		
		
		
		
		
	},
	actions: {
		
		
		
	},
};
