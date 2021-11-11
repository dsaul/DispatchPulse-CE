import Vue from 'vue';
import '@/plugins/vuetify';
import App from './App.vue';
import router from '@/plugins/router';
import store from '@/plugins/store/store';
import './registerServiceWorker';
import vuetify from '@/plugins/vuetify';
import '@/plugins/vue-scroll-sync';
import '@/plugins/vue-the-mask';
import '@/plugins/vue-content-placeholders';
import SignalRConnection from '@/RPC/SignalRConnection';


(window as any).DEBUG_PRINT_REQUESTS = false;

Vue.config.productionTip = false;

// This callback runs before every route change, including on page load.
router.beforeEach((to, from, next) => {
	// This goes through the matched routes from last to first, finding the closest route with a title.
	// eg. if we have /some/deep/nested/route and /some, /deep, and /nested have titles, nested's will be chosen.
	const nearestWithTitle = to.matched.slice().reverse().find((r) => r.meta && r.meta.title);

	// Find the nearest route element with meta tags.
	const nearestWithMeta = to.matched.slice().reverse().find((r) => r.meta && r.meta.metaTags);
	//const previousNearestWithMeta = from.matched.slice().reverse().find((r) => r.meta && r.meta.metaTags);

	// If a route with a title was found, set the document (page) title to that value.
	if (nearestWithTitle) {
		document.title = nearestWithTitle.meta.title;
	}

	// Remove any stale meta tags from the document using the key attribute we set below.
	Array.from(document.querySelectorAll('[data-vue-router-controlled]')).map((el) => {
		(el.parentNode as any).removeChild(el);
	});

	// Skip rendering meta tags if there are none.
	if (!nearestWithMeta) {
		return next();
	}

	// Turn the meta tag definitions into actual elements in the head.
	nearestWithMeta.meta.metaTags.map((tagDef: any) => {
		const tag = document.createElement('meta');

		Object.keys(tagDef).forEach((key) => {
			tag.setAttribute(key, tagDef[key]);
		});

		// We use this to track which meta tags we create, so we don't interfere with other ones.
		tag.setAttribute('data-vue-router-controlled', '');

		return tag;
	})
	// Add the meta tags to the document head.
	.forEach((tag: any) => document.head.appendChild(tag));

	next();
});

//(window as any).DEBUG_STORE = store;
(window as any).DEBUG_SIGNALR = SignalRConnection;
//(window as any).DEBUG_CaseInsensitivePropertyGet = CaseInsensitivePropertyGet;

new Vue({
	router,
	store,
	vuetify,
	render: (h) => h(App),
	}).$mount('#app');
