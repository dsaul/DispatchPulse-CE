/**
 * Check if a string is null or empty.
 * @param {String} str string to check.
 * @return {boolean}
 */
export default (str: string | null | undefined): boolean => {
	if (typeof str === 'undefined') {
		return true;
	}
	if (undefined === str) {
		return true;
	}
	if (null === str) {
		return true;
	}
	if (str.length && 0 === str.length) {
		return true;
	}
	if (str.trim && '' === str.trim()) {
		return true;
	}
	return false;
};
