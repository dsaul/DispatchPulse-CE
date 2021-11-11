

export default (): boolean => {
	return window.matchMedia('(max-width: 500px)').matches;
};
