import { Component, Vue  } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import ComponentBase from '@/Components/ComponentBase/ComponentBase';
import { NavigationGuardNext, Route } from 'vue-router/types/router';

Component.registerHooks([
	'beforeRouteEnter',
	'beforeRouteLeave',
	'beforeRouteUpdate',
]);

@Component({
	components: {
		
	},
})
export default class ViewBase extends ComponentBase {
	
	public connectionMonitorInterval: ReturnType<typeof setTimeout> | null = null;
	// We put connected in here so it doesn't show the error by default.
	protected connectionStatus: string | null = 'Connected';
	
	// public beforeRouteEnter(to: any, from: any, next: any) {
	// 	// called before the route that renders this component is confirmed.
	// 	// does NOT have access to `this` component instance,
	// 	// because it has not been created yet when this guard is called!
	// 	console.log('beforeRouteEnter');
		
	// 	next();
	// }
	
	public beforeRouteUpdate(to: Route, from: Route, next: NavigationGuardNext): void {
		// called when the route that renders this component has changed,
		// but this component is reused in the new route.
		// For example, for a route with dynamic params `/foo/:id`, when we
		// navigate between `/foo/1` and `/foo/2`, the same `Foo` component instance
		// will be reused, and this hook will be called when that happens.
		// has access to `this` component instance.
		
		//console.log('asd');
		
		next();
		
		
		
		requestAnimationFrame(() => {
			const oldName = from.name as string;
			const newName = to.name as string;
			
			if (oldName !== newName)
				return;
			if (from.params.id === to.params.id)
				return;
			if (!to.query.tab)
				return;
			if (Array.isArray(to.query.tab)) {
				let stop = true;
				for (const o of to.query.tab) {
					if (!IsNullOrEmpty(o)) {
						stop = false;
						break;
					}
				}
				if (stop) {
					return;
				}
			} else {
				if (!IsNullOrEmpty(to.query.tab)) {
					this.SwitchToTabFromRoute();
				}
			}
			
			
		});
		
		
	}
	
	public ReloadFromLogin(): void {
		//console.log('ReloadFromLogin');
		this.ReLoadData();
	}
	
	protected mounted(): void {
		
		this.connectionMonitorInterval = setInterval(() => {
			if (SignalRConnection.Connection == null) {
				Vue.set(this, 'connectionStatus', false);
			} else {
				Vue.set(this, 'connectionStatus', SignalRConnection.Connection.state);
			}
		}, 1000);
		
		
		this.MountedAfter();
	}
	
	protected destroyed(): void {
		
		if (this.connectionMonitorInterval) {
			clearInterval(this.connectionMonitorInterval);
			this.connectionMonitorInterval = null;
		}
		
		this.DestroyedAfter();
	}
	
	// public beforeRouteLeave(to: any, from: any, next: any) {
	// 	// called when the route that renders this component is about to
	// 	// be navigated away from.
	// 	// has access to `this` component instance.
		
	// 	next();
	// }
	
	protected SwitchToTabFromRoute(): void {
		// this.$refs.editor.SwitchToTabFromRoute();
	}
	
	protected ReLoadData(): void {
		//
	}
	
	protected MountedAfter(): void {
		//
	}
	
	protected DestroyedAfter(): void {
		//
	}
	
}
