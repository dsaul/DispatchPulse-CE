

export default (val: string): boolean | string => {
	return !!val || 'Name is required';
};

