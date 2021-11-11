
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';

export default (val: string): boolean | string => {
	if (IsNullOrEmpty(val)) {
		return true;
	}
	
	if (val.length < 4) {
		return 'Optional, but must be greater then 4 letters.';
	} else {
		return true;
	}
};
