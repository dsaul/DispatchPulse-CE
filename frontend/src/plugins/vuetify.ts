import Vue from "vue";
import Vuetify from "vuetify";
//import 'vuetify/src/stylus/app.styl';
import "vuetify/src/styles/main.sass";
import "@mdi/font/css/materialdesignicons.css";
//import 'font-awesome/css/font-awesome.min.css';
import "@fortawesome/fontawesome-free/css/all.min.css";

Vue.use(Vuetify);

export default new Vuetify({
	icons: {
		iconfont: "md"
	},
	theme: {
		themes: {
			light: {
				primary: "#f87560",
				secondary: "#424242",
				accent: "#82B1FF",
				error: "#FF5252",
				info: "#2196F3",
				success: "#4CAF50",
				warning: "#FFC107"
			}
		}
	}
});
