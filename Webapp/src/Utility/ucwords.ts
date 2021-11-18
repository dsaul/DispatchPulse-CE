/**
 * Make the first letter of each word uppercase.
 * @param {String} str Input string.
 */
export default (str: string): string => {
	return (str + '').replace(/^([a-z])|\s+([a-z])/g, ($1) => {
		return $1.toUpperCase();
	});
};
