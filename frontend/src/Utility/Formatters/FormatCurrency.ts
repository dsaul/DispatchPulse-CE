export default (amount: number, currency: string): string => {
	if (undefined === amount || null === amount) {
		return "";
	}

	return `$${amount.toFixed(2)} ${currency}`.trim();
};
