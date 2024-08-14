// https://stackoverflow.com/questions/12484386/access-javascript-property-case-insensitively

export default (object: Record<string, any>, key: string): any => {
	if (!object) {
		return null;
	}

	if (key === undefined) {
		console.error("CaseInsensitivePropertyGet key === undefined");
		return null;
	}

	if (key === null) {
		console.error("CaseInsensitivePropertyGet key === null");
		return null;
	}

	if (!key.toLowerCase) {
		console.error(
			"CaseInsensitivePropertyGet !key.toLowerCase, supplied key doesn't appear to be a string?",
			key
		);
		return null;
	}

	//console.log(Object.keys(object), key);

	//console.log(object);

	const foundKey: string | null =
		Object.keys(object).find(o => {
			if (!o.toLowerCase) {
				console.warn(
					"iterating keys, found one that doesn't appear to be a string, saying it doesn't match",
					o
				);
				return false;
			}

			return o.toLowerCase() === key.toLowerCase();
		}) || null;
	//console.log(foundKey);

	if (!foundKey) {
		return null;
	}
	return object[foundKey] as any;
};
