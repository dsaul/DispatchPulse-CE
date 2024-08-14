import store from "@/plugins/store/store";

export default (): void => {
	localStorage.removeItem("SessionUUID");
	store.commit("reset");

	window.location.reload();
};
