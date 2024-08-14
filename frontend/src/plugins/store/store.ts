import Vue from "vue";
import Vuex from "vuex";
import Sessions from "./Sessions";
import Database from "./Database";
import Notifications from "./Notifications";
import VuexReset from "@ianwalter/vuex-reset";

Vue.use(Vuex);

const store: any = new Vuex.Store({
	plugins: [VuexReset()],
	state: {
		openDialogues: {},
		dialogueModelStates: {},
		drawers: {
			showNavigation: window.innerWidth > 960
		},
		demoMode: false
	},
	mutations: {
		// A no-op mutation must be added to serve as a trigger for a reset. The
		// name of the trigger mutation defaults to 'reset' but can be specified
		// in options, e.g. VuexReset({ trigger: 'data' }).
		reset: () => {
			//
		},

		SetModelState(state: any, payload: { name: string; state: any }) {
			Vue.set(state.dialogueModelStates, payload.name, payload.state);
		},

		SetDemoMode(state: any, payload: boolean) {
			Vue.set(state, "demoMode", payload);
		}
	},
	modules: {
		Sessions,
		Database,
		Notifications
	}
});

(window as any).DEBUG_store = store;

export default store;
