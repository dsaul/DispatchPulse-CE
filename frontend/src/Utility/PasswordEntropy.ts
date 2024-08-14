// Typescript port of https://github.com/dsaul/fast-password-entropy/blob/master/src/index.js

interface PasswordEntropyCharset {
	name: string;
	re: RegExp;
	length: number;
}

export default abstract class PasswordEntropy {
	/**
	 * Calculate the given password entropy.
	 *
	 * @param   {string} string is the password string.
	 * @returns {number}        the calculated entropy.
	 */
	public static Determine(string: string): number {
		return string
			? PasswordEntropy.CalcEntropy(
					PasswordEntropy.calcCharsetLength(string),
					string.length
					// eslint-disable-next-line no-mixed-spaces-and-tabs
			  )
			: 0;
	}

	/**
	 * Standard character sets list.
	 *
	 * It assumes the `uppercase` and `lowercase` charsets to have 26 chars as in
	 * the English alphabet. Numbers are 10 characters long. Symbols are the rest
	 * of the 33 remaining chars in the 7-bit ASCII table.
	 *
	 * @type {Array}
	 */
	private static stdCharsets: PasswordEntropyCharset[] = [
		{
			name: "lowercase",
			re: /[a-z]/, // abcdefghijklmnopqrstuvwxyz
			length: 26
		},
		{
			name: "uppercase",
			re: /[A-Z]/, // ABCDEFGHIJKLMNOPQRSTUVWXYZ
			length: 26
		},
		{
			name: "numbers",
			re: /[0-9]/, // 1234567890
			length: 10
		},
		{
			name: "symbols",
			re: /[^a-zA-Z0-9]/, //  !"#$%&'()*+,-./:;<=>?@[\]^_`{|}~ (and any other)
			length: 33
		}
	];

	/**
	 * Helper function to calculate the total charset lengths of a given string
	 * using the standard character sets.
	 *
	 * @type {Function}
	 */
	private static calcCharsetLength = PasswordEntropy.calcCharsetLengthWith(
		PasswordEntropy.stdCharsets
	);

	/**
	 * Calculate the entropy of a string based on the size of the charset used and
	 * the length of the string.
	 *
	 * Based on:
	 * http://resources.infosecinstitute.com/password-security-complexity-vs-length/
	 *
	 * @param   {number} charset is the size of the string charset.
	 * @param   {number} length  is the length of the string.
	 * @returns {number}         the calculated entropy.
	 */

	private static CalcEntropy(charset: number, length: number): number {
		return Math.round((length * Math.log(charset)) / Math.LN2);
	}

	/**
	 * Creates a function to calculate the total charset length of a string based on
	 * the given charsets.
	 *
	 * @param  {Object[]} charsets are description of each charset. Shall contain a
	 *                             regular expression `re` to identify each
	 *                             character and a `length` with the total possible
	 *                             characters in the set.
	 * @returns {Function}         a function that will receive a string and return
	 *                             the total charset length.
	 */
	private static calcCharsetLengthWith(
		charsets: PasswordEntropyCharset[]
	): (str: string) => number {
		return (string: string) =>
			charsets.reduce(
				(length, charset) =>
					length + (charset.re.test(string) ? charset.length : 0),
				0
			);
	}
}
