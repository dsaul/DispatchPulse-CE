
export default (val: string): boolean | string => {
	return (val && val.length >= 2) || 'Must be greater than 2 letters';
};


