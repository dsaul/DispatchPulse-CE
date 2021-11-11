/* tslint:disable:no-bitwise */
/**
 * Generate a 36 character UUID for use. 
 */
import { guid } from '@/Utility/GlobalTypes';

export default (): guid => {
	let d = new Date().getTime();
	
	if (window.performance && typeof window.performance.now === 'function') {
		d += window.performance.now(); //use high-precision timer if available
	}

	const uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
		const r = (d + Math.random() * 16) % 16 | 0;
		d = Math.floor(d / 16);
		return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
	});
	return uuid;
};
