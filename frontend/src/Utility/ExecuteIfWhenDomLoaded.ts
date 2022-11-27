/**
 * Execute a function when the document is done loading
 * @param {function} fn A callback function.
 */
export default (fn: EventListenerOrEventListenerObject | (() => void)): void => {
	if (
		document.readyState === 'complete' ||
		(document.readyState !== 'loading' && !(document.documentElement as any).doScroll)
	) {
		(fn as (() => void))();
	} else {
		document.addEventListener('DOMContentLoaded', fn as EventListenerOrEventListenerObject);
	}
};
